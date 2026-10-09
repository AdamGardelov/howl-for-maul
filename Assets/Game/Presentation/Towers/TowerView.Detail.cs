using UnityEngine;
namespace FrostMaze
{
    public sealed partial class TowerView
    {
        void DressFoundation(Material trim,Material dark,int faction,bool robot)
        {
            Part("Footing rim",PrimitiveType.Cylinder,new Vector3(0,.035f,0),new Vector3(.42f,.012f,.42f),trim);
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
            // Keep the shared hearth seal small. The silhouette is authored by each order;
            // shared skirts, buttresses and rear magazines used to make every unit read alike.
        }
    }
}
