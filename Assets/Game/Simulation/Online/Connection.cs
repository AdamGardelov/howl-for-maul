using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Threading;
namespace FrostMaze.Simulation.Online
{
    // Each peer owns bounded queues. Neither socket reads nor writes can block Unity's main thread.
    public sealed class Connection : IPacketConnection
    {
        readonly TcpClient client;readonly BlockingCollection<byte[]> outgoing=new BlockingCollection<byte[]>(256);
        readonly ConcurrentQueue<Packet> incoming=new ConcurrentQueue<Packet>();
        int pending,closed;public bool Closed=>Volatile.Read(ref closed)!=0;
        public string Error {get;private set;}="";
        public Connection(TcpClient socket){client=socket;client.NoDelay=true;client.SendTimeout=5000;
            new Thread(Read){IsBackground=true,Name="Howl receive"}.Start();new Thread(Write){IsBackground=true,Name="Howl send"}.Start();}
        public void Send(Packet packet){if(Closed)return;var bytes=Protocol.Encode(packet);try{if(!outgoing.TryAdd(bytes))Fail("Connection too slow");}catch(InvalidOperationException){Fail("Disconnected");}}
        public bool TryRead(out Packet p){if(incoming.TryDequeue(out p)){Interlocked.Decrement(ref pending);return true;}return false;}
        public void CloseAfterFlush(){outgoing.CompleteAdding();}
        void Read(){try{var stream=client.GetStream();using(var r=new BinaryReader(stream)){while(!Closed){int n=r.ReadInt32();if(n<=0||n>Protocol.MaxBytes)throw new InvalidDataException("Invalid packet size");var bytes=r.ReadBytes(n);if(bytes.Length!=n)throw new EndOfStreamException();var packet=Protocol.Decode(bytes);if(Interlocked.Increment(ref pending)>256)throw new InvalidDataException("Too many pending messages");incoming.Enqueue(packet);}}}catch(Exception e){Fail(e is EndOfStreamException?"Peer disconnected":e.GetType().Name);}}
        void Write(){try{using(var w=new BinaryWriter(client.GetStream()))foreach(var bytes in outgoing.GetConsumingEnumerable()){w.Write(bytes.Length);w.Write(bytes);w.Flush();}Fail("Disconnected");}catch(Exception e){Fail(e.GetType().Name);}}
        void Fail(string reason){if(Interlocked.Exchange(ref closed,1)!=0)return;Error=reason;outgoing.CompleteAdding();client.Close();}
        public void Dispose(){Fail("Disconnected");}
    }
}
