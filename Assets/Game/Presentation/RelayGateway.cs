using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
namespace FrostMaze
{
    public sealed class RelayTicket : IDisposable
    {
        public UtpPacketTransport Transport;
        public string Code;
        public void Dispose()=>Transport?.Dispose();
    }
    public interface IRelayGateway
    {
        Task<RelayTicket> Connect(bool host,string code,CancellationToken cancel,Action<string> status);
    }
    public sealed class RelayGateway : IRelayGateway
    {
        static Task signIn;
        public static bool Configured=>Guid.TryParse(Application.cloudProjectId,out _);
        public async Task<RelayTicket> Connect(bool host,string code,CancellationToken cancel,Action<string> status)
        {
            if(!Configured)throw new InvalidOperationException("Online service setup is not complete for this build. Link Howl for Maul to Unity Cloud and enable Authentication and Relay. Solo and LAN are available.");
            cancel.ThrowIfCancellationRequested();status("Signing in to online services…");
            // A cancelled attempt cannot start a competing anonymous sign-in.
            if(signIn==null||signIn.IsFaulted||signIn.IsCanceled)signIn=SignIn();
            await signIn;cancel.ThrowIfCancellationRequested();
            if(!AuthenticationService.Instance.IsSignedIn){signIn=SignIn();await signIn;cancel.ThrowIfCancellationRequested();}
            if(host){
                status("Creating a private online lobby…");
                var allocation=await RelayService.Instance.CreateAllocationAsync(3);cancel.ThrowIfCancellationRequested();
                var joinCode=await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);cancel.ThrowIfCancellationRequested();
                status("Connecting to relay…");
                return new RelayTicket{Code=joinCode,Transport=UtpPacketTransport.Relay(true,allocation.ToRelayServerData("dtls"))};
            }
            status("Finding your friend's lobby…");
            var joined=await RelayService.Instance.JoinAllocationAsync(code);cancel.ThrowIfCancellationRequested();
            status("Connecting to host…");
            return new RelayTicket{Code=code,Transport=UtpPacketTransport.Relay(false,joined.ToRelayServerData("dtls"))};
        }
        static async Task SignIn(){
            if(UnityServices.State!=ServicesInitializationState.Initialized){
                var options=new InitializationOptions();var args=Environment.GetCommandLineArgs();int profile=Array.IndexOf(args,"--howl-auth-profile");
                if(profile>=0&&profile+1<args.Length)options.SetProfile(args[profile+1]);
                await UnityServices.InitializeAsync(options);
            }
            try{
                if(!AuthenticationService.Instance.IsSignedIn)await AuthenticationService.Instance.SignInAnonymouslyAsync();
                await RelayNotices.Fetch();
            }catch(AuthenticationException e){RelayNotices.Set(e.Notifications);throw;}
        }
        public static string Explain(Exception error){
            if(error is RelayServiceException relay){switch(relay.Reason){
                case RelayExceptionReason.JoinCodeNotFound:case RelayExceptionReason.AllocationNotFound:return "That code has expired or is incorrect. Ask the host for a new code.";
                case RelayExceptionReason.InactiveProject:case RelayExceptionReason.Forbidden:return "Online services are not enabled for this game project. Solo and LAN are available.";
                case RelayExceptionReason.RateLimited:return "Too many connection attempts. Wait a moment, then retry.";
                case RelayExceptionReason.Conflict:return "The lobby is full or no longer accepting players. Ask the host for a new lobby.";
                default:return "Could not reach the online lobby. Check your connection and join code, then retry.";
            }}
            if(error is InvalidOperationException)return error.Message;
            return "Online connection failed. Check your internet connection, then retry.";
        }
    }
}
