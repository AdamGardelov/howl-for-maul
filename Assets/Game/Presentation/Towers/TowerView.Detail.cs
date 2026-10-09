using UnityEngine;
namespace FrostMaze
{
    public sealed partial class TowerView
    {
        void DressFoundation(Material trim,Material dark,int faction,bool robot)
        {
            Part("Footing rim",PrimitiveType.Cylinder,new Vector3(0,.19f,0),new Vector3(.91f,.025f,.91f),trim);
            for(int face=-1;face<=1;face+=2) {
            Part("Faction crest backing",PrimitiveType.Cube,new Vector3(0,.115f,face*.45f),new Vector3(.30f,.15f,.025f),dark);
            for(int i=0;i<3;i++) {
                var rune=Part("Inset faction crest",PrimitiveType.Cube,new Vector3((i-1)*.065f,.12f,face*.47f),new Vector3(.027f,.06f+(i==(faction%3)?.03f:0),.012f),accent);
                rune.transform.localRotation=Quaternion.Euler(0,0,robot?0:(i-1)*22);
            }
            }
            if(Role=="Wall")return;
            for(int side=-1;side<=1;side+=2) {
                Part(robot?"Recessed service panel":"Carved footing bracket",PrimitiveType.Cube,new Vector3(side*.32f,.31f,-.16f),new Vector3(.13f,.13f,.24f),dark);
                Part(robot?"Service panel rivet":"Bracket inlay",PrimitiveType.Sphere,new Vector3(side*.32f,.38f,-.16f),new Vector3(.06f,.035f,.06f),trim);
            }
        }
    }
}
