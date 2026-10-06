namespace FrostMaze.Simulation
{
    public sealed class Enemy
    {
        public int Id, Checkpoint, BlockerId;
        public V2 Position, Velocity, IntendedDirection;
        public WaveSpec Spec;
        public float Health, AttackCooldown;
        public bool Blocked, Exited;
        public V2 Destination;
    }
}
