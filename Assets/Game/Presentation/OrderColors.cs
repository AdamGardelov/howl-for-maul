using UnityEngine;
namespace FrostMaze
{
    // Shared visual vocabulary for painted actor surfaces, cores and their shots.
    // Player ownership colors intentionally remain independent.
    static class OrderColors
    {
        static readonly Color[] ironPigment={new Color(.18f,.43f,.48f),new Color(.43f,.48f,.22f),new Color(.47f,.32f,.62f),new Color(.46f,.63f,.58f),new Color(.39f,.48f,.25f),new Color(.22f,.47f,.46f),new Color(.59f,.20f,.12f),new Color(.18f,.46f,.43f)};
        static readonly Color[] ironLight={new Color(.43f,.80f,.87f),new Color(.87f,.77f,.39f),new Color(.79f,.65f,1),new Color(.83f,.95f,.86f),new Color(.79f,.84f,.46f),new Color(.57f,.89f,.79f),new Color(1,.51f,.20f),new Color(.45f,.86f,.81f)};
        static readonly Color[] winterPigment={new Color(.34f,.63f,.72f),new Color(.43f,.55f,.28f),new Color(.70f,.29f,.11f),new Color(.27f,.46f,.70f)};
        static readonly Color[] winterLight={new Color(.59f,.86f,.95f),new Color(.74f,.83f,.47f),new Color(1,.62f,.25f),new Color(.51f,.74f,1)};
        public static Color Pigment(bool iron,int faction)=>iron?ironPigment[faction]:winterPigment[faction%4];
        public static Color Glow(bool iron,int faction)=>iron?ironLight[faction]:winterLight[faction%4];
    }
}
