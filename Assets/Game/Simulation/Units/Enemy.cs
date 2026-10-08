namespace FrostMaze.Simulation
{
    public sealed class Enemy
    {
        public int DetourTicks;
        public V2 DetourDirection;
        public float SlowFraction,SlowRemaining;
        public int Id, Checkpoint, BlockerId, Lane;
        public V2 Position, Velocity, IntendedDirection;
        public WaveSpec Spec;
        public float Health, AttackCooldown;
        public bool Blocked, Exited;
        public V2 Destination;
    }
}
