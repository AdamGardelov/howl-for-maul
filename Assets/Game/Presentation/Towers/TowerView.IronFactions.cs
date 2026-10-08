using UnityEngine;
namespace FrostMaze
{
    public sealed partial class TowerView
    {
        void IronBody(string name,Material dark,Vector3 scale)
        {
            var pivot=new GameObject(Role+" weapon");pivot.transform.SetParent(transform,false);weapon=pivot.transform;
            Part(name,PrimitiveType.Sphere,new Vector3(0,.73f,0),scale,shell,weapon);
            Part("Core mounting",PrimitiveType.Cylinder,new Vector3(0,.41f,0),new Vector3(.4f,.2f,.4f),dark,weapon);
        }
        void GravityTower(Material trim,Material dark,int slot)
        {
            IronBody("Gravity housing",dark,new Vector3(.61f,.6f,.55f));
            if(slot==0) {
                Part("Gyro spindle",PrimitiveType.Cylinder,new Vector3(0,1.09f,0),new Vector3(.14f,.32f,.14f),accent,weapon);
                for(int i=0;i<2;i++){var blade=Part("Gyro blade",PrimitiveType.Cube,new Vector3(0,1.35f,0),new Vector3(.76f,.07f,.14f),trim,weapon);blade.transform.localRotation=Quaternion.Euler(0,i*90,0);}
                Part("Gyro sensor",PrimitiveType.Sphere,new Vector3(0,1.43f,0),Vector3.one*.16f,light,weapon);
            } else if(slot==1) {
                Part("Anchor spine",PrimitiveType.Cube,new Vector3(0,1.03f,0),new Vector3(.2f,.84f,.24f),accent,weapon);
                Part("Anchor crossbar",PrimitiveType.Cube,new Vector3(0,.96f,0),new Vector3(.77f,.18f,.26f),trim,weapon);
                for(int side=-1;side<=1;side+=2)Crystal("Anchor hook",new Vector3(side*.3f,1.14f,0),new Vector3(.23f,.4f,.27f),trim,weapon);
                Part("Gravity well",PrimitiveType.Sphere,new Vector3(0,1.45f,0),Vector3.one*.26f,light,weapon);
            } else if(slot==2) {
                Part("Starcaller orb",PrimitiveType.Sphere,new Vector3(0,1.27f,0),Vector3.one*.49f,light,weapon);
                for(int i=0;i<4;i++){float a=i*Mathf.PI/2;Crystal("Star cradle",new Vector3(Mathf.Sin(a)*.26f,1.09f,Mathf.Cos(a)*.26f),new Vector3(.15f,.66f,.15f),trim,weapon);}
            } else if(slot==3) {
                for(int i=0;i<3;i++)Part("Wave resonator",PrimitiveType.Cylinder,new Vector3(0,.98f+i*.2f,0),new Vector3(.62f-i*.12f,.045f,.62f-i*.12f),i==1?accent:trim,weapon);
                Part("Resonator core",PrimitiveType.Cylinder,new Vector3(0,1.19f,0),new Vector3(.18f,.35f,.18f),light,weapon);
            } else if(slot==4) {
                for(int side=-1;side<=1;side+=2){Part("Charge capacitor",PrimitiveType.Cylinder,new Vector3(side*.24f,1.08f,0),new Vector3(.23f,.34f,.23f),trim,weapon);Crystal("Sky electrode",new Vector3(side*.24f,1.52f,0),new Vector3(.13f,.33f,.13f),light,weapon);}
                Part("Charge bridge",PrimitiveType.Cube,new Vector3(0,1.1f,-.13f),new Vector3(.57f,.16f,.16f),accent,weapon);
            } else if(slot==5) {
                for(int side=-1;side<=1;side+=2)Part("Granite shoulder",PrimitiveType.Cube,new Vector3(side*.24f,1.01f,0),new Vector3(.29f,.57f,.44f),trim,weapon);
                Crystal("Granite crown",new Vector3(0,1.42f,0),new Vector3(.39f,.45f,.37f),shell,weapon);
                Part("Granite visor",PrimitiveType.Cube,new Vector3(0,1.43f,.18f),new Vector3(.29f,.07f,.05f),light,weapon);
            } else {
                Part("Eclipse heart",PrimitiveType.Sphere,new Vector3(0,1.22f,0),Vector3.one*.58f,dark,weapon);
                for(int i=0;i<8;i++){float a=i*Mathf.PI/4;Part("Eclipse rim",PrimitiveType.Sphere,new Vector3(Mathf.Sin(a)*.34f,1.22f+Mathf.Cos(a)*.34f,0),Vector3.one*.16f,light,weapon);}
                for(int side=-1;side<=1;side+=2)Part("Eclipse cannon",PrimitiveType.Cube,new Vector3(side*.27f,.85f,.19f),new Vector3(.2f,.24f,.54f),trim,weapon);
            }
        }
        void ScrapTower(Material trim,Material dark,int slot)
        {
            IronBody("Scrap riveted hull",dark,new Vector3(.48f,.47f,.43f));
            for(int side=-1;side<=1;side+=2)Part("Scrap boot",PrimitiveType.Cube,new Vector3(side*.2f,.35f,.09f),new Vector3(.22f,.19f,.43f),trim,weapon);
            if(slot==0) {
                Part("Strider backpack",PrimitiveType.Cube,new Vector3(0,.88f,-.22f),new Vector3(.42f,.43f,.21f),accent,weapon);
                Part("Strider helmet",PrimitiveType.Sphere,new Vector3(0,1.11f,0),new Vector3(.37f,.4f,.35f),trim,weapon);
                Part("Strider aerial",PrimitiveType.Cylinder,new Vector3(.2f,1.32f,-.06f),new Vector3(.06f,.25f,.06f),light,weapon);
            } else if(slot==1) {
                Crystal("Knight lance",new Vector3(.26f,1.2f,.1f),new Vector3(.13f,1.05f,.13f),accent,weapon);
                Part("Knight shield",PrimitiveType.Cube,new Vector3(-.23f,.95f,.15f),new Vector3(.32f,.5f,.15f),trim,weapon);
                Crystal("Knight helm",new Vector3(0,1.24f,0),new Vector3(.34f,.38f,.32f),shell,weapon);
            } else if(slot==2) {
                Part("Wind hub",PrimitiveType.Sphere,new Vector3(0,1.11f,0),Vector3.one*.27f,light,weapon);
                for(int i=0;i<3;i++){float a=i*Mathf.PI*2/3;var vane=Part("Windkeeper vane",PrimitiveType.Cube,new Vector3(Mathf.Sin(a)*.25f,1.15f,Mathf.Cos(a)*.25f),new Vector3(.15f,.09f,.49f),trim,weapon);vane.transform.localRotation=Quaternion.Euler(18,i*120,0);}
            } else if(slot==3) {
                for(int i=0;i<5;i++){float a=i*Mathf.PI*2/5;Crystal("Bloom petal",new Vector3(Mathf.Sin(a)*.23f,1.13f,Mathf.Cos(a)*.23f),new Vector3(.25f,.5f,.23f),trim,weapon);}
                Part("Bloom core",PrimitiveType.Sphere,new Vector3(0,1.34f,0),Vector3.one*.27f,light,weapon);
            } else if(slot==4) {
                Part("Whiteout pack",PrimitiveType.Cube,new Vector3(0,1.02f,0),new Vector3(.59f,.48f,.42f),trim,weapon);
                for(int side=-1;side<=1;side+=2)Crystal("Whiteout missile",new Vector3(side*.2f,1.4f,0),new Vector3(.2f,.61f,.2f),accent,weapon);
                Part("Whiteout sight",PrimitiveType.Sphere,new Vector3(0,1.09f,.24f),Vector3.one*.18f,light,weapon);
            } else if(slot==5) {
                Part("Hatchet handle",PrimitiveType.Cylinder,new Vector3(0,1.1f,0),new Vector3(.12f,.42f,.12f),dark,weapon);
                for(int side=-1;side<=1;side+=2){var axe=Crystal("Hatchet blade",new Vector3(side*.2f,1.37f,0),new Vector3(.36f,.49f,.2f),trim,weapon);axe.transform.localRotation=Quaternion.Euler(0,0,side*30);}
                Part("Frost canister",PrimitiveType.Sphere,new Vector3(0,.83f,.23f),Vector3.one*.25f,light,weapon);
            } else {
                Part("Fossil skull",PrimitiveType.Sphere,new Vector3(0,1.18f,.03f),new Vector3(.64f,.51f,.6f),trim,weapon);
                Part("Fossil jaw",PrimitiveType.Cube,new Vector3(0,.95f,.25f),new Vector3(.48f,.13f,.37f),shell,weapon);
                for(int side=-1;side<=1;side+=2){Crystal("Fossil horn",new Vector3(side*.25f,1.5f,-.07f),new Vector3(.15f,.44f,.18f),accent,weapon);Part("Fossil eye",PrimitiveType.Sphere,new Vector3(side*.15f,1.24f,.28f),Vector3.one*.13f,light,weapon);}
            }
        }
        void OverdriveTower(Material trim,Material dark,int slot)
        {
            IronBody("Overdrive engine",dark,new Vector3(.68f,.57f,.61f));
            for(int side=-1;side<=1;side+=2)Part("Engine exhaust",PrimitiveType.Cylinder,new Vector3(side*.25f,.72f,-.2f),new Vector3(.15f,.29f,.15f),dark,weapon);
            if(slot==0) {
                Part("Junk crusher",PrimitiveType.Cube,new Vector3(0,1.06f,0),new Vector3(.6f,.39f,.5f),trim,weapon);
                Part("Crusher mouth",PrimitiveType.Cube,new Vector3(0,1.03f,.26f),new Vector3(.39f,.13f,.04f),dark,weapon);
                Crystal("Junk crest",new Vector3(0,1.42f,-.04f),new Vector3(.22f,.37f,.3f),accent,weapon);
            } else if(slot==1) {
                Part("Freeze chamber",PrimitiveType.Cylinder,new Vector3(0,1.12f,0),new Vector3(.53f,.35f,.53f),trim,weapon);
                for(int i=0;i<4;i++){float a=i*Mathf.PI/2;Crystal("Freeze prong",new Vector3(Mathf.Sin(a)*.29f,1.42f,Mathf.Cos(a)*.29f),new Vector3(.16f,.52f,.16f),light,weapon);}
            } else if(slot==2) {
                Part("Splash pressure tank",PrimitiveType.Sphere,new Vector3(0,1.04f,-.08f),new Vector3(.65f,.6f,.57f),trim,weapon);
                var nozzle=Part("Pressure nozzle",PrimitiveType.Cylinder,new Vector3(0,1.24f,.22f),new Vector3(.34f,.28f,.34f),accent,weapon);nozzle.transform.localRotation=Quaternion.Euler(45,0,0);
            } else if(slot==3) {
                for(int i=0;i<4;i++)Part("Spring winding",PrimitiveType.Cylinder,new Vector3(0,.94f+i*.13f,0),new Vector3(.35f,.025f,.35f),trim,weapon);
                Part("Striker fist",PrimitiveType.Cube,new Vector3(0,1.51f,0),new Vector3(.54f,.29f,.48f),accent,weapon);
            } else if(slot==4) {
                Crystal("Dusk hood",new Vector3(0,1.25f,0),new Vector3(.55f,.83f,.46f),dark,weapon);
                for(int side=-1;side<=1;side+=2){var dart=Crystal("Dusk sky dart",new Vector3(side*.26f,1.13f,.07f),new Vector3(.17f,.73f,.17f),trim,weapon);dart.transform.localRotation=Quaternion.Euler(-20,0,side*-18);}
                Part("Dusk visor",PrimitiveType.Cube,new Vector3(0,1.31f,.2f),new Vector3(.28f,.06f,.05f),light,weapon);
            } else if(slot==5) {
                for(int side=-1;side<=1;side+=2){var rotor=Part("Turbo rotor",PrimitiveType.Cylinder,new Vector3(side*.23f,1.14f,0),new Vector3(.33f,.32f,.33f),trim,weapon);rotor.transform.localRotation=Quaternion.Euler(90,0,0);Part("Turbo lens",PrimitiveType.Sphere,new Vector3(side*.23f,1.14f,.32f),Vector3.one*.16f,light,weapon);}
            } else {
                Part("Champion mask",PrimitiveType.Sphere,new Vector3(0,1.18f,.1f),new Vector3(.67f,.76f,.36f),trim,weapon);
                for(int side=-1;side<=1;side+=2){Crystal("Mask horn",new Vector3(side*.27f,1.54f,0),new Vector3(.18f,.42f,.24f),accent,weapon);Part("Mask eye",PrimitiveType.Cube,new Vector3(side*.15f,1.3f,.29f),new Vector3(.16f,.07f,.05f),light,weapon);}
                Part("Mask jaw",PrimitiveType.Cube,new Vector3(0,.95f,.25f),new Vector3(.36f,.18f,.25f),dark,weapon);
            }
        }
        void TidalTower(Material trim,Material dark,int slot)
        {
            IronBody("Tidal pressure hull",dark,new Vector3(.53f,.57f,.51f));
            if(slot==0) {
                Part("Grenade drum",PrimitiveType.Sphere,new Vector3(0,1.02f,0),new Vector3(.53f,.54f,.47f),trim,weapon);
                Part("Grenade cap",PrimitiveType.Cylinder,new Vector3(0,1.35f,0),new Vector3(.27f,.08f,.27f),accent,weapon);
                Part("Grenade fuse",PrimitiveType.Cube,new Vector3(.12f,1.45f,0),new Vector3(.23f,.06f,.1f),light,weapon);
            } else if(slot==1) {
                Part("Kite mast",PrimitiveType.Cylinder,new Vector3(0,1.05f,0),new Vector3(.12f,.45f,.12f),trim,weapon);
                for(int side=-1;side<=1;side+=2){var sail=Crystal("Kite sail",new Vector3(side*.19f,1.24f,0),new Vector3(.3f,.7f,.14f),accent,weapon);sail.transform.localRotation=Quaternion.Euler(0,0,-side*24);Part("Kite terminal",PrimitiveType.Sphere,new Vector3(side*.35f,1.39f,0),Vector3.one*.17f,light,weapon);}
            } else if(slot==2) {
                Part("Jester globe",PrimitiveType.Sphere,new Vector3(0,1.12f,0),Vector3.one*.47f,trim,weapon);
                for(int side=-1;side<=1;side+=2){var tip=Crystal("Jester cap",new Vector3(side*.22f,1.43f,0),new Vector3(.21f,.47f,.23f),accent,weapon);tip.transform.localRotation=Quaternion.Euler(0,0,-side*30);Part("Jester charge",PrimitiveType.Sphere,new Vector3(side*.33f,1.58f,0),Vector3.one*.17f,light,weapon);}
            } else if(slot==3) {
                Part("Aqua reservoir",PrimitiveType.Cylinder,new Vector3(0,1.08f,0),new Vector3(.45f,.37f,.45f),light,weapon);
                Part("Reservoir lid",PrimitiveType.Cylinder,new Vector3(0,1.47f,0),new Vector3(.55f,.05f,.55f),trim,weapon);
                for(int side=-1;side<=1;side+=2)Part("Aqua pipe",PrimitiveType.Cylinder,new Vector3(side*.27f,.99f,0),new Vector3(.13f,.31f,.13f),accent,weapon);
            } else if(slot==4) {
                for(int side=-1;side<=1;side+=2){var blade=Crystal("Keeper sky blade",new Vector3(side*.21f,1.26f,0),new Vector3(.25f,.91f,.16f),trim,weapon);blade.transform.localRotation=Quaternion.Euler(-15,0,side*20);}
                Part("Blade sensor",PrimitiveType.Sphere,new Vector3(0,1.16f,.2f),Vector3.one*.21f,light,weapon);
            } else if(slot==5) {
                Part("Orbit axis",PrimitiveType.Cylinder,new Vector3(0,1.09f,0),new Vector3(.15f,.39f,.15f),trim,weapon);
                for(int i=0;i<3;i++){float a=i*Mathf.PI*2/3;Part("Orbit satellite",PrimitiveType.Sphere,new Vector3(Mathf.Sin(a)*.29f,1.21f,Mathf.Cos(a)*.29f),Vector3.one*.27f,accent,weapon);}
                Part("Orbit beacon",PrimitiveType.Sphere,new Vector3(0,1.54f,0),Vector3.one*.2f,light,weapon);
            } else {
                Part("Verdant trunk",PrimitiveType.Cylinder,new Vector3(0,1.06f,0),new Vector3(.38f,.46f,.38f),trim,weapon);
                for(int i=0;i<5;i++){float a=i*Mathf.PI*2/5;var leaf=Crystal("Verdant crown",new Vector3(Mathf.Sin(a)*.25f,1.42f,Mathf.Cos(a)*.25f),new Vector3(.26f,.65f,.2f),accent,weapon);leaf.transform.localRotation=Quaternion.Euler(Mathf.Cos(a)*20,0,-Mathf.Sin(a)*20);}
                Part("Verdant heart",PrimitiveType.Sphere,new Vector3(0,1.46f,.15f),Vector3.one*.29f,light,weapon);
            }
        }
    }
}
