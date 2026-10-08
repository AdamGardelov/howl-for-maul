namespace FrostMaze.Simulation
{
    public sealed class Enemy
    {
        public int DetourTicks;
        public V2 DetourDirection;
        public float SlowFraction,SlowRemaining;
        public int Id, Checkpoint, BlockerId, Lane;
        public V2 Position, Velocity, IntendedDirection;
        // Presentation observes actual melee strikes; these fields do not drive movement or damage.
        public long LastAttackTick=-100;
        public V2 AttackDirection;
        public WaveSpec Spec;
        public float Health, AttackCooldown;
        public bool Blocked, Exited;
        public V2 Destination;
    }
}
