using UnityEngine;
using FrostMaze.Simulation;
namespace FrostMaze
{
    // All shapes are original, cosmetic and kept inside the simulation footprint.
    public sealed partial class TowerView : MonoBehaviour
    {
        public string Role { get; private set; }
        public int VisibleLevel { get; private set; }
        Transform weapon;
        float weaponWidth=1;
        Prototype game;
        long observedShot,shotTick=-100;

        GameObject[] tiers=new GameObject[2];
        Material shell,accent,light;
        Mesh Faceted(PrimitiveType kind) => kind==PrimitiveType.Cylinder?game.Models.Column:game.Models.Shell;
        GameObject Crystal(string name,Vector3 position,Vector3 scale,Material material,Transform parent=null) {
            var part=Part(name,PrimitiveType.Sphere,position,scale,material,parent);part.GetComponent<MeshFilter>().sharedMesh=game.Models.Crystal;return part;
        }
        void RimeTower(Material trim,Material dark)
        {
            Part("Snow lip",PrimitiveType.Cylinder,new Vector3(0,.28f,0),new Vector3(.84f,.035f,.84f),trim);
            if(Role=="Wall") {
                for(int i=0;i<3;i++){var rock=Part("Cairn stone",PrimitiveType.Sphere,new Vector3(i==1?.05f:-.03f,.38f+i*.17f,0),new Vector3(.84f-i*.16f,.35f,.72f-i*.12f),i==2?trim:shell);rock.transform.localRotation=Quaternion.Euler(0,i*31,0);}
                Crystal("Cairn rune",new Vector3(0,.57f,.32f),new Vector3(.14f,.22f,.08f),light);return;
            }
            var pivot=new GameObject(Role+" weapon");pivot.transform.SetParent(transform,false);weapon=pivot.transform;
            if(Role=="Artillery") {
                Part("Basin plinth",PrimitiveType.Cylinder,new Vector3(0,.43f,0),new Vector3(.7f,.16f,.7f),shell,weapon);
                Part("Dark basin",PrimitiveType.Cylinder,new Vector3(0,.65f,0),new Vector3(.59f,.045f,.59f),dark,weapon);
                for(int i=0;i<6;i++){float a=i*Mathf.PI/3;var rim=Part("Basin rim",PrimitiveType.Cube,new Vector3(Mathf.Sin(a)*.3f,.7f,Mathf.Cos(a)*.3f),new Vector3(.3f,.2f,.12f),trim,weapon);rim.transform.localRotation=Quaternion.Euler(0,i*60,0);}
                Crystal("Hail charge",new Vector3(0,.88f,0),new Vector3(.3f,.43f,.3f),accent,weapon);
            } else if(Role=="Control") {
                Part("Obelisk foot",PrimitiveType.Cylinder,new Vector3(0,.43f,0),new Vector3(.64f,.12f,.64f),shell,weapon);
                Crystal("Rime heart",new Vector3(0,1.08f,0),new Vector3(.44f,1.22f,.44f),accent,weapon);
                for(int side=-1;side<=1;side+=2){var claw=Part("Stone claw",PrimitiveType.Cylinder,new Vector3(side*.29f,.71f,0),new Vector3(.17f,.34f,.17f),trim,weapon);claw.transform.localRotation=Quaternion.Euler(0,0,side*-13);}
                Crystal("Heart glint",new Vector3(0,1.15f,.2f),new Vector3(.09f,.27f,.035f),light,weapon);
            } else if(Role=="Interceptor") {
                Part("Sky pedestal",PrimitiveType.Cylinder,new Vector3(0,.49f,0),new Vector3(.56f,.22f,.56f),shell,weapon);
                for(int side=-1;side<=1;side+=2){Part("Needle socket",PrimitiveType.Cylinder,new Vector3(side*.22f,.82f,0),new Vector3(.26f,.19f,.26f),trim,weapon);Crystal("Aurora spire",new Vector3(side*.22f,1.23f,0),new Vector3(.22f,.88f,.22f),accent,weapon);}
                Part("Sky sight",PrimitiveType.Sphere,new Vector3(0,.95f,.12f),new Vector3(.17f,.17f,.17f),light,weapon);
            } else {
                Part("Watchtower stone",PrimitiveType.Cylinder,new Vector3(0,.61f,0),new Vector3(.6f,.29f,.6f),shell,weapon);
                Part("Watchtower cornice",PrimitiveType.Cylinder,new Vector3(0,.91f,0),new Vector3(.74f,.065f,.74f),trim,weapon);
                for(int side=-1;side<=1;side+=2)Part("Crown stone",PrimitiveType.Cube,new Vector3(side*.26f,1.09f,-.03f),new Vector3(.17f,.3f,.42f),shell,weapon);
                var shard=Crystal("Shard launcher",new Vector3(0,1.08f,.12f),new Vector3(.29f,.67f,.29f),accent,weapon);shard.transform.localRotation=Quaternion.Euler(64,0,0);
                Crystal("Watchtower sigil",new Vector3(0,.64f,.3f),new Vector3(.12f,.26f,.06f),light,weapon);
            }
        }
        void StoneTower(Material trim,Material dark,bool pebble)
        {
            if(Role=="Wall") {
                for(int i=-1;i<=1;i++) {
                    Part("Basalt slab",PrimitiveType.Cylinder,new Vector3(i*.25f,.49f,0),new Vector3(.36f,i==0?.34f:.26f,.7f),shell);
                    Part("Basalt seal",PrimitiveType.Cube,new Vector3(i*.25f,.56f,.33f),new Vector3(.09f,.2f,.035f),accent);
                }
                return;
            }
            var pivot=new GameObject(Role+" weapon");pivot.transform.SetParent(transform,false);weapon=pivot.transform;
            if(Role=="Control") {
                Part("Worldroot trunk",PrimitiveType.Cylinder,new Vector3(0,.76f,0),new Vector3(.43f,.45f,.43f),dark,weapon);
                for(int i=0;i<4;i++) {
                    float a=i*Mathf.PI/2;
                    var root=Part("Stone root",PrimitiveType.Cylinder,new Vector3(Mathf.Sin(a)*.22f,.46f,Mathf.Cos(a)*.22f),new Vector3(.18f,.28f,.18f),shell,weapon);
                    root.transform.localRotation=Quaternion.Euler(Mathf.Cos(a)*35,0,-Mathf.Sin(a)*35);
                }
                Part("Root crown",PrimitiveType.Sphere,new Vector3(0,1.21f,0),new Vector3(.72f,.49f,.64f),accent,weapon);
                Crystal("Root heart",new Vector3(0,.85f,.23f),new Vector3(.17f,.36f,.1f),light,weapon);
            } else if(Role=="Interceptor") {
                for(int side=-1;side<=1;side+=2)Part("Crag support",PrimitiveType.Cylinder,new Vector3(side*.25f,.77f,0),new Vector3(.2f,.46f,.3f),shell,weapon);
                var cradle=Part("Sky cradle",PrimitiveType.Cube,new Vector3(0,1.12f,.03f),new Vector3(.7f,.15f,.43f),trim,weapon);cradle.transform.localRotation=Quaternion.Euler(25,0,0);
                Part("Sky boulder",PrimitiveType.Sphere,new Vector3(0,1.4f,.06f),new Vector3(.4f,.43f,.4f),accent,weapon);
            } else if(pebble) {
                Part("Warden body",PrimitiveType.Sphere,new Vector3(0,.59f,0),new Vector3(.71f,.63f,.64f),shell,weapon);
                Part("Pebble hopper",PrimitiveType.Cylinder,new Vector3(0,.96f,-.05f),new Vector3(.56f,.12f,.48f),trim,weapon);
                Part("Loaded pebble",PrimitiveType.Sphere,new Vector3(0,1.12f,-.05f),Vector3.one*.28f,accent,weapon);
                Part("Warden aperture",PrimitiveType.Cube,new Vector3(0,.68f,.3f),new Vector3(.28f,.15f,.09f),dark,weapon);
                Part("Warden eye",PrimitiveType.Cube,new Vector3(0,.84f,.28f),new Vector3(.25f,.06f,.05f),light,weapon);
            } else {
                Part("Quake monolith",PrimitiveType.Cube,new Vector3(0,.82f,0),new Vector3(.56f,1.05f,.48f),shell,weapon);
                Part("Idol brow",PrimitiveType.Cube,new Vector3(0,1.21f,.06f),new Vector3(.75f,.18f,.56f),trim,weapon);
                for(int side=-1;side<=1;side+=2)Part("Idol eye",PrimitiveType.Cube,new Vector3(side*.13f,1.05f,.25f),new Vector3(.12f,.07f,.035f),light,weapon);
                Part("Quake seal",PrimitiveType.Cylinder,new Vector3(0,.45f,.29f),new Vector3(.4f,.12f,.25f),accent,weapon);
            }
        }
        void EmberTower(Material trim,Material dark,bool meteor)
        {
            if(Role=="Wall") {
                Part("Coal bunker",PrimitiveType.Cube,new Vector3(0,.44f,0),new Vector3(.76f,.38f,.68f),dark);
                for(int side=-1;side<=1;side+=2)Part("Bunker brace",PrimitiveType.Cube,new Vector3(side*.29f,.51f,0),new Vector3(.11f,.51f,.73f),shell);
                for(int i=-1;i<=1;i++)Part("Banked coal",PrimitiveType.Sphere,new Vector3(i*.2f,.65f,0),new Vector3(.25f,.19f,.37f),accent);
                return;
            }
            var pivot=new GameObject(Role+" weapon");pivot.transform.SetParent(transform,false);weapon=pivot.transform;
            if(Role=="Interceptor") {
                Part("Lance mount",PrimitiveType.Cylinder,new Vector3(0,.58f,0),new Vector3(.48f,.3f,.48f),dark,weapon);
                for(int side=-1;side<=1;side+=2) {
                    var fin=Part("Lance fin",PrimitiveType.Cube,new Vector3(side*.22f,1.03f,0),new Vector3(.13f,.8f,.22f),shell,weapon);fin.transform.localRotation=Quaternion.Euler(0,0,side*12);
                }
                Crystal("Flare spear",new Vector3(0,1.28f,0),new Vector3(.23f,.97f,.23f),accent,weapon);
                Crystal("White hot tip",new Vector3(0,1.64f,0),new Vector3(.1f,.27f,.1f),light,weapon);
            } else if(Role=="Artillery"&&meteor) {
                Part("Crucible foot",PrimitiveType.Cylinder,new Vector3(0,.42f,0),new Vector3(.54f,.15f,.54f),dark,weapon);
                Part("Crucible bowl",PrimitiveType.Sphere,new Vector3(0,.74f,0),new Vector3(.74f,.64f,.74f),shell,weapon);
                Part("Molten pool",PrimitiveType.Cylinder,new Vector3(0,.98f,0),new Vector3(.55f,.045f,.55f),accent,weapon);
                for(int side=-1;side<=1;side+=2)Part("Crucible handle",PrimitiveType.Cube,new Vector3(side*.34f,.85f,0),new Vector3(.1f,.36f,.24f),trim,weapon);
                Crystal("Meteor core",new Vector3(0,1.25f,0),new Vector3(.31f,.43f,.31f),light,weapon);
            } else if(Role=="Artillery") {
                Part("Furnace housing",PrimitiveType.Cube,new Vector3(0,.64f,0),new Vector3(.66f,.68f,.56f),shell,weapon);
                for(int side=-1;side<=1;side+=2) {
                    Part("Furnace chimney",PrimitiveType.Cylinder,new Vector3(side*.22f,1.11f,-.12f),new Vector3(.16f,.3f,.18f),dark,weapon);
                    Part("Chimney ember",PrimitiveType.Cylinder,new Vector3(side*.22f,1.41f,-.12f),new Vector3(.13f,.025f,.14f),accent,weapon);
                }
                Part("Furnace mouth",PrimitiveType.Cube,new Vector3(0,.61f,.29f),new Vector3(.48f,.34f,.05f),dark,weapon);
                for(int i=-1;i<=1;i++)Part("Furnace grate",PrimitiveType.Cube,new Vector3(i*.13f,.62f,.32f),new Vector3(.045f,.26f,.025f),accent,weapon);
            } else {
                Part("Cinder drum",PrimitiveType.Cylinder,new Vector3(0,.69f,0),new Vector3(.49f,.37f,.49f),shell,weapon);
                Part("Cinder crown",PrimitiveType.Cylinder,new Vector3(0,1.08f,0),new Vector3(.6f,.08f,.6f),dark,weapon);
                for(int side=-1;side<=1;side+=2) {
                    var nozzle=Part("Cinder nozzle",PrimitiveType.Cylinder,new Vector3(side*.15f,.9f,.2f),new Vector3(.16f,.21f,.16f),accent,weapon);nozzle.transform.localRotation=Quaternion.Euler(75,0,0);
                    Part("Drum vent",PrimitiveType.Cube,new Vector3(side*.245f,.66f,0),new Vector3(.025f,.31f,.12f),light,weapon);
                }
                Crystal("Pilot flame",new Vector3(0,1.27f,0),new Vector3(.23f,.35f,.23f),accent,weapon);
            }
        }
        void VoltTower(Material trim,Material dark,bool marshal)
        {
            if(Role=="Wall") {
                Part("Scrap barricade",PrimitiveType.Cube,new Vector3(0,.5f,0),new Vector3(.72f,.5f,.6f),dark);
                for(int i=-1;i<=1;i++) {
                    var plate=Part("Scrap plate",PrimitiveType.Cube,new Vector3(i*.22f,.57f,0),new Vector3(.18f,.63f,.67f),shell);plate.transform.localRotation=Quaternion.Euler(0,0,i*8);
                    Part("Power rivet",PrimitiveType.Sphere,new Vector3(i*.22f,.62f,-.34f),Vector3.one*.09f,accent);
                }
                return;
            }
            var pivot=new GameObject(Role+" weapon");pivot.transform.SetParent(transform,false);weapon=pivot.transform;
            if(Role=="Relay") {
                Part("Relay generator",PrimitiveType.Cylinder,new Vector3(0,.51f,0),new Vector3(.59f,.23f,.59f),dark,weapon);
                for(int side=-1;side<=1;side+=2) {
                    Part("Relay electrode",PrimitiveType.Cylinder,new Vector3(side*.23f,.98f,0),new Vector3(.13f,.46f,.13f),shell,weapon);
                    for(int i=0;i<3;i++)Part("Induction ring",PrimitiveType.Cylinder,new Vector3(side*.23f,.78f+i*.17f,0),new Vector3(.27f,.035f,.27f),accent,weapon);
                    Part("Arc terminal",PrimitiveType.Sphere,new Vector3(side*.23f,1.47f,0),Vector3.one*.22f,light,weapon);
                }
            } else if(Role=="Interceptor") {
                Part("Rail turntable",PrimitiveType.Cylinder,new Vector3(0,.55f,0),new Vector3(.64f,.25f,.64f),dark,weapon);
                for(int side=-1;side<=1;side+=2) {
                    var rail=Part("Skyrail conductor",PrimitiveType.Cube,new Vector3(side*.18f,1.11f,.08f),new Vector3(.13f,.96f,.17f),shell,weapon);rail.transform.localRotation=Quaternion.Euler(24,0,0);
                    var strip=Part("Rail charge strip",PrimitiveType.Cube,new Vector3(side*.18f,1.11f,.18f),new Vector3(.065f,.78f,.035f),accent,weapon);strip.transform.localRotation=Quaternion.Euler(24,0,0);
                }
                Part("Rail capacitor",PrimitiveType.Sphere,new Vector3(0,.83f,0),Vector3.one*.28f,light,weapon);
            } else {
                for(int side=-1;side<=1;side+=2)Part("Armored boot",PrimitiveType.Cube,new Vector3(side*.15f,.43f,.03f),new Vector3(.22f,.31f,.35f),dark,weapon);
                Part(marshal?"Marshal chest":"Cadet chest",PrimitiveType.Sphere,new Vector3(0,.84f,0),new Vector3(marshal?.65f:.48f,.57f,.43f),shell,weapon);
                Part("Helmet shell",PrimitiveType.Sphere,new Vector3(0,1.27f,0),new Vector3(.44f,.39f,.4f),accent,weapon);
                Part("Dark visor",PrimitiveType.Cube,new Vector3(0,1.3f,.19f),new Vector3(.31f,.11f,.035f),dark,weapon);
                var cannon=Part("Pulse cannon",PrimitiveType.Cylinder,new Vector3(.29f,.87f,.17f),new Vector3(marshal?.26f:.2f,.25f,marshal?.26f:.2f),accent,weapon);cannon.transform.localRotation=Quaternion.Euler(90,0,0);
                Part("Cannon aperture",PrimitiveType.Sphere,new Vector3(.29f,.87f,.43f),new Vector3(.12f,.12f,.04f),light,weapon);
                if(marshal) {
                    for(int side=-1;side<=1;side+=2)Part("Marshal pauldron",PrimitiveType.Sphere,new Vector3(side*.29f,1.1f,0),new Vector3(.24f,.31f,.44f),trim,weapon);
                    Crystal("Marshal crest",new Vector3(0,1.58f,0),new Vector3(.16f,.39f,.16f),light,weapon);
                    Part("Back capacitor",PrimitiveType.Cylinder,new Vector3(0,.96f,-.26f),new Vector3(.32f,.28f,.19f),accent,weapon);
                }
            }
        }
        GameObject Part(string name,PrimitiveType kind,Vector3 p,Vector3 scale,Material material,Transform parent=null)
        {
            var o=GameObject.CreatePrimitive(kind);o.name=name;o.transform.SetParent(parent==null?transform:parent,false);
            o.transform.localPosition=p;o.transform.localScale=scale;o.GetComponent<Renderer>().sharedMaterial=material;
            if(kind==PrimitiveType.Cube)o.GetComponent<MeshFilter>().sharedMesh=game.Models.BeveledBox;
            if(kind==PrimitiveType.Cylinder||kind==PrimitiveType.Sphere)o.GetComponent<MeshFilter>().sharedMesh=Faceted(kind);
            var collider=o.GetComponent<Collider>();collider.enabled=false;Destroy(collider);return o;
        }
        public Tower Subject {get;private set;}
        float observedHealth;
        CombatFeedback feedback;
        public void Initialize(Prototype game,Tower tower,TowerDesign design,int faction)
        {
            this.game=game;Subject=tower;observedHealth=tower.Health;feedback=game.GetComponent<CombatFeedback>();
            var shots=game.World.Shots;
            observedShot=shots.Count>0?shots[shots.Count-1].Serial:0;
            var spec=tower.Spec;bool robot=game.World.Config.Theme=="iron";
            var palette=game.TowerPalette(faction);shell=palette[0];accent=palette[1];light=palette[2];
            Role=spec.Damage<=0?"Wall":design!=null&&design.Requires.Length>0?"Champion":!spec.TargetsGround?"Interceptor":spec.SlowFraction>0?"Control":spec.ChainTargets>0?"Relay":spec.SplashRadius>0?"Artillery":"Sentry";
            FactionFooting(palette[3],palette[4],faction,robot);
            if(robot&&faction==0)PulseTower(palette[3],palette[4],tower.Design);
            else if(robot&&faction==1)BlastTower(palette[3],palette[4],tower.Design-7);
            else if(robot&&faction==2)PrismTower(palette[3],palette[4],tower.Design-14);
            else if(robot&&faction==3)HorizonTower(palette[3],palette[4],tower.Design-21);
            else if(robot&&faction==4)GravityTower(palette[3],palette[4],tower.Design-28);
            else if(robot&&faction==5)ScrapTower(palette[3],palette[4],tower.Design-35);
            else if(robot&&faction==6)OverdriveTower(palette[3],palette[4],tower.Design-42);
            else if(robot&&faction==7)TidalTower(palette[3],palette[4],tower.Design-49);
            else if(!robot&&faction==0)RimeTower(palette[3],palette[4]);
            else if(!robot&&faction==1)StoneTower(palette[3],palette[4],spec.TargetsAir);
            else if(!robot&&faction==2)EmberTower(palette[3],palette[4],design!=null&&design.Name=="Meteor Crucible");
            else if(!robot&&faction==3)VoltTower(palette[3],palette[4],design!=null&&design.Name=="Nova Marshal");
            else {
            if(!robot&&Role!="Wall")for(int side=-1;side<=1;side+=2) {
                Part("Stone buttress",PrimitiveType.Cube,new Vector3(side*.31f,.38f,-.05f),new Vector3(.16f,.43f,.48f),shell);
            }
            if(Role=="Wall") {
                Part("Fortified wall",PrimitiveType.Cube,new Vector3(0,.42f,0),new Vector3(.8f,.5f,.8f),shell);
                for(int x=-1;x<=1;x++)Part("Battlement",PrimitiveType.Cube,new Vector3(x*.27f,.75f,0),new Vector3(.19f,.22f,.7f),accent);
            } else {
                var pivot=new GameObject(Role+" weapon");pivot.transform.SetParent(transform,false);weapon=pivot.transform;
                Part(robot?"Armored torso":"Pedestal",robot?PrimitiveType.Cube:PrimitiveType.Cylinder,new Vector3(0,.6f,0),new Vector3(.5f,.38f,.48f),shell,weapon);
                if(Role=="Artillery") {
                    var barrel=Part("Mortar barrel",PrimitiveType.Cylinder,new Vector3(0,.95f,.14f),new Vector3(.46f,.37f,.46f),accent,weapon);barrel.transform.localRotation=Quaternion.Euler(30,0,0);
                    Part("Mortar aperture",PrimitiveType.Sphere,new Vector3(0,1.27f,.33f),new Vector3(.29f,.07f,.29f),light,weapon);
                } else if(Role=="Interceptor") {
                    for(int side=-1;side<=1;side+=2){var rail=Part("Sky rail",PrimitiveType.Cube,new Vector3(side*.23f,1.06f,.08f),new Vector3(.12f,.75f,.16f),accent,weapon);rail.transform.localRotation=Quaternion.Euler(25,0,0);}
                    Part("Targeting lens",PrimitiveType.Sphere,new Vector3(0,.95f,.29f),Vector3.one*.24f,light,weapon);
                } else if(Role=="Control") {
                    var crystal=Part("Control crystal",PrimitiveType.Cube,new Vector3(0,1.1f,0),Vector3.one*.43f,light,weapon);crystal.transform.localRotation=Quaternion.Euler(0,45,35);
                    for(int side=-1;side<=1;side+=2)Part("Crystal prong",PrimitiveType.Cylinder,new Vector3(side*.29f,.92f,0),new Vector3(.08f,.34f,.08f),accent,weapon);
                } else if(Role=="Relay") {
                    for(int side=-1;side<=1;side+=2){Part("Coil mast",PrimitiveType.Cylinder,new Vector3(side*.24f,.96f,0),new Vector3(.12f,.38f,.12f),accent,weapon);Part("Arc node",PrimitiveType.Sphere,new Vector3(side*.24f,1.38f,0),Vector3.one*.25f,light,weapon);}
                } else {
                    Part(robot?"Helmet":"Runestone crown",robot?PrimitiveType.Sphere:PrimitiveType.Cylinder,new Vector3(0,1.02f,0),robot?new Vector3(.48f,.43f,.42f):new Vector3(.53f,.14f,.53f),accent,weapon);
                    if(!robot)for(int side=-1;side<=1;side+=2)Part("Crown merlon",PrimitiveType.Cube,new Vector3(side*.2f,1.22f,0),new Vector3(.14f,.2f,.36f),shell,weapon);
                    Part("Visor",PrimitiveType.Cube,new Vector3(0,1.04f,.2f),new Vector3(.34f,.1f,.07f),light,weapon);
                    var arm=Part("Arm cannon",PrimitiveType.Cylinder,new Vector3(.28f,.78f,.2f),new Vector3(.22f,.31f,.22f),accent,weapon);arm.transform.localRotation=Quaternion.Euler(90,0,0);
                    if(Role=="Champion")for(int side=-1;side<=1;side+=2){Part("Champion shoulder",PrimitiveType.Cube,new Vector3(side*.3f,1.06f,0),Vector3.one*.3f,accent,weapon);Part("Champion crown",PrimitiveType.Cube,new Vector3(side*.16f,1.38f,0),new Vector3(.1f,.3f,.12f),light,weapon);}
                }
            }
            }
            FactionArchitecture(palette[3],palette[4],faction,robot);
            DressFoundation(palette[3],palette[4],faction,robot);
            FitSilhouette();
            string modelKey=game.World.Config.Theme+"/"+faction+"/"+tower.Design;
            CombineRigidParts(transform,modelKey+"/base");
            if(weapon!=null)CombineRigidParts(weapon,modelKey+"/weapon");
            if(kinetic!=null)CombineRigidParts(kinetic,modelKey+"/kinetic");
            for(int i=0;i<2;i++)tiers[i]=Part("Upgrade tier "+(i+2),PrimitiveType.Cube,new Vector3((i==0?-1:1)*.23f,.20f,-.30f),new Vector3(.10f,.13f,.09f),light);
            Sync(tower,false);
        }
        public void Sync(Tower tower,bool clearance)
        {
            if(tower.Health<observedHealth)feedback.TowerStruck(tower,false);
            observedHealth=tower.Health;
            transform.position=new Vector3(tower.Center.X,0,tower.Center.Y);
            transform.localScale=new Vector3(tower.Spec.Width,clearance?.08f:1,tower.Spec.Height);
            VisibleLevel=tower.Level;for(int i=0;i<2;i++)tiers[i].SetActive(tower.Level>=i+2);
            if(weapon!=null) {
                weapon.localScale=new Vector3(weaponWidth,1+.08f*(tower.Level-1),weaponWidth);
                var shots=game.World.Shots;
                // Only direct shots from this footprint drive its weapon. Chain origins are enemies.
                for(int i=shots.Count-1;i>=0&&shots[i].Serial>observedShot;i--) {
                    var shot=shots[i];
                    if(shot.Chained||V2.Distance(shot.From,tower.Center)>.001f)continue;
                    var direction=shot.To-shot.From;
                    if(direction.Length>.001f)weapon.localRotation=Quaternion.Euler(0,Mathf.Atan2(direction.X,direction.Y)*Mathf.Rad2Deg,0);
                    shotTick=game.World.Tick;break;
                }
                if(shots.Count>0)observedShot=shots[shots.Count-1].Serial;
                // Simulation ticks freeze in pause/setup and naturally follow the speed controls.
                float recoil=Mathf.Clamp01(1-(game.World.Tick-shotTick)/6f)*(Role=="Artillery"?.14f:.09f);
                weapon.localPosition=-(weapon.localRotation*Vector3.forward)*recoil;
            }
            if(kinetic!=null)kinetic.localRotation=kineticRest*Quaternion.AngleAxis(game.World.Tick*World.FixedDelta*kineticSpeed,Vector3.up);
        }
    }
}
