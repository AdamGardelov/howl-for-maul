using System;
namespace FrostMaze.Simulation
{
    [Serializable]
    public class TowerSpec
    {
        public int Width = 1, Height = 1;
        public float Fill = 0.86f, Health = 100, Damage = 8, Interval = 0.65f, Range = 3.6f;
        public bool TargetsGround = true, TargetsAir = true;
    }
    public sealed class Tower
    {
        public int Id, CellX, CellY;
        public TowerSpec Spec;
        public float Health, Cooldown;
        public int LastTarget;
        public V2 Center => new V2(CellX + Spec.Width * 0.5f, CellY + Spec.Height * 0.5f);
        public V2 Half => new V2(Spec.Width * 0.5f - (1 - Spec.Fill) * 0.5f, Spec.Height * 0.5f - (1 - Spec.Fill) * 0.5f);
    }
}
