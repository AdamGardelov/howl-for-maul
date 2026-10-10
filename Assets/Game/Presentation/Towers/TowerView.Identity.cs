using UnityEngine;
namespace FrostMaze
{
    public sealed partial class TowerView
    {
        Transform kinetic;Quaternion kineticRest;float kineticSpeed;bool kineticSwing;
        void FitSilhouette()
        {
            // Keep even diagonal weapon turns and upgrades inside the actual occupied cell.
            // Normalize horizontal geometry only; the order's height and negative space survive.
            float radius=.47f;var vertices=new System.Collections.Generic.List<Vector3>();
            foreach(var filter in GetComponentsInChildren<MeshFilter>()){
                var matrix=transform.worldToLocalMatrix*filter.transform.localToWorldMatrix;
                filter.sharedMesh.GetVertices(vertices);
                foreach(var vertex in vertices){var p=matrix.MultiplyPoint3x4(vertex);radius=Mathf.Max(radius,new Vector2(p.x,p.z).magnitude);}
            }
            float scale=(livingCrown!=null?.43f:.47f)/radius;
            // Fit the weapon in tower axes. Static pieces receive the same matrix during batching:
            // scaling a slanted piece's local X/Z leaves its tilted local Y sticking outside the cell.
            if(weapon!=null)weapon.localScale=new Vector3(scale,weaponHeight,scale);
            weaponWidth=scale;
        }
        void FactionFooting(Material trim,Material dark,int faction,bool iron)
        {
            bool stone=!iron&&(faction==0||faction==1);
            var foot=Part("Foundation",stone?PrimitiveType.Cylinder:PrimitiveType.Cube,new Vector3(0,.045f,0),new Vector3(stone?.84f:.64f,.04f,stone?.84f:.60f),dark);
            if(!iron&&faction==1)foot.GetComponent<MeshFilter>().sharedMesh=game.Models.Shell;
            if(iron&&(faction==2||faction==4||faction==7)){foot.GetComponent<MeshFilter>().sharedMesh=game.Models.Column;foot.transform.localScale=new Vector3(.60f,.035f,.60f);}
            if(!iron&&faction==0)Part("Snow drift",PrimitiveType.Sphere,new Vector3(-.20f,.12f,-.14f),new Vector3(.56f,.18f,.47f),trim);
        }
        Transform Motion(string name,Vector3 position,Quaternion rotation,float speed)
        {
            kinetic=new GameObject(name).transform;kinetic.SetParent(weapon??transform,false);kinetic.localPosition=position;
            kinetic.localRotation=kineticRest=rotation;kineticSpeed=speed;return kinetic;
        }
        GameObject Loop(string name,Vector3 p,float diameter,Quaternion rotation,Material material,Transform parent=null)
        {
            var loop=Part(name,PrimitiveType.Sphere,p,Vector3.one*diameter,material,parent);
            loop.GetComponent<MeshFilter>().sharedMesh=game.Models.Halo;loop.transform.localRotation=rotation;return loop;
        }
        void Strut(string name,Vector3 a,Vector3 b,float width,Material material,Transform parent=null)
        {
            var beam=Part(name,PrimitiveType.Cylinder,(a+b)*.5f,new Vector3(width,(b-a).magnitude*.5f,width),material,parent);
            beam.transform.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);
        }
        void Shape(string name,Mesh mesh,Vector3 scale)
        {
            var item=(weapon??transform).Find(name);if(item==null)return;
            item.GetComponent<MeshFilter>().sharedMesh=mesh;item.localScale=scale;
        }
        void LowerWeapon(float height,float offset=0)
        {
            if(weapon==null)return;
            foreach(Transform child in weapon){var p=child.localPosition;p.y=p.y*height+offset;child.localPosition=p;var s=child.localScale;s.y*=height;child.localScale=s;}
        }
        void FactionArchitecture(Material trim,Material dark,int faction,bool iron)
        {
            if(!iron){WinterArchitecture(trim,dark,faction);return;}
            int slot=Subject.Design%7;
            if(faction==0){ // Pulse instruments carry their own masonry beds and cast frames.
                return;
            }else if(faction==1){ // Blast: low six-legged siege beetles, not upright soldiers.
                for(int side=-1;side<=1;side+=2)for(int leg=-1;leg<=1;leg++){
                    var knee=new Vector3(side*.34f,.29f,leg*.22f);
                    Strut("Beetle thigh",new Vector3(side*.17f,.48f,leg*.17f),knee,.17f,trim);
                    Strut("Beetle claw",knee,new Vector3(side*.41f,.065f,leg*.25f),.12f,dark);
                    Part("Siege talon",PrimitiveType.Cube,new Vector3(side*.40f,.08f,leg*.25f+.035f),new Vector3(.16f,.12f,.19f),shell);
                }
                Part("Siege undercarriage",PrimitiveType.Sphere,new Vector3(0,.36f,-.04f),new Vector3(.56f,.21f,.48f),dark);
                Part("Copper turntable",PrimitiveType.Cylinder,new Vector3(0,.46f,0),new Vector3(.42f,.055f,.42f),trim);
            }else if(faction==2){ // Prism: suspended glass with a broken ivory shrine around it.
                Shape("Prism waist",game.Models.Crystal,new Vector3(.16f,.32f,.16f));
                Shape("Prism cuirass",game.Models.Crystal,new Vector3(.33f,.54f,.30f));
                Shape("Champion carapace",game.Models.Crystal,new Vector3(.63f,.83f,.45f));
                if(slot==4||slot==6)for(int side=-1;side<=1;side+=2){
                    var shard=Crystal("Ivory prism petal",new Vector3(side*.28f,.64f,-.09f),new Vector3(.21f,1.1f,.35f),trim);
                    shard.transform.localRotation=Quaternion.Euler(-12,0,-side*17);
                }
                if(slot==3){
                    var orbit=Motion("Prism light orbit",new Vector3(0,.83f,0),Quaternion.Euler(16,0,12),14);
                    Loop("Prism orbit",Vector3.zero,.78f,Quaternion.identity,accent,orbit);
                    Crystal("Orbiting lens",new Vector3(.33f,0,0),new Vector3(.12f,.22f,.12f),light,orbit);
                }
            }else if(faction==3){ // Shared joinery; each weapon supplies its own silhouette.
                if(Role=="Champion") {
                    for(int side=-1;side<=1;side+=2)for(int end=-1;end<=1;end+=2) {
                        var knee=new Vector3(side*.30f,.31f,end*.24f);
                        Strut("Crawler oak thigh",new Vector3(side*.16f,.63f,end*.12f),knee,.17f,shell);
                        Strut("Crawler brass foot",knee,new Vector3(side*.34f,.07f,end*.31f),.10f,trim);
                    }
                } else {
                    for(int i=0;i<3;i++) {
                        float a=i*Mathf.PI*2/3;
                        var foot=new Vector3(Mathf.Sin(a)*.33f,.07f,Mathf.Cos(a)*.33f);
                        Strut("Survey tripod",foot,new Vector3(0,.71f,0),.12f,shell);
                        Part("Brass tripod shoe",PrimitiveType.Sphere,foot,new Vector3(.16f,.13f,.16f),trim);
                    }
                }
                Part("Survey turntable",PrimitiveType.Cylinder,new Vector3(0,.66f,0),new Vector3(.36f,.045f,.36f),trim);
            }else if(faction==4){ // Gravity: open mechanical orreries with visible empty space.
                Shape("Gravity housing",game.Models.Crystal,new Vector3(.31f,.40f,.31f));
                Shape("Core mounting",game.Models.Crystal,new Vector3(.21f,.25f,.21f));
                for(int i=0;i<3;i++){float a=i*Mathf.PI*2/3;
                    Crystal("Floating anchor stone",new Vector3(Mathf.Sin(a)*.29f,.26f,Mathf.Cos(a)*.29f),new Vector3(.25f,.4f,.27f),shell);
                }
                if(slot==2||slot==6){
                    var orbit=Motion("Gravity gimbal",new Vector3(0,slot==2?1.27f:.92f,0),Quaternion.Euler(slot==2?15:63,0,20),22);
                    Loop("Rotating gravity halo",Vector3.zero,.88f,Quaternion.identity,trim,orbit);
                    if(slot==6)Loop("Crossed gravity halo",Vector3.zero,.73f,Quaternion.Euler(68,0,30),accent,orbit);
                    Part("Orbital counterweight",PrimitiveType.Sphere,new Vector3(.4f,0,0),Vector3.one*.16f,light,orbit);
                }
            }else if(faction==5){ // Scrap: improvised wheeled salvage rigs.
                LowerWeapon(.84f,-.08f);
                Part("Salvage cart bed",PrimitiveType.Cube,new Vector3(0,.25f,0),new Vector3(.72f,.18f,.70f),shell);
                for(int side=-1;side<=1;side+=2)for(int axle=-1;axle<=1;axle+=2){
                    var wheel=Part("Cart wheel",PrimitiveType.Cylinder,new Vector3(side*.34f,.19f,axle*.24f),new Vector3(.31f,.065f,.31f),dark);
                    wheel.transform.localRotation=Quaternion.Euler(0,0,90);
                    var hub=Part("Cart bronze hub",PrimitiveType.Cylinder,new Vector3(side*.405f,.19f,axle*.24f),new Vector3(.13f,.015f,.13f),trim);hub.transform.localRotation=wheel.transform.localRotation;
                }
                if(slot==5){
                    Strut("Scrap crane",new Vector3(-.28f,.25f,-.24f),new Vector3(-.22f,1.34f,-.16f),.10f,accent);
                    Strut("Crane jib",new Vector3(-.22f,1.34f,-.16f),new Vector3(.12f,1.34f,-.10f),.09f,accent);
                    Strut("Suspended salvage",new Vector3(.12f,1.34f,-.10f),new Vector3(.12f,1.0f,-.10f),.025f,dark);
                }
            }else if(faction==6){ // Overdrive: squat iron drakes, furnace jaws and folded wings.
                LowerWeapon(.9f,-.10f);
                for(int side=-1;side<=1;side+=2){
                    Part("Drake haunch",PrimitiveType.Sphere,new Vector3(side*.26f,.40f,-.09f),new Vector3(.32f,.43f,.36f),shell,weapon);
                    Part("Drake foreclaw",PrimitiveType.Cube,new Vector3(side*.26f,.14f,.22f),new Vector3(.22f,.19f,.33f),trim,weapon);
                    if(slot==4||slot==6){
                        var wing=Part("Folded furnace wing",PrimitiveType.Sphere,new Vector3(side*.22f,.92f,-.10f),new Vector3(.45f,1.5f,.75f),accent,weapon);
                        wing.GetComponent<MeshFilter>().sharedMesh=game.Models.Wing(side);wing.transform.localRotation=Quaternion.Euler(25,0,side*(slot==4?38:67));
                    }
                    Crystal("Drake tooth",new Vector3(side*.13f,.77f,.31f),new Vector3(.085f,.23f,.085f),trim,weapon);
                }
                for(int i=0;i<3;i++)Crystal("Dorsal furnace spine",new Vector3(0,.62f+i*.12f,-.36f+i*.10f),new Vector3(.13f,.32f,.19f),accent,weapon);
            }else{ // Tidal: shell cradles and slow working waterwheels.
                Shape("Tidal pressure hull",game.Models.Bell,new Vector3(.63f,.45f,.63f));
                Shape("Core mounting",game.Models.Shell,new Vector3(.45f,.20f,.45f));
                if(slot==0||slot==6)for(int side=-1;side<=1;side+=2){
                    var shellPiece=Crystal("Pearl shell petal",new Vector3(side*.27f,.51f,0),new Vector3(.27f,.83f,.62f),trim);
                    shellPiece.transform.localRotation=Quaternion.Euler(0,0,-side*25);
                }
                if(slot==3){
                    var wheel=Motion("Working tidewheel",new Vector3(0,.57f,-.18f),Quaternion.Euler(90,0,0),-18);
                    Loop("Tidewheel rim",Vector3.zero,.88f,Quaternion.identity,shell,wheel);
                    for(int i=0;i<8;i++){float a=i*Mathf.PI/4;var end=new Vector3(Mathf.Sin(a)*.39f,0,Mathf.Cos(a)*.39f);
                        Strut("Tidewheel spoke",Vector3.zero,end,.04f,trim,wheel);
                        var paddle=Part("Copper water scoop",PrimitiveType.Cube,end,new Vector3(.18f,.17f,.10f),accent,wheel);paddle.transform.localRotation=Quaternion.Euler(0,i*45,0);
                    }
                }
            }
        }
        void WinterArchitecture(Material trim,Material dark,int faction)
        {
            if(faction==0){
                if(Role=="Wall")return;
                if(Role=="Artillery"){
                    Shape("Basin plinth",game.Models.Bell,new Vector3(.62f,.40f,.62f));
                    Loop("Hail bell yoke",new Vector3(0,.91f,0),.89f,Quaternion.Euler(90,0,0),trim);
                    Strut("Bell suspender",new Vector3(0,1.34f,0),new Vector3(0,1.02f,0),.055f,dark);
                }else if(Role=="Control"||Role=="Interceptor"){
                    for(int side=-1;side<=1;side+=2){var rib=Crystal("Swept frost petal",new Vector3(side*.31f,.62f,-.12f),new Vector3(.18f,.89f,.32f),trim);rib.transform.localRotation=Quaternion.Euler(-18,0,-side*19);}
                }
            }else if(faction==1){
                return; // Living trees author their own roots, limbs and crowns.
            }else if(faction==2){
                if(Role=="Wall")return;
                Part("Brick forge hearth",PrimitiveType.Cube,new Vector3(0,.21f,0),new Vector3(.74f,.25f,.70f),shell);
                for(int side=-1;side<=1;side+=2){
                    Strut("Wrought iron handle",new Vector3(side*.31f,.36f,0),new Vector3(side*.33f,.95f,-.04f),.09f,dark);
                    Loop("Forge handle ring",new Vector3(side*.31f,.79f,-.04f),.28f,Quaternion.Euler(0,0,90),trim);
                }
            }else{
                if(Role=="Wall")return;
                for(int side=-1;side<=1;side+=2){
                    Part("Conductor foot",PrimitiveType.Cube,new Vector3(side*.27f,.12f,-.11f),new Vector3(.25f,.15f,.38f),dark);
                    Strut("Copper earth cable",new Vector3(side*.27f,.18f,-.1f),new Vector3(side*.26f,.65f,-.19f),.07f,trim);
                }
                if(Role=="Relay")Loop("Storm induction halo",new Vector3(0,1.08f,0),.82f,Quaternion.Euler(90,0,0),trim,weapon);
            }
        }
    }
}
