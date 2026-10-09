using System;
namespace FrostMaze.Simulation.Online
{
    // Ordered, reliable messages. Session owns lifetime; transport never advances the world.
    public interface IPacketConnection : IDisposable
    {
        bool Closed { get; }
        string Error { get; }
        void Send(Packet packet);
        bool TryRead(out Packet packet);
        void CloseAfterFlush();
    }
    public interface IPacketTransport : IDisposable
    {
        IPacketConnection Server { get; }
        string Failure { get; }
        void Update();
        bool TryAccept(out IPacketConnection connection);
    }
}
