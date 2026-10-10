using UnityEngine;
using FrostMaze.Simulation;
namespace FrostMaze
{
    // Static paint is authored at build time. Geometry and navigation still use the map mask.
    public sealed class WorldSurfaceSet : ScriptableObject
    {
        public string Fingerprint,EditorInputs;
        public Texture2D Ground,Cap,Wall,Water,Exterior;
        public const string PaintRevision="woodland-banks-2";
        public static string Key(Scenario c) {
            var text=PaintRevision+"|"+c.Theme+"|"+c.Width+"|"+c.Height+"|"+c.LayoutCellSize.ToString(System.Globalization.CultureInfo.InvariantCulture)+"|"+c.WalkableSymbols+"|"+string.Join("\n",c.LayoutRows);
            foreach(var lane in c.Lanes)text+="|"+JsonUtility.ToJson(lane);
            using(var sha=System.Security.Cryptography.SHA256.Create())return System.BitConverter.ToString(sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(text))).Replace("-","");
        }
        public static WorldSurfaceSet Find(Scenario c) {
            if(System.Array.IndexOf(System.Environment.GetCommandLineArgs(),"--howl-no-baked-surfaces")>=0)return null;
            var set=Resources.Load<WorldSurfaceSet>("World/Baked/"+(c.Theme=="iron"?"Ironfold":"Rimewatch"));
            return set!=null&&set.Fingerprint==Key(c)&&set.Ground!=null&&set.Cap!=null&&set.Wall!=null&&set.Water!=null&&set.Exterior!=null?set:null;
        }
    }
}
