using UnityEngine;
namespace FrostMaze
{
    public sealed partial class TowerView
    {
        // Pulse Foundry uses the first seven catalog slots. All pieces are cosmetic.
        void PulseTower(Material trim,Material dark,int slot)
        {
            var pivot=new GameObject(Role+" weapon");pivot.transform.SetParent(transform,false);weapon=pivot.transform;
            for(int side=-1;side<=1;side+=2)Part("Foundry boot",PrimitiveType.Cube,new Vector3(side*.16f,.4f,.02f),new Vector3(.25f,.25f,.37f),dark,weapon);
            Part("Foundry torso",PrimitiveType.Sphere,new Vector3(0,.72f,0),new Vector3(.52f,.53f,.46f),shell,weapon);
            if(slot==2) {
                Part("Shear housing",PrimitiveType.Cube,new Vector3(0,1.04f,0),new Vector3(.64f,.31f,.48f),dark,weapon);
                for(int side=-1;side<=1;side+=2) {
                    var blade=Crystal("Shear blade",new Vector3(side*.23f,1.36f,.08f),new Vector3(.21f,.72f,.17f),trim,weapon);blade.transform.localRotation=Quaternion.Euler(18,0,side*-16);
                }
                Part("Shear charge",PrimitiveType.Sphere,new Vector3(0,1.07f,.26f),Vector3.one*.19f,accent,weapon);
            } else if(slot==4) {
                Part("Flare targeting dome",PrimitiveType.Sphere,new Vector3(0,1.02f,0),new Vector3(.44f,.37f,.42f),accent,weapon);
                for(int side=-1;side<=1;side+=2) {
                    var pod=Part("Flare rocket pod",PrimitiveType.Cylinder,new Vector3(side*.27f,1.18f,0),new Vector3(.22f,.38f,.22f),shell,weapon);pod.transform.localRotation=Quaternion.Euler(20,0,0);
                    Crystal("Flare rocket tip",new Vector3(side*.27f,1.54f,.13f),new Vector3(.16f,.28f,.16f),light,weapon);
                }
            } else if(slot==5) {
                Part("Runner cowl",PrimitiveType.Sphere,new Vector3(0,1.08f,0),new Vector3(.46f,.43f,.43f),dark,weapon);
                for(int side=-1;side<=1;side+=2) {
                    Crystal("Cryo reservoir",new Vector3(side*.26f,.96f,0),new Vector3(.19f,.66f,.22f),accent,weapon);
                    Part("Runner skate",PrimitiveType.Cube,new Vector3(side*.16f,.27f,.08f),new Vector3(.2f,.08f,.54f),trim,weapon);
                }
                Crystal("Frost projector",new Vector3(0,1.13f,.27f),new Vector3(.19f,.28f,.29f),light,weapon);
            } else {
                Part("Foundry helmet",PrimitiveType.Sphere,new Vector3(0,1.12f,0),new Vector3(.43f,.39f,.4f),accent,weapon);
                Part("Foundry visor",PrimitiveType.Cube,new Vector3(0,1.14f,.19f),new Vector3(.29f,.085f,.04f),light,weapon);
                if(slot==0) {
                    var barrel=Part("Fuse barrel",PrimitiveType.Cylinder,new Vector3(.27f,.77f,.2f),new Vector3(.19f,.23f,.19f),accent,weapon);barrel.transform.localRotation=Quaternion.Euler(90,0,0);
                    Part("Fuse antenna",PrimitiveType.Cylinder,new Vector3(-.16f,1.44f,0),new Vector3(.05f,.16f,.05f),trim,weapon);
                } else if(slot==1) {
                    for(int side=-1;side<=1;side+=2) {
                        Part("Iron gauntlet",PrimitiveType.Sphere,new Vector3(side*.27f,.82f,.16f),new Vector3(.3f,.43f,.39f),trim,weapon);
                        Part("Gauntlet emitter",PrimitiveType.Cube,new Vector3(side*.27f,.86f,.35f),new Vector3(.18f,.1f,.035f),light,weapon);
                    }
                } else if(slot==3) {
                    Part("Ranger pack",PrimitiveType.Cube,new Vector3(0,.91f,-.24f),new Vector3(.4f,.53f,.21f),dark,weapon);
                    var rifle=Part("Ranger rifle",PrimitiveType.Cylinder,new Vector3(.27f,.95f,.15f),new Vector3(.14f,.32f,.14f),trim,weapon);rifle.transform.localRotation=Quaternion.Euler(90,0,0);
                    Part("Ranger scope",PrimitiveType.Sphere,new Vector3(.27f,1.08f,.28f),Vector3.one*.12f,light,weapon);
                } else {
                    for(int side=-1;side<=1;side+=2) {
                        Part("Echo shoulder",PrimitiveType.Sphere,new Vector3(side*.28f,1.13f,0),new Vector3(.28f,.35f,.46f),trim,weapon);
                        var cannon=Part("Echo cannon",PrimitiveType.Cylinder,new Vector3(side*.27f,.82f,.19f),new Vector3(.23f,.26f,.23f),accent,weapon);cannon.transform.localRotation=Quaternion.Euler(90,0,0);
                        Crystal("Echo crest",new Vector3(side*.16f,1.49f,0),new Vector3(.12f,.51f,.14f),light,weapon);
                    }
                    Part("Echo reactor",PrimitiveType.Sphere,new Vector3(0,.82f,-.28f),new Vector3(.36f,.4f,.18f),accent,weapon);
                }
            }
        }
    }
}
