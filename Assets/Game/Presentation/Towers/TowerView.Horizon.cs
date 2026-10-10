using UnityEngine;
namespace FrostMaze
{
    public sealed partial class TowerView
    {
        // Long-range survey instruments made from oak, brass and amber signal glass.
        // Bows, sights and winches carry the role at the ordinary playing camera.
        void HorizonTube(string name,Vector3 from,Vector3 to,float diameter,Material metal,Material dark)
        {
            Strut(name,from,to,diameter,metal,weapon);
            var direction=(to-from).normalized;
            Strut(name+" open mouth",to,to+direction*.012f,diameter*.76f,dark,weapon);
            Strut(name+" lens",to+direction*.014f,to+direction*.019f,diameter*.47f,light,weapon);
        }
        void HorizonBow(string name,Vector3 at,float span,float length,Material trim,Material dark)
        {
            Part(name+" oak rail",PrimitiveType.Cube,at,new Vector3(.12f,.11f,length),shell,weapon);
            for(int side=-1;side<=1;side+=2) {
                var elbow=at+new Vector3(side*span*.32f,.035f,length*.20f);
                var tip=at+new Vector3(side*span*.5f,.08f,length*.08f);
                Strut(name+" inner limb",at+new Vector3(0,0,length*.27f),elbow,.095f,shell,weapon);
                Strut(name+" brass tip",elbow,tip,.065f,trim,weapon);
                Strut(name+" tension string",tip,at+new Vector3(0,.05f,-length*.36f),.014f,dark,weapon);
            }
            Strut(name+" bolt",at+new Vector3(0,.085f,-length*.33f),at+new Vector3(0,.085f,length*.44f),.037f,trim,weapon);
            var point=Crystal(name+" bolt head",at+new Vector3(0,.085f,length*.49f),new Vector3(.075f,.17f,.075f),accent,weapon);
            point.transform.localRotation=Quaternion.Euler(90,0,0);
        }
        void HorizonTower(Material trim,Material dark,int slot)
        {
            var pivot=new GameObject(Role+" weapon");pivot.transform.SetParent(transform,false);weapon=pivot.transform;
            if(slot==0) { // Small, low and precise: the field scope is the inexpensive opener.
                Part("Glimmer saddle",PrimitiveType.Cube,new Vector3(0,.76f,0),new Vector3(.30f,.19f,.23f),shell,weapon);
                HorizonTube("Glimmer scope",new Vector3(0,.93f,-.29f),new Vector3(0,1.01f,.32f),.23f,trim,dark);
                for(int side=-1;side<=1;side+=2)Loop("Scope elevation wheel",new Vector3(side*.19f,.83f,-.01f),.21f,Quaternion.Euler(0,0,90),accent,weapon);
            } else if(slot==1) { // A tall open sun mirror, not another cannon.
                Strut("Regent mast",new Vector3(0,.69f,0),new Vector3(0,1.23f,0),.14f,shell,weapon);
                Loop("Solar crown",new Vector3(0,1.27f,0),.64f,Quaternion.Euler(90,0,0),trim,weapon);
                Part("Regent amber lens",PrimitiveType.Sphere,new Vector3(0,1.27f,.02f),new Vector3(.32f,.32f,.10f),light,weapon);
                for(int i=0;i<8;i++) {
                    float a=i*Mathf.PI/4;
                    Strut("Sun spoke",new Vector3(Mathf.Sin(a)*.20f,1.27f+Mathf.Cos(a)*.20f,0),new Vector3(Mathf.Sin(a)*.34f,1.27f+Mathf.Cos(a)*.34f,0),.04f,accent,weapon);
                }
                Part("Regent pennant",PrimitiveType.Cube,new Vector3(0,.94f,-.08f),new Vector3(.22f,.29f,.025f),accent,weapon);
            } else if(slot==2) { // Broad horizontal repeater with a visible drum magazine.
                Part("Dust intake",PrimitiveType.Cube,new Vector3(0,.78f,0),new Vector3(.39f,.22f,.43f),shell,weapon);
                HorizonBow("Dustkeeper crossbow",new Vector3(0,.96f,0),.82f,.68f,trim,dark);
                var drum=Part("Repeater magazine",PrimitiveType.Cylinder,new Vector3(0,.80f,-.20f),new Vector3(.35f,.20f,.35f),accent,weapon);
                drum.transform.localRotation=Quaternion.Euler(0,0,90);
                for(int side=-1;side<=1;side+=2)Loop("Magazine rim",new Vector3(side*.21f,.80f,-.20f),.34f,Quaternion.Euler(0,0,90),trim,weapon);
            } else if(slot==3) { // Shielded watchbow: pale ribs form a tall split mantlet.
                HorizonBow("Boneplate watchbow",new Vector3(0,1.03f,0),.60f,.71f,trim,dark);
                for(int side=-1;side<=1;side+=2)for(int i=0;i<3;i++) {
                    var plate=Part("Boneplate rib",PrimitiveType.Cube,new Vector3(side*(.23f-i*.025f),.82f+i*.17f,.17f),new Vector3(.21f,.19f,.12f),trim,weapon);
                    plate.transform.localRotation=Quaternion.Euler(0,side*-18,side*10);
                }
                HorizonTube("Watchbow sight",new Vector3(0,1.23f,-.16f),new Vector3(0,1.23f,.17f),.11f,accent,dark);
            } else if(slot==4) { // Air specialist: two elevated harpoons and empty space between.
                Part("Deepdiver yoke",PrimitiveType.Cube,new Vector3(0,.83f,0),new Vector3(.58f,.15f,.22f),shell,weapon);
                for(int side=-1;side<=1;side+=2) {
                    var lower=new Vector3(side*.23f,.82f,-.13f);var upper=new Vector3(side*.23f,1.52f,.15f);
                    HorizonTube("Sky harpoon",lower,upper,.12f,accent,dark);
                    Strut("Harpoon outer limb",lower+new Vector3(side*.14f,.22f,0),upper-new Vector3(0,.19f,0),.08f,trim,weapon);
                    Strut("Harpoon cable",lower+new Vector3(side*.14f,.22f,0),lower,.015f,dark,weapon);
                }
                Loop("Skyfinder reticle",new Vector3(0,1.11f,.09f),.28f,Quaternion.Euler(65,0,0),trim,weapon);
            } else if(slot==5) { // Heavy auger on an exposed geared cradle.
                Part("Drill yoke",PrimitiveType.Cube,new Vector3(0,.84f,0),new Vector3(.55f,.24f,.44f),shell,weapon);
                HorizonTube("Warden axle",new Vector3(0,1.06f,-.30f),new Vector3(0,1.13f,.25f),.20f,dark,dark);
                for(int i=0;i<4;i++)Loop("Auger flight",new Vector3(0,1.07f+i*.018f,-.17f+i*.12f),.43f-i*.055f,Quaternion.Euler(82,0,i*25),trim,weapon);
                var drill=Crystal("Warden drill",new Vector3(0,1.14f,.34f),new Vector3(.20f,.33f,.20f),accent,weapon);drill.transform.localRotation=Quaternion.Euler(82,0,0);
                for(int side=-1;side<=1;side+=2)Loop("Drill winding wheel",new Vector3(side*.29f,.91f,-.07f),.30f,Quaternion.Euler(0,0,90),trim,weapon);
            } else { // Champion: a walking double ballista and the order's high standard.
                Part("Crawler hull",PrimitiveType.Cube,new Vector3(0,.73f,0),new Vector3(.62f,.24f,.57f),shell,weapon);
                HorizonBow("Lower siege bow",new Vector3(0,.98f,.01f),.85f,.74f,trim,dark);
                HorizonBow("Upper siege bow",new Vector3(0,1.28f,.01f),.70f,.67f,trim,dark);
                for(int side=-1;side<=1;side+=2) {
                    Strut("Siege rack",new Vector3(side*.22f,.77f,-.19f),new Vector3(side*.22f,1.32f,-.19f),.09f,trim,weapon);
                    Part("Crawler shoulder",PrimitiveType.Sphere,new Vector3(side*.28f,.79f,0),new Vector3(.23f,.24f,.45f),accent,weapon);
                }
                Strut("Guild standard",new Vector3(0,.72f,-.28f),new Vector3(0,1.84f,-.28f),.055f,trim,weapon);
                Part("Guild banner",PrimitiveType.Cube,new Vector3(.15f,1.57f,-.29f),new Vector3(.29f,.37f,.027f),accent,weapon);
                Part("Banner sun",PrimitiveType.Sphere,new Vector3(.15f,1.59f,-.268f),new Vector3(.13f,.13f,.014f),light,weapon);
                Loop("Champion sun finial",new Vector3(0,1.83f,-.28f),.19f,Quaternion.Euler(90,0,0),trim,weapon);
            }
        }
    }
}
