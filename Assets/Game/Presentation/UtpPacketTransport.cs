using System;
using System.Collections.Generic;
using FrostMaze.Simulation.Online;
using Unity.Networking.Transport;
using Unity.Networking.Transport.Relay;
using Unity.Networking.Transport.Utilities;
namespace FrostMaze
{
    // One reliable, ordered stream for commands AND simulation frames. No lossy state channel.
    public sealed class UtpPacketTransport : IPacketTransport
    {
        NetworkDriver driver;
        NetworkPipeline pipeline;
        readonly Dictionary<NetworkConnection,Link> links=new Dictionary<NetworkConnection,Link>();
        readonly Queue<IPacketConnection> accepted=new Queue<IPacketConnection>();
        readonly bool host,relay;
        bool disposed;
        double clock;
        public IPacketConnection Server {get;private set;}
        public string Failure {get;private set;}="";
        public bool IsDisposed=>disposed;
        public bool Ready=>!disposed&&(!relay||driver.GetRelayConnectionStatus()==RelayConnectionStatus.Established);
        public ushort Port=>driver.GetLocalEndpoint().Port;
        public static UtpPacketTransport Relay(bool host,RelayServerData data)=>new UtpPacketTransport(host,data,default,0,0);
        // Exercises the exact Relay adapter/pipeline locally without cloud credentials.
        public static UtpPacketTransport Local(bool host,ushort port=0,int delayMs=0,int lossPercent=0)=>new UtpPacketTransport(host,null,NetworkEndpoint.LoopbackIpv4.WithPort(port),delayMs,lossPercent);
        UtpPacketTransport(bool isHost,RelayServerData? relayData,NetworkEndpoint endpoint,int delayMs,int loss)
        {
            host=isHost;relay=relayData.HasValue;
            var settings=new NetworkSettings();try{
                settings.WithNetworkConfigParameters(connectTimeoutMS:500,maxConnectAttempts:20,disconnectTimeoutMS:15000,heartbeatTimeoutMS:1000,sendQueueCapacity:256,receiveQueueCapacity:256);
                settings.WithFragmentationStageParameters(payloadCapacity:Protocol.MaxBytes);
                settings.WithReliableStageParameters(windowSize:64);
                if(relay){var data=relayData.Value;settings.WithRelayParameters(ref data);}
                bool simulate=delayMs>0||loss>0;
                if(simulate)settings.WithSimulatorStageParameters(maxPacketCount:256,maxPacketSize:1500,packetDelayMs:delayMs,packetDropPercentage:loss);
                driver=NetworkDriver.Create(settings);
                pipeline=simulate?driver.CreatePipeline(typeof(FragmentationPipelineStage),typeof(ReliableSequencedPipelineStage),typeof(SimulatorPipelineStage)):
                    driver.CreatePipeline(typeof(FragmentationPipelineStage),typeof(ReliableSequencedPipelineStage));
            }
            finally{settings.Dispose();}
            try{
                if(driver.Bind(NetworkEndpoint.AnyIpv4.WithPort(!relay&&host?endpoint.Port:(ushort)0))!=0)throw new InvalidOperationException("Could not bind transport.");
                if(host){if(driver.Listen()!=0)throw new InvalidOperationException("Could not listen.");}
                else {var connection=driver.Connect(relay?relayData.Value.Endpoint:endpoint);if(!connection.IsCreated)throw new InvalidOperationException("Could not connect.");links.Add(connection,new Link(this,connection));}
            }catch{driver.Dispose();disposed=true;throw;}
        }
        public bool TryAccept(out IPacketConnection link){if(accepted.Count>0){link=accepted.Dequeue();return true;}link=null;return false;}
        public void Update()
        {
            if(disposed)return;clock=UnityEngine.Time.realtimeSinceStartupAsDouble;
            driver.ScheduleUpdate().Complete();
            if(relay&&driver.GetRelayConnectionStatus()==RelayConnectionStatus.AllocationInvalid){Failure="Relay connection expired. Create or join a new lobby.";Dispose();return;}
            if(host){NetworkConnection c;while((c=driver.Accept())!=default){if(links.Count>=8){driver.Disconnect(c);continue;}var link=new Link(this,c);links.Add(c,link);accepted.Enqueue(link);}}
            NetworkEvent.Type type;
            int events=0;
            while(events++<2048&&(type=driver.PopEvent(out var connection,out var reader,out var receivedPipeline))!=NetworkEvent.Type.Empty){
                if(!links.TryGetValue(connection,out var link))continue;
                if(type==NetworkEvent.Type.Connect){if(!host)Server=link;}
                else if(type==NetworkEvent.Type.Disconnect){link.MarkClosed("Peer disconnected");if(!host)Failure="Host disconnected or could not be reached. Try a new join code.";}
                else if(type==NetworkEvent.Type.Data){
                    if(receivedPipeline!=pipeline||reader.Length<=0||reader.Length>Protocol.MaxBytes){link.Fail("Invalid network packet");continue;}
                    var data=new byte[reader.Length];for(int i=0;i<data.Length;i++)data[i]=reader.ReadByte();link.Receive(data);
                }
            }
            var dead=new List<NetworkConnection>();
            foreach(var pair in links){pair.Value.Flush();if(pair.Value.Closed)dead.Add(pair.Key);}
            foreach(var c in dead)links.Remove(c);
        }
        public void Dispose(){if(disposed)return;disposed=true;
            foreach(var pair in links){pair.Value.MarkClosed("Disconnected");if(driver.IsCreated)driver.Disconnect(pair.Key);}
            links.Clear();accepted.Clear();if(driver.IsCreated){driver.ScheduleUpdate().Complete();driver.ScheduleFlushSend().Complete();driver.Dispose();}
        }
        sealed class Link : IPacketConnection
        {
            readonly UtpPacketTransport owner;readonly NetworkConnection connection;
            readonly Queue<byte[]> outgoing=new Queue<byte[]>();readonly Queue<Packet> incoming=new Queue<Packet>();
            bool draining;double closeAt=-1;
            public bool Closed {get;private set;}
            public string Error {get;private set;}="";
            public Link(UtpPacketTransport transport,NetworkConnection peer){owner=transport;connection=peer;}
            public void Send(Packet p){if(Closed||draining)return;if(outgoing.Count>=256){Fail("Connection too slow");return;}outgoing.Enqueue(Protocol.Encode(p));}
            public bool TryRead(out Packet p){if(incoming.Count>0){p=incoming.Dequeue();return true;}p=null;return false;}
            public void Receive(byte[] data){if(Closed||draining)return;if(incoming.Count>=256){Fail("Too many pending messages");return;}try{incoming.Enqueue(Protocol.Decode(data));}catch{Fail("Invalid network message");}}
            public void Flush(){
                if(Closed)return;
                for(int count=0;count<64&&outgoing.Count>0;count++){
                    var data=outgoing.Peek();int result=owner.driver.BeginSend(owner.pipeline,connection,out var writer,data.Length);
                    if(result==(int)ErrorCode.NetworkSendQueueFull)return;
                    if(result<0){Fail("Network send failed");return;}
                    foreach(byte b in data)writer.WriteByte(b);
                    if(writer.HasFailedWrites){owner.driver.AbortSend(writer);Fail("Network packet overflow");return;}
                    result=owner.driver.EndSend(writer);
                    if(result==(int)ErrorCode.NetworkSendQueueFull)return;
                    if(result<0){Fail("Network send failed");return;}
                    outgoing.Dequeue();
                }
                // Let the refusal reach the remote player before closing; normal disposal is immediate.
                if(draining&&outgoing.Count==0){if(closeAt<0)closeAt=owner.clock+.5;else if(owner.clock>=closeAt)Dispose();}
            }
            public void CloseAfterFlush(){draining=true;}
            public void MarkClosed(string reason){Closed=true;Error=reason;outgoing.Clear();}
            public void Fail(string reason){if(Closed)return;MarkClosed(reason);if(!owner.disposed)owner.driver.Disconnect(connection);}
            public void Dispose()=>Fail("Disconnected");
        }
        // Alias avoids colliding with the connection's user-facing Error property.
        enum ErrorCode { NetworkSendQueueFull=-5 }
    }
}
