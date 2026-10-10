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
        float weaponWidth=1,weaponHeight=1;
        Prototype game;
        long observedShot,shotTick=-100;

        GameObject[] tiers=new GameObject[2];
        Material shell,accent,light;
        Mesh Faceted(PrimitiveType kind) => kind==PrimitiveType.Cylinder?game.Models.Column:game.Models.Shell;
        GameObject Crystal(string name,Vector3 position,Vector3 scale,Material material,Transform parent=null) {
            var part=Part(name,PrimitiveType.Sphere,position,scale,material,parent);part.GetComponent<MeshFilter>().sharedMesh=game.Models.Crystal;return part;
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
        public void Initialize(Prototype game,Tower tower,TowerDesign design,int faction,bool presentationOnly=false)
        {
            this.game=game;Subject=tower;observedHealth=tower.Health;feedback=game.GetComponent<CombatFeedback>();
            var shots=game.World.Shots;
            observedShot=shots.Count>0?shots[shots.Count-1].Serial:0;
            var spec=tower.Spec;bool robot=game.World.Config.Theme=="iron";
            var palette=game.TowerPalette(faction);shell=palette[0];accent=palette[1];light=palette[2];
            Role=spec.Damage<=0?"Wall":design!=null&&design.Requires.Length>0?"Champion":!spec.TargetsGround?"Interceptor":spec.SlowFraction>0?"Control":spec.ChainTargets>0?"Relay":spec.SplashRadius>0?"Artillery":"Sentry";
            FactionFooting(palette[3],palette[4],faction,robot);
            if(robot&&faction==0)PulseTower(palette[3],palette[4],tower.Design);
            else if(!robot&&faction==1)RootboundTower(palette[3],palette[4],tower.Design-5);
            else CreatureTower(palette[3],palette[4],faction,robot);
            DressFoundation(palette[3],palette[4],faction,robot);
            weaponHeight=TowerReadability.Height(game.World.Config,tower.Design);
            // Scale the whole authored anatomy, including static supports, around the ground.
            foreach(Transform part in transform){
                if(part==weapon)continue;
                var position=part.localPosition;position.y*=weaponHeight;part.localPosition=position;
                var size=part.localScale;size.y*=weaponHeight;part.localScale=size;
            }
            if(weapon!=null)weapon.localScale=new Vector3(1,weaponHeight,1);
            FitSilhouette();
            string modelKey=game.World.Config.Theme+"/"+faction+"/"+tower.Design;
            CombineRigidParts(transform,modelKey+"/base");
            if(weapon!=null)CombineRigidParts(weapon,modelKey+"/weapon");
            if(kinetic!=null)CombineRigidParts(kinetic,modelKey+"/kinetic");
            if(livingCrown!=null)CombineRigidParts(livingCrown,modelKey+"/crown");
            if(livingArm!=null)CombineRigidParts(livingArm,modelKey+"/arm");
            if(creatureHead!=null)CombineRigidParts(creatureHead,modelKey+"/head");
            if(creatureGesture!=null)CombineRigidParts(creatureGesture,modelKey+"/gesture");
            for(int i=0;i<2;i++)tiers[i]=Part("Upgrade tier "+(i+2),PrimitiveType.Cube,new Vector3((i==0?-1:1)*.23f,.20f,-.30f),new Vector3(.10f,.13f,.09f),light);
            if(presentationOnly)Pose(tower,false);else Sync(tower,false);
        }
        void Pose(Tower tower,bool clearance)
        {
            transform.position=new Vector3(tower.Center.X,0,tower.Center.Y);
            transform.localScale=new Vector3(tower.Spec.Width,clearance?.08f:1,tower.Spec.Height);
            VisibleLevel=tower.Level;for(int i=0;i<2;i++)tiers[i].SetActive(tower.Level>=i+2);
            if(weapon!=null)weapon.localScale=new Vector3(weaponWidth,weaponHeight*(1+.08f*(tower.Level-1)),weaponWidth);
        }
        public void Sync(Tower tower,bool clearance)
        {
            if(tower.Health<observedHealth)feedback.TowerStruck(tower,false);
            observedHealth=tower.Health;Pose(tower,clearance);
            if(weapon!=null) {
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
                weapon.localPosition=livingCrown!=null||creature?Vector3.zero:-(weapon.localRotation*Vector3.forward)*recoil;
                AnimateRootbound();AnimateCreature();
            }
            if(kinetic!=null){
                float phase=game.World.Tick*World.FixedDelta*kineticSpeed;
                kinetic.localRotation=kineticRest*(kineticSwing?Quaternion.AngleAxis(Mathf.Sin(phase*Mathf.Deg2Rad)*5,Vector3.forward):Quaternion.AngleAxis(phase,Vector3.up));
            }
        }
    }
}
