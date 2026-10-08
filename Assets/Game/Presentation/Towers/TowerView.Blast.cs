using UnityEngine;
namespace FrostMaze
{
    public sealed partial class TowerView
    {
        void BlastTower(Material trim,Material dark,int slot)
        {
            var pivot=new GameObject(Role+" weapon");pivot.transform.SetParent(transform,false);weapon=pivot.transform;
            Part("Blast chassis",PrimitiveType.Sphere,new Vector3(0,.56f,0),new Vector3(.68f,.55f,.62f),dark,weapon);
            for(int side=-1;side<=1;side+=2)Part("Blast tread",PrimitiveType.Cube,new Vector3(side*.28f,.4f,0),new Vector3(.19f,.25f,.58f),shell,weapon);
            if(slot==0) {
                Part("Alloy dome",PrimitiveType.Sphere,new Vector3(0,.92f,0),new Vector3(.57f,.52f,.49f),shell,weapon);
                var barrel=Part("Alloy cannon",PrimitiveType.Cylinder,new Vector3(0,.91f,.23f),new Vector3(.25f,.23f,.25f),accent,weapon);barrel.transform.localRotation=Quaternion.Euler(90,0,0);
                Part("Alloy stripe",PrimitiveType.Cube,new Vector3(0,1.16f,0),new Vector3(.13f,.06f,.37f),light,weapon);
            } else if(slot==1) {
                Part("Breaker piston",PrimitiveType.Cylinder,new Vector3(0,.93f,0),new Vector3(.23f,.4f,.23f),accent,weapon);
                Part("Crash hammer",PrimitiveType.Cube,new Vector3(0,1.29f,0),new Vector3(.73f,.35f,.44f),shell,weapon);
                for(int side=-1;side<=1;side+=2)Part("Hammer cap",PrimitiveType.Cube,new Vector3(side*.33f,1.29f,0),new Vector3(.09f,.39f,.47f),trim,weapon);
            } else if(slot==2) {
                Part("Gale hub",PrimitiveType.Sphere,new Vector3(0,1.03f,0),new Vector3(.43f,.6f,.43f),accent,weapon);
                for(int i=0;i<4;i++) {
                    float a=i*Mathf.PI/2;
                    var fin=Crystal("Gale vane",new Vector3(Mathf.Sin(a)*.24f,1.2f,Mathf.Cos(a)*.24f),new Vector3(.18f,.68f,.2f),trim,weapon);fin.transform.localRotation=Quaternion.Euler(Mathf.Cos(a)*-26,0,Mathf.Sin(a)*26);
                }
                Part("Gale charge",PrimitiveType.Sphere,new Vector3(0,1.4f,0),Vector3.one*.22f,light,weapon);
            } else if(slot==3) {
                Part("Heatkeeper boiler",PrimitiveType.Cylinder,new Vector3(0,.95f,0),new Vector3(.55f,.42f,.55f),shell,weapon);
                Part("Boiler cap",PrimitiveType.Cylinder,new Vector3(0,1.39f,0),new Vector3(.62f,.065f,.62f),dark,weapon);
                for(int side=-1;side<=1;side+=2)Part("Heat exhaust",PrimitiveType.Cylinder,new Vector3(side*.28f,.93f,-.05f),new Vector3(.13f,.35f,.13f),accent,weapon);
                Crystal("Boiler flame",new Vector3(0,1.59f,0),new Vector3(.24f,.34f,.24f),light,weapon);
            } else if(slot==4) {
                Part("Quicksilver fuselage",PrimitiveType.Sphere,new Vector3(0,1.01f,0),new Vector3(.34f,.53f,.69f),trim,weapon);
                for(int side=-1;side<=1;side+=2) {
                    var wing=Part("Quicksilver wing",PrimitiveType.Sphere,new Vector3(side*.23f,1.04f,-.04f),new Vector3(.48f,.11f,.43f),shell,weapon);wing.GetComponent<MeshFilter>().sharedMesh=game.Models.Wing(side);
                    Part("Sky bomb",PrimitiveType.Sphere,new Vector3(side*.25f,.93f,.18f),new Vector3(.17f,.2f,.28f),accent,weapon);
                }
                Part("Quicksilver sight",PrimitiveType.Sphere,new Vector3(0,1.21f,.12f),Vector3.one*.18f,light,weapon);
            } else if(slot==5) {
                Part("Rootguard trunk",PrimitiveType.Cylinder,new Vector3(0,.91f,0),new Vector3(.35f,.39f,.35f),shell,weapon);
                for(int i=0;i<3;i++) {
                    float a=i*Mathf.PI*2/3;
                    var tube=Part("Rootguard barrel",PrimitiveType.Cylinder,new Vector3(Mathf.Sin(a)*.23f,1.17f,Mathf.Cos(a)*.23f),new Vector3(.19f,.28f,.19f),accent,weapon);tube.transform.localRotation=Quaternion.Euler(Mathf.Cos(a)*22,0,-Mathf.Sin(a)*22);
                }
                Part("Rootguard sensor",PrimitiveType.Sphere,new Vector3(0,1.43f,0),Vector3.one*.23f,light,weapon);
            } else {
                Part("Citadel keep",PrimitiveType.Cube,new Vector3(0,.98f,0),new Vector3(.55f,.82f,.49f),shell,weapon);
                for(int side=-1;side<=1;side+=2) {
                    Part("Citadel turret",PrimitiveType.Cylinder,new Vector3(side*.26f,1.12f,0),new Vector3(.25f,.43f,.25f),trim,weapon);
                    var mortar=Part("Citadel mortar",PrimitiveType.Cylinder,new Vector3(side*.25f,1.45f,.12f),new Vector3(.2f,.25f,.2f),accent,weapon);mortar.transform.localRotation=Quaternion.Euler(30,0,0);
                }
                Crystal("Citadel beacon",new Vector3(0,1.6f,0),new Vector3(.2f,.37f,.2f),light,weapon);
            }
        }
    }
}
