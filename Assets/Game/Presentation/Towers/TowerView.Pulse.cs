using UnityEngine;
namespace FrostMaze
{
    public sealed partial class TowerView
    {
        // Reclaimed hearth-forge instruments: stone beds, cast copper and small glass cores.
        // Shapes and mechanisms are cosmetic; catalog roles, shot signatures and cells stay intact.
        GameObject FoundryShape(string name,Mesh mesh,Vector3 point,Vector3 size,Material material,Transform parent=null)
        {
            var part=Part(name,PrimitiveType.Sphere,point,size,material,parent);
            part.GetComponent<MeshFilter>().sharedMesh=mesh;return part;
        }
        void FoundryPipe(string name,Vector3[] points,float width,Material material,Transform parent=null)
        {
            string key=Subject.Design+"/"+name+"/"+points[0];
            FoundryShape(name,game.Models.CurvedPipe(key,points,width),Vector3.zero,Vector3.one,material,parent);
        }
        void FoundryMuzzle(Vector3 point,float diameter,float length,Material trim,Material dark,Transform parent)
        {
            var barrel=FoundryShape("Cast bellmouth",game.Models.Bell,point,new Vector3(diameter,length,diameter),shell,parent);
            barrel.transform.localRotation=Quaternion.Euler(-90,0,0);
            Loop("Rolled copper lip",point+Vector3.forward*(length*.47f),diameter,Quaternion.Euler(90,0,0),trim,parent);
            Part("Deep bore",PrimitiveType.Sphere,point+Vector3.forward*(length*.46f),new Vector3(diameter*.65f,diameter*.65f,.025f),dark,parent);
            Part("Pulse lens",PrimitiveType.Sphere,point+Vector3.forward*(length*.48f),new Vector3(diameter*.28f,diameter*.28f,.03f),light,parent);
        }
        void PulseTower(Material trim,Material dark,int slot)
        {
            // The base is masonry, separate from the moving weapon: it cannot rotate over a neighbor.
            Part("Worn slate bed",PrimitiveType.Cube,new Vector3(0,.10f,0),new Vector3(.68f,.17f,.59f),dark);
            for(int side=-1;side<=1;side+=2){
                Part("Stone foundation course",PrimitiveType.Cube,new Vector3(side*.22f,.18f,-.06f),new Vector3(.25f,.13f,.45f),dark);
                Part("Copper anchor",PrimitiveType.Cube,new Vector3(side*.25f,.235f,.10f),new Vector3(.075f,.06f,.15f),trim);
            }
            var pivot=new GameObject(Role+" weapon");pivot.transform.SetParent(transform,false);weapon=pivot.transform;
            if(slot==0) { // A little workshop lantern, built around one short bellmouth.
                FoundryShape("Lantern copper hood",game.Models.Bell,new Vector3(0,.52f,-.06f),new Vector3(.46f,.55f,.43f),shell,weapon);
                FoundryShape("Lantern glass",game.Models.Shell,new Vector3(0,.61f,-.07f),new Vector3(.26f,.35f,.25f),accent,weapon);
                Loop("Carrying handle",new Vector3(0,.93f,-.06f),.32f,Quaternion.Euler(90,0,0),trim,weapon);
                FoundryMuzzle(new Vector3(0,.48f,.18f),.28f,.32f,trim,dark,weapon);
                for(int side=-1;side<=1;side+=2)Strut("Lantern cage",new Vector3(side*.18f,.35f,-.04f),new Vector3(side*.13f,.77f,-.04f),.045f,trim,weapon);
            } else if(slot==1) { // An open hammer press; the hanging ram makes the silhouette.
                for(int side=-1;side<=1;side+=2){
                    FoundryPipe("Swept press yoke",new[]{new Vector3(side*.30f,.24f,0),new Vector3(side*.31f,.72f,0),new Vector3(side*.23f,1.04f,0),new Vector3(0,1.10f,0)},.11f,shell,weapon);
                    FoundryShape("Press knuckle",game.Models.Shell,new Vector3(side*.30f,.67f,0),Vector3.one*.19f,trim,weapon);
                }
                Strut("Hammer suspension",new Vector3(0,1.05f,0),new Vector3(0,.72f,0),.09f,trim,weapon);
                Part("Ironhand ram",PrimitiveType.Cube,new Vector3(0,.66f,.02f),new Vector3(.35f,.32f,.35f),shell,weapon);
                for(int finger=-1;finger<=1;finger++)Part("Hammer tooth",PrimitiveType.Cube,new Vector3(finger*.10f,.50f,.15f),new Vector3(.07f,.13f,.13f),trim,weapon);
                FoundryMuzzle(new Vector3(0,.65f,.22f),.17f,.14f,trim,dark,weapon);
                Part("Slate anvil",PrimitiveType.Cube,new Vector3(0,.29f,0),new Vector3(.44f,.12f,.40f),dark,weapon);
            } else if(slot==2) { // Exposed toothed wheel, instead of another helmet and arm gun.
                Strut("Shear bearing",new Vector3(0,.25f,0),new Vector3(0,.70f,0),.22f,shell,weapon);
                var wheel=Motion("Foundry cutting wheel",new Vector3(0,.94f,0),Quaternion.Euler(75,0,0),34);
                Loop("Cast wheel rim",Vector3.zero,.71f,Quaternion.identity,trim,wheel);
                for(int i=0;i<8;i++){
                    float a=i*Mathf.PI/4;var outward=new Vector3(Mathf.Sin(a),0,Mathf.Cos(a));
                    Strut("Shear spoke",Vector3.zero,outward*.32f,.045f,shell,wheel);
                    var tooth=Crystal("Forged cutting tooth",outward*.38f,new Vector3(.12f,.13f,.27f),trim,wheel);
                    tooth.transform.localRotation=Quaternion.Euler(90,i*45,0);
                }
                FoundryShape("Wheel hub",game.Models.Shell,Vector3.zero,new Vector3(.22f,.17f,.22f),accent,wheel);
            } else if(slot==3) { // A long sighting instrument in a curved cast cradle.
                for(int side=-1;side<=1;side+=2)FoundryPipe("Ranger cradle",new[]{new Vector3(side*.24f,.23f,-.12f),new Vector3(side*.28f,.49f,-.05f),new Vector3(side*.18f,.81f,0)},.095f,shell,weapon);
                FoundryMuzzle(new Vector3(0,.83f,.16f),.24f,.64f,trim,dark,weapon);
                FoundryShape("Ranger rear chamber",game.Models.Shell,new Vector3(0,.83f,-.25f),new Vector3(.29f,.28f,.38f),shell,weapon);
                Loop("Sighting halo",new Vector3(0,1.06f,.06f),.20f,Quaternion.Euler(90,0,0),trim,weapon);
                Part("Sighting glass",PrimitiveType.Sphere,new Vector3(0,1.06f,.08f),new Vector3(.10f,.10f,.035f),accent,weapon);
                Loop("Elevation handwheel",new Vector3(-.23f,.55f,-.06f),.24f,Quaternion.Euler(0,0,90),trim,weapon);
            } else if(slot==4) { // A three-pronged sky beacon with a small bright flame at its heart.
                FoundryShape("Beacon cup",game.Models.Bell,new Vector3(0,.47f,0),new Vector3(.52f,.43f,.52f),shell,weapon);
                for(int i=0;i<3;i++){
                    float a=i*Mathf.PI*2/3;var direction=new Vector3(Mathf.Sin(a),0,Mathf.Cos(a));
                    FoundryPipe("Beacon petal",new[]{direction*.19f+Vector3.up*.38f,direction*.30f+Vector3.up*.78f,direction*.23f+Vector3.up*1.15f,direction*.13f+Vector3.up*1.34f},.08f,trim,weapon);
                    FoundryShape("Beacon verdigris shoulder",game.Models.Shell,direction*.28f+Vector3.up*.77f,new Vector3(.14f,.32f,.14f),accent,weapon);
                }
                FoundryShape("Sky glass",game.Models.Shell,new Vector3(0,.86f,0),new Vector3(.24f,.46f,.24f),accent,weapon);
                Crystal("Beacon flame",new Vector3(0,1.09f,0),new Vector3(.15f,.38f,.15f),light,weapon);
            } else if(slot==5) { // A frost-filled apothecary vessel, copper bands and paired return pipes.
                FoundryShape("Rime glass vessel",game.Models.Shell,new Vector3(0,.76f,0),new Vector3(.45f,.84f,.40f),accent,weapon);
                FoundryShape("Vessel lid",game.Models.Bell,new Vector3(0,1.20f,0),new Vector3(.43f,.27f,.41f),shell,weapon);
                for(int ring=0;ring<3;ring++)Loop("Vessel copper band",new Vector3(0,.46f+ring*.25f,0),.46f,Quaternion.identity,trim,weapon);
                for(int side=-1;side<=1;side+=2)FoundryPipe("Frost return pipe",new[]{new Vector3(side*.14f,.31f,.10f),new Vector3(side*.33f,.43f,.08f),new Vector3(side*.33f,.91f,.02f),new Vector3(side*.19f,1.07f,0)},.065f,shell,weapon);
                FoundryMuzzle(new Vector3(0,.58f,.27f),.25f,.25f,trim,dark,weapon);
                Crystal("Bound frost heart",new Vector3(0,.79f,.20f),new Vector3(.12f,.30f,.04f),light,weapon);
            } else { // A great wardbell carried in two sweeping cast arches.
                for(int side=-1;side<=1;side+=2)FoundryPipe("Echo bell arch",new[]{new Vector3(side*.33f,.25f,-.08f),new Vector3(side*.35f,.75f,-.10f),new Vector3(side*.28f,1.22f,-.06f),new Vector3(side*.12f,1.48f,0),new Vector3(0,1.50f,0)},.13f,shell,weapon);
                var bell=Motion("Echo wardbell",new Vector3(0,1.25f,0),Quaternion.identity,70);kineticSwing=true;
                FoundryShape("Great cast bell",game.Models.Bell,new Vector3(0,-.34f,0),new Vector3(.55f,.70f,.55f),trim,bell);
                Loop("Bell verdigris rim",new Vector3(0,-.63f,0),.55f,Quaternion.identity,accent,bell);
                Strut("Bell clapper",new Vector3(0,-.10f,0),new Vector3(0,-.68f,0),.05f,dark,bell);
                FoundryShape("Bell glass tongue",game.Models.Shell,new Vector3(0,-.69f,0),new Vector3(.12f,.17f,.12f),light,bell);
                for(int side=-1;side<=1;side+=2)FoundryMuzzle(new Vector3(side*.24f,.44f,.17f),.21f,.26f,trim,dark,weapon);
                Loop("Crown carrying ring",new Vector3(0,1.62f,0),.19f,Quaternion.Euler(90,0,0),trim,weapon);
            }
        }
    }
}
