using UnityEngine;
namespace FrostMaze
{
    // Shared visual vocabulary for painted actor surfaces, cores and their shots.
    // Player ownership colors intentionally remain independent.
    static class OrderColors
    {
        static readonly Color[] ironPigment={new Color(.18f,.43f,.48f),new Color(.35f,.29f,.49f),new Color(.20f,.53f,.47f),new Color(.49f,.36f,.16f),new Color(.42f,.30f,.55f),new Color(.34f,.43f,.24f),new Color(.59f,.20f,.12f),new Color(.18f,.46f,.43f)};
        static readonly Color[] ironLight={new Color(.43f,.80f,.87f),new Color(.68f,.56f,.84f),new Color(.52f,.88f,.77f),new Color(1,.77f,.35f),new Color(.74f,.57f,.94f),new Color(.71f,.82f,.45f),new Color(1,.51f,.20f),new Color(.45f,.86f,.81f)};
        static readonly Color[] winterPigment={new Color(.34f,.63f,.72f),new Color(.43f,.55f,.28f),new Color(.70f,.29f,.11f),new Color(.27f,.46f,.70f)};
        static readonly Color[] winterLight={new Color(.59f,.86f,.95f),new Color(.74f,.83f,.47f),new Color(1,.62f,.25f),new Color(.51f,.74f,1)};
        public static Color Pigment(bool iron,int faction)=>iron?ironPigment[faction]:winterPigment[faction%4];
        public static Color Glow(bool iron,int faction)=>iron?ironLight[faction]:winterLight[faction%4];
    }
}
