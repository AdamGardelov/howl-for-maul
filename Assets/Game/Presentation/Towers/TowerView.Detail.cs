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
            // Twelve order seals share the hearth shape but use a distinct cut / arrangement.
            int mark=robot?faction:faction+8;
            for(int i=0;i<4;i++) {
                float angle=mark%2==0?45+i*90:i*90;
                float radians=angle*Mathf.Deg2Rad;
                var rune=Part("Inset order seal",PrimitiveType.Cube,new Vector3(Mathf.Sin(radians)*.085f,.12f+Mathf.Cos(radians)*.045f,face*.47f),new Vector3(.025f,.07f,.013f),accent);
                rune.transform.localRotation=Quaternion.Euler(0,0,angle+(mark/2)*15);
            }
            Part("Hearth seal heart",PrimitiveType.Sphere,new Vector3(0,.12f,face*.475f),new Vector3(.06f,.075f,.014f),light);
            }
            if(Role=="Wall")return;
            // A low enamel order pennant stays within the one-cell footing and batches with it.
            Part("Order pennant clasp",PrimitiveType.Cube,new Vector3(0,.58f,-.36f),new Vector3(.3f,.07f,.07f),trim);
            for(int side=-1;side<=1;side+=2) {
                var cloth=Part("Order pennant",PrimitiveType.Cube,new Vector3(side*.075f,.42f,-.38f),new Vector3(.14f,.27f,.025f),accent);
                cloth.transform.localRotation=Quaternion.Euler(6,0,side*4);
            }
            for(int side=-1;side<=1;side+=2) {
                var brace=Part(robot?"Sloped armor outrigger":"Carved stone buttress",PrimitiveType.Cube,new Vector3(side*.30f,.45f,-.19f),new Vector3(.17f,.52f,.25f),trim);
                brace.transform.localRotation=Quaternion.Euler(-12,0,-side*12);
                if(Role=="Interceptor")Crystal("Skyward rear fin",new Vector3(side*.25f,.98f,-.21f),new Vector3(.13f,.66f,.17f),accent);
                else if(Role=="Artillery")Part("Heavy rear magazine",PrimitiveType.Cylinder,new Vector3(side*.24f,.58f,-.25f),new Vector3(.22f,.26f,.25f),dark);
                Part(robot?"Recessed service panel":"Carved footing bracket",PrimitiveType.Cube,new Vector3(side*.32f,.31f,-.16f),new Vector3(.13f,.13f,.24f),dark);
                Part(robot?"Service panel rivet":"Bracket inlay",PrimitiveType.Sphere,new Vector3(side*.32f,.38f,-.16f),new Vector3(.06f,.035f,.06f),trim);
            }
        }
    }
}
