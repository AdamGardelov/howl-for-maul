using FrostMaze.Simulation;
namespace FrostMaze
{
    // Authored proportions, independent of damage, price and the occupied build cell.
    // Short starters, broad siege pieces and tall sky weapons must not share one outline.
    public static class TowerReadability
    {
        static readonly float[] WinterHeights={
            .78f,.80f,1.10f,.90f,1.22f,  // Rime
            .72f,.64f,1.03f,1.15f,1.04f, // Rootbound
            .73f,.85f,.88f,1.10f,1.20f,  // Ember
            .70f,.83f,1.07f,1.22f,1.06f  // Volt
        };
        static readonly float[] IronHeights={
            .86f,.92f,.96f,.88f,1.09f,.94f,1.09f, // Pulse
            .74f,.94f,.83f,1.04f,1.16f,.90f,1.18f, // Blast
            .74f,1.03f,.83f,1.08f,1.21f,.93f,1.12f, // Prism
            .82f,.92f,.89f,1.04f,1.15f,.88f,1.03f, // Horizon
            .75f,.95f,.83f,1.02f,1.19f,.87f,1.14f, // Gravity
            .76f,1.07f,.90f,.86f,1.12f,1.02f,1.18f, // Scrap
            .72f,.87f,.77f,1.05f,1.17f,.86f,1.13f, // Overdrive
            .73f,1.03f,.87f,.95f,1.18f,.97f,1.15f  // Tidal
        };
        public static float Height(Scenario config,int design)
        {
            var heights=config.Theme=="iron"?IronHeights:WinterHeights;
            return design>=0&&design<heights.Length?heights[design]:1;
        }
    }
}
