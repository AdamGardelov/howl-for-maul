using UnityEngine;
namespace FrostMaze
{
    public sealed partial class TowerView
    {
        // Copper pressure engines: the mechanism, rather than a common turret, carries the role.
        void BlastPipe(string name,Vector3 from,Vector3 to,float width,Material metal,Material dark)
        {
            Strut(name,from,to,width,metal,weapon);
            var axis=(to-from).normalized;
            Strut(name+" dark bore",to,to+axis*.012f,width*.76f,dark,weapon);
            Strut(name+" charge",to+axis*.015f,to+axis*.019f,width*.30f,light,weapon);
            Loop(name+" muzzle band",to-axis*.035f,width*1.13f,Quaternion.FromToRotation(Vector3.up,axis),metal,weapon);
        }
        GameObject BlastPlate(string name,Vector3 at,Vector3 size,Material material)
        {
            var plate=Part(name,PrimitiveType.Sphere,at,size,material,weapon);
            plate.GetComponent<MeshFilter>().sharedMesh=game.Models.Armor;return plate;
        }
        void BlastTower(Material trim,Material dark,int slot)
        {
            var pivot=new GameObject(Role+" weapon");pivot.transform.SetParent(transform,false);weapon=pivot.transform;
            if(slot==0) { // Small forward gun and two split shell plates: the inexpensive siege beetle.
                Part("Alloy dome",PrimitiveType.Sphere,new Vector3(0,.65f,-.07f),new Vector3(.61f,.44f,.56f),shell,weapon);
                for(int side=-1;side<=1;side+=2){
                    var plate=BlastPlate("Split copper shell",new Vector3(side*.21f,.73f,-.12f),new Vector3(.25f,.25f,.55f),trim);
                    plate.transform.localRotation=Quaternion.Euler(0,0,side*-20);
                    Part("Alloy eye",PrimitiveType.Sphere,new Vector3(side*.19f,.77f,.18f),new Vector3(.09f,.08f,.04f),light,weapon);
                }
                BlastPipe("Alloy cannon",new Vector3(0,.69f,.05f),new Vector3(0,.72f,.47f),.24f,accent,dark);
            } else if(slot==1) { // Open press, raised hammer, visible anvil: broad and mechanical.
                Part("Press anvil",PrimitiveType.Cube,new Vector3(0,.52f,.07f),new Vector3(.51f,.22f,.45f),dark,weapon);
                for(int side=-1;side<=1;side+=2){
                    Strut("Hammer guide",new Vector3(side*.28f,.42f,-.13f),new Vector3(side*.28f,1.35f,-.13f),.10f,trim,weapon);
                    Part("Press foot",PrimitiveType.Cube,new Vector3(side*.28f,.51f,-.10f),new Vector3(.21f,.22f,.45f),shell,weapon);
                }
                Part("Press bridge",PrimitiveType.Cube,new Vector3(0,1.35f,-.13f),new Vector3(.73f,.17f,.24f),shell,weapon);
                Strut("Breaker piston",new Vector3(0,1.4f,-.09f),new Vector3(0,.99f,-.09f),.14f,accent,weapon);
                Part("Crash hammer",PrimitiveType.Cube,new Vector3(0,1.04f,.10f),new Vector3(.57f,.26f,.39f),trim,weapon);
                Part("Hammer striking face",PrimitiveType.Cube,new Vector3(0,.89f,.10f),new Vector3(.52f,.08f,.35f),dark,weapon);
                for(int side=-1;side<=1;side+=2)Part("Hammer charge slit",PrimitiveType.Cube,new Vector3(side*.18f,1.05f,.305f),new Vector3(.07f,.15f,.023f),light,weapon);
            } else if(slot==2) { // Wind artillery is a circular pressure fan on a low cradle.
                var hub=new Vector3(0,.96f,.04f);
                Loop("Gale pressure rim",hub,.76f,Quaternion.Euler(90,0,0),trim,weapon);
                Loop("Gale rear casing",hub+Vector3.back*.17f,.72f,Quaternion.Euler(90,0,0),shell,weapon);
                for(int i=0;i<5;i++){
                    float a=i*Mathf.PI*2/5;
                    var vane=BlastPlate("Gale vane",hub+new Vector3(Mathf.Sin(a)*.20f,Mathf.Cos(a)*.20f,.01f),new Vector3(.18f,.32f,.09f),accent);
                    vane.transform.localRotation=Quaternion.Euler(0,0,-i*72-32);
                }
                Part("Gale hub",PrimitiveType.Sphere,hub+Vector3.forward*.10f,new Vector3(.23f,.23f,.16f),light,weapon);
                for(int side=-1;side<=1;side+=2)Strut("Fan cradle",new Vector3(side*.25f,.42f,-.03f),hub+Vector3.right*(side*.29f),.13f,shell,weapon);
                BlastPipe("Pressure intake",new Vector3(0,.64f,-.34f),new Vector3(0,.88f,-.25f),.22f,trim,dark);
            } else if(slot==3) { // Tall, rounded pressure kettle with a gauge, bands and paired flues.
                var vessel=Part("Heatkeeper boiler",PrimitiveType.Sphere,new Vector3(0,.86f,0),new Vector3(.54f,.94f,.51f),shell,weapon);
                vessel.GetComponent<MeshFilter>().sharedMesh=game.Models.Armor;
                Part("Boiler dome",PrimitiveType.Sphere,new Vector3(0,1.31f,0),new Vector3(.54f,.24f,.51f),trim,weapon);
                for(int band=0;band<2;band++)Loop("Pressure band",new Vector3(0,.60f+band*.49f,0),.56f,Quaternion.identity,accent,weapon);
                Loop("Pressure gauge rim",new Vector3(0,1.09f,.272f),.25f,Quaternion.Euler(90,0,0),trim,weapon);
                Part("Pressure gauge face",PrimitiveType.Sphere,new Vector3(0,1.09f,.278f),new Vector3(.19f,.19f,.025f),dark,weapon);
                Strut("Pressure needle",new Vector3(0,1.09f,.30f),new Vector3(-.055f,1.145f,.30f),.018f,light,weapon);
                for(int side=-1;side<=1;side+=2){
                    if(side==1)BlastPipe("Heat exhaust",new Vector3(.27f,.53f,-.12f),new Vector3(.27f,1.72f,-.12f),.17f,trim,dark);
                    BlastPipe("Steam nozzle",new Vector3(side*.16f,.71f,.16f),new Vector3(side*.19f,.77f,.39f),.13f,accent,dark);
                }
                Part("Banked furnace",PrimitiveType.Cube,new Vector3(0,.67f,.259f),new Vector3(.20f,.25f,.025f),light,weapon);
                for(int bar=-1;bar<=1;bar++)Part("Furnace grille",PrimitiveType.Cube,new Vector3(bar*.065f,.67f,.28f),new Vector3(.025f,.27f,.035f),dark,weapon);
            } else if(slot==4) { // Air-only rack: raised swept wings with empty sky beneath them.
                Part("Sky rack yoke",PrimitiveType.Cube,new Vector3(0,.68f,-.08f),new Vector3(.52f,.15f,.32f),trim,weapon);
                for(int side=-1;side<=1;side+=2){
                    Strut("Sky rack upright",new Vector3(side*.17f,.49f,-.14f),new Vector3(side*.17f,1.02f,-.14f),.085f,dark,weapon);
                    var wing=Part("Quicksilver wing",PrimitiveType.Sphere,new Vector3(side*.11f,1.13f,-.03f),new Vector3(.44f,.85f,.64f),shell,weapon);
                    wing.GetComponent<MeshFilter>().sharedMesh=game.Models.Wing(side);wing.transform.localRotation=Quaternion.Euler(-22,0,side*19);
                    BlastPipe("Sky bomb rail",new Vector3(side*.22f,.91f,-.22f),new Vector3(side*.22f,1.48f,.20f),.115f,accent,dark);
                    var tip=Crystal("Quicksilver warhead",new Vector3(side*.22f,1.53f,.235f),new Vector3(.17f,.28f,.17f),trim,weapon);tip.transform.localRotation=Quaternion.Euler(36,0,0);
                }
                Part("Quicksilver fuselage",PrimitiveType.Sphere,new Vector3(0,1.20f,-.04f),new Vector3(.22f,.25f,.65f),trim,weapon);
                Loop("Skyfinder sight",new Vector3(0,1.4f,.22f),.23f,Quaternion.Euler(55,0,0),accent,weapon);
            } else if(slot==5) { // A planted three-barrel battery; wide low shield contrasts with the boiler.
                Part("Rootguard shield",PrimitiveType.Cube,new Vector3(0,.63f,.14f),new Vector3(.77f,.42f,.24f),shell,weapon);
                for(int gun=-1;gun<=1;gun++){
                    var from=new Vector3(gun*.23f,.77f,-.27f);var to=new Vector3(gun*.25f,.91f+(gun==0?.10f:0),.34f);
                    BlastPipe("Rootguard barrel",from,to,.18f,trim,dark);
                    Part("Rootguard breech",PrimitiveType.Sphere,from,new Vector3(.23f,.25f,.29f),accent,weapon);
                }
                for(int side=-1;side<=1;side+=2)Loop("Battery elevation gear",new Vector3(side*.35f,.70f,-.09f),.28f,Quaternion.Euler(0,0,90),accent,weapon);
                Part("Rootguard eye",PrimitiveType.Sphere,new Vector3(0,.62f,.278f),new Vector3(.21f,.075f,.027f),light,weapon);
            } else { // Mobile bastion: paired siege mortars, armored prow and a tall split standard.
                Part("Citadel keep",PrimitiveType.Cube,new Vector3(0,.64f,0),new Vector3(.73f,.39f,.63f),shell,weapon);
                for(int side=-1;side<=1;side+=2){
                    var from=new Vector3(side*.24f,.83f,-.23f);var to=new Vector3(side*.24f,1.45f,.26f);
                    BlastPipe("Citadel mortar",from,to,.29f,trim,dark);
                    BlastPlate("Citadel shoulder",new Vector3(side*.36f,.80f,-.05f),new Vector3(.24f,.38f,.58f),accent);
                    Part("Bastion prow",PrimitiveType.Cube,new Vector3(side*.20f,.62f,.34f),new Vector3(.36f,.47f,.13f),trim,weapon);
                    Strut("Champion standard",new Vector3(side*.13f,.72f,-.30f),new Vector3(side*.13f,1.78f,-.30f),.065f,trim,weapon);
                    Part("Split siege banner",PrimitiveType.Cube,new Vector3(side*.12f,1.49f,-.31f),new Vector3(.18f,.41f,.045f),accent,weapon);
                }
                Loop("Citadel seal",new Vector3(0,1.75f,-.29f),.29f,Quaternion.Euler(90,0,0),trim,weapon);
                Part("Citadel beacon",PrimitiveType.Sphere,new Vector3(0,1.75f,-.26f),Vector3.one*.105f,light,weapon);
                for(int vent=-1;vent<=1;vent++)Part("Bastion vent",PrimitiveType.Cube,new Vector3(vent*.11f,.64f,.42f),new Vector3(.048f,.19f,.027f),light,weapon);
            }
        }
    }
}
