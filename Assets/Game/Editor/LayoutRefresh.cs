using System.IO;
using UnityEditor;
namespace FrostMaze.Editor
{
    public static class LayoutRefresh
    {
        // Explicit authoring action, never an automatic overwrite on load or compilation.
        public static void Refresh()
        {
            foreach(string name in new[]{"Rimewatch","Ironfold"}){
                var asset=AssetDatabase.LoadAssetAtPath<MapDefinition>("Assets/Game/Maps/Resources/"+name+".asset");
                string text=File.ReadAllText("Assets/Game/Maps/LayoutSources/"+name+".txt");
                var layout=name=="Ironfold"?Simulation.ReferenceMaps.Ironfold(text):Simulation.ReferenceMaps.Rimewatch(text);
                var c=asset.Settings;c.LayoutRows=layout.LayoutRows;c.Terrain=layout.Terrain;c.Width=layout.Width;c.Height=layout.Height;
                c.Lanes=layout.Lanes;c.Spawn=layout.Spawn;c.GroundRoute=layout.GroundRoute;c.FlightRoute=layout.FlightRoute;
                c.BuilderStarts=layout.BuilderStarts;c.SoloBuilderStart=layout.SoloBuilderStart;
                EditorUtility.SetDirty(asset);
            }
            AssetDatabase.SaveAssets();
        }
    }
}
