using UnityEngine;
namespace FrostMaze
{
    public sealed partial class TowerView
    {
        void PrismTower(Material trim,Material dark,int slot)
        {
            var pivot=new GameObject(Role+" weapon");pivot.transform.SetParent(transform,false);weapon=pivot.transform;
            Part("Prism waist",PrimitiveType.Cylinder,new Vector3(0,.48f,0),new Vector3(.35f,.22f,.35f),dark,weapon);
            Part("Prism cuirass",PrimitiveType.Sphere,new Vector3(0,.77f,0),new Vector3(.58f,.55f,.43f),shell,weapon);
            if(slot==0) {
                Crystal("Shade hood",new Vector3(0,1.12f,0),new Vector3(.47f,.62f,.43f),trim,weapon);
                Part("Shade visor",PrimitiveType.Cube,new Vector3(0,1.16f,.19f),new Vector3(.3f,.055f,.05f),light,weapon);
                var barrel=Part("Shade blaster",PrimitiveType.Cylinder,new Vector3(.24f,.86f,.21f),new Vector3(.18f,.2f,.18f),accent,weapon);barrel.transform.localRotation=Quaternion.Euler(90,0,0);
            } else if(slot==1) {
                for(int side=-1;side<=1;side+=2) {
                    Part("Herald conductor",PrimitiveType.Cylinder,new Vector3(side*.25f,1.12f,0),new Vector3(.12f,.39f,.12f),trim,weapon);
                    Part("Herald terminal",PrimitiveType.Sphere,new Vector3(side*.25f,1.5f,0),Vector3.one*.24f,light,weapon);
                }
                Crystal("Spark core",new Vector3(0,1.13f,.08f),new Vector3(.27f,.48f,.27f),accent,weapon);
            } else if(slot==2) {
                for(int side=-1;side<=1;side+=2) {
                    Part("Lodestone pole",PrimitiveType.Cube,new Vector3(side*.24f,1.1f,0),new Vector3(.24f,.63f,.37f),trim,weapon);
                    Part("Pole charge",PrimitiveType.Cube,new Vector3(side*.24f,1.43f,0),new Vector3(.25f,.08f,.38f),light,weapon);
                }
                Part("Magnet bridge",PrimitiveType.Cube,new Vector3(0,.83f,0),new Vector3(.65f,.21f,.37f),accent,weapon);
            } else if(slot==3) {
                for(int i=0;i<4;i++) {
                    float angle=i*1.5f;
                    Part("Serpent coil",PrimitiveType.Sphere,new Vector3(Mathf.Sin(angle)*.13f,.87f+i*.15f,Mathf.Cos(angle)*.13f),new Vector3(.37f,.24f,.34f),i%2==0?accent:trim,weapon);
                }
                Part("Serpent head",PrimitiveType.Cube,new Vector3(0,1.47f,.12f),new Vector3(.32f,.2f,.36f),shell,weapon);
                Part("Serpent eye",PrimitiveType.Cube,new Vector3(0,1.5f,.31f),new Vector3(.22f,.05f,.04f),light,weapon);
            } else if(slot==4) {
                for(int side=-1;side<=1;side+=2) {
                    var lance=Crystal("Twin sky lance",new Vector3(side*.23f,1.18f,0),new Vector3(.25f,.88f,.25f),trim,weapon);lance.transform.localRotation=Quaternion.Euler(-18,0,side*-12);
                    Part("Twin lens",PrimitiveType.Sphere,new Vector3(side*.23f,1.24f,.14f),Vector3.one*.16f,light,weapon);
                }
            } else if(slot==5) {
                Part("Needle helm",PrimitiveType.Sphere,new Vector3(0,1.15f,0),new Vector3(.4f,.43f,.37f),trim,weapon);
                for(int i=-1;i<=1;i++) {
                    var spike=Crystal("Needle rack",new Vector3(i*.22f,1.15f,-.14f),new Vector3(.11f,.75f,.11f),accent,weapon);spike.transform.localRotation=Quaternion.Euler(-25,0,-i*17);
                }
                Part("Guard shield",PrimitiveType.Sphere,new Vector3(-.25f,.85f,.19f),new Vector3(.26f,.49f,.17f),shell,weapon);
            } else {
                Part("Champion carapace",PrimitiveType.Sphere,new Vector3(0,1.08f,-.08f),new Vector3(.74f,.8f,.6f),trim,weapon);
                for(int side=-1;side<=1;side+=2) {
                    var cannon=Part("Shell cannon",PrimitiveType.Cylinder,new Vector3(side*.27f,1.05f,.25f),new Vector3(.23f,.29f,.23f),accent,weapon);cannon.transform.localRotation=Quaternion.Euler(75,0,0);
                    Crystal("Shell crest",new Vector3(side*.2f,1.53f,-.04f),new Vector3(.2f,.42f,.25f),shell,weapon);
                }
                Crystal("Champion prism",new Vector3(0,1.26f,.24f),new Vector3(.23f,.43f,.18f),light,weapon);
            }
        }
    }
}
