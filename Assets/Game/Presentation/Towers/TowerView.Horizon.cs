using UnityEngine;
namespace FrostMaze
{
    public sealed partial class TowerView
    {
        void HorizonTower(Material trim,Material dark,int slot)
        {
            var pivot=new GameObject(Role+" weapon");pivot.transform.SetParent(transform,false);weapon=pivot.transform;
            Part("Horizon column",PrimitiveType.Cylinder,new Vector3(0,.63f,0),new Vector3(.4f,.35f,.4f),shell,weapon);
            if(slot==0) {
                Part("Glimmer scope",PrimitiveType.Sphere,new Vector3(0,1.04f,.06f),new Vector3(.38f,.37f,.61f),trim,weapon);
                Part("Glimmer lens",PrimitiveType.Sphere,new Vector3(0,1.04f,.35f),new Vector3(.22f,.22f,.06f),light,weapon);
                Crystal("Scope fin",new Vector3(0,1.32f,-.07f),new Vector3(.13f,.34f,.22f),accent,weapon);
            } else if(slot==1) {
                Part("Regent sun",PrimitiveType.Sphere,new Vector3(0,1.24f,0),Vector3.one*.4f,light,weapon);
                for(int i=0;i<6;i++) {
                    float a=i*Mathf.PI/3;
                    var ray=Crystal("Solar crown",new Vector3(Mathf.Sin(a)*.29f,1.24f+Mathf.Cos(a)*.29f,0),new Vector3(.12f,.3f,.16f),accent,weapon);ray.transform.localRotation=Quaternion.Euler(0,0,-i*60);
                }
            } else if(slot==2) {
                Part("Dust intake",PrimitiveType.Cube,new Vector3(0,1.04f,0),new Vector3(.64f,.42f,.52f),trim,weapon);
                for(int i=-1;i<=1;i++)Part("Intake grille",PrimitiveType.Cube,new Vector3(i*.16f,1.04f,.27f),new Vector3(.055f,.29f,.035f),dark,weapon);
                Part("Dust stack",PrimitiveType.Cylinder,new Vector3(0,1.42f,-.09f),new Vector3(.23f,.2f,.23f),accent,weapon);
            } else if(slot==3) {
                for(int i=0;i<3;i++) {
                    Part("Boneplate rib",PrimitiveType.Cube,new Vector3(0,.86f+i*.2f,-.04f),new Vector3(.65f-i*.1f,.11f,.38f),trim,weapon);
                }
                Part("Boneplate spine",PrimitiveType.Cylinder,new Vector3(0,1.1f,-.12f),new Vector3(.17f,.37f,.17f),accent,weapon);
                Part("Boneplate sight",PrimitiveType.Sphere,new Vector3(0,1.48f,.04f),Vector3.one*.21f,light,weapon);
            } else if(slot==4) {
                Part("Deepdiver bell",PrimitiveType.Sphere,new Vector3(0,1.03f,0),new Vector3(.57f,.62f,.51f),trim,weapon);
                for(int side=-1;side<=1;side+=2) {
                    var spear=Crystal("Sky harpoon",new Vector3(side*.25f,1.24f,.02f),new Vector3(.12f,.78f,.12f),accent,weapon);spear.transform.localRotation=Quaternion.Euler(-20,0,0);
                }
                Part("Diver porthole",PrimitiveType.Sphere,new Vector3(0,1.05f,.25f),new Vector3(.26f,.26f,.06f),light,weapon);
            } else if(slot==5) {
                Part("Drill yoke",PrimitiveType.Cube,new Vector3(0,.94f,0),new Vector3(.63f,.28f,.4f),trim,weapon);
                var drill=Crystal("Warden drill",new Vector3(0,1.25f,.13f),new Vector3(.43f,.85f,.43f),accent,weapon);drill.transform.localRotation=Quaternion.Euler(45,0,0);
                for(int side=-1;side<=1;side+=2)Part("Drill bearing",PrimitiveType.Sphere,new Vector3(side*.25f,1.09f,0),Vector3.one*.2f,dark,weapon);
            } else {
                Part("Crawler hull",PrimitiveType.Sphere,new Vector3(0,1.05f,0),new Vector3(.7f,.51f,.66f),trim,weapon);
                for(int side=-1;side<=1;side+=2)for(int i=-1;i<=1;i++) {
                    var leg=Part("Crawler leg",PrimitiveType.Cube,new Vector3(side*.32f,.65f,i*.23f),new Vector3(.12f,.51f,.14f),shell,weapon);leg.transform.localRotation=Quaternion.Euler(0,0,side*24);
                }
                var cannon=Part("Crawler rail",PrimitiveType.Cylinder,new Vector3(0,1.29f,.21f),new Vector3(.23f,.37f,.23f),accent,weapon);cannon.transform.localRotation=Quaternion.Euler(70,0,0);
                Part("Crawler optics",PrimitiveType.Sphere,new Vector3(0,1.48f,-.1f),Vector3.one*.2f,light,weapon);
            }
        }
    }
}
