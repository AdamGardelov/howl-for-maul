using FrostMaze.Simulation;
using UnityEngine;
namespace FrostMaze
{
    // Cosmetic model and poses only; the simulation remains the owner of every collision disc.
    public sealed class EnemyView : MonoBehaviour
    {
        public Enemy Subject {get;private set;}
        public HowlForm Form {get;private set;}
        public Transform Body {get;private set;}
        public Transform LeftMotion {get;private set;}
        public Transform RightMotion {get;private set;}
        public Renderer Signal {get;private set;}
        GameObject slowHalo;
        Material normal,hit,siege;
        float observedHealth,scale;long hitUntil=-1,visualTick=-1;
        bool flying;
        public void Initialize(Enemy enemy,HowlModels library,Material[] palette,Material frost,Material hit,Material siege,ModelMeshes meshes)
        {
            Subject=enemy;Form=EnemyIdentity.For(enemy.Spec);flying=enemy.Spec.Flying;
            observedHealth=enemy.Spec.Health;normal=palette[3];this.hit=hit;this.siege=siege;
            var model=library.Get(Form);
            Body=Node(EnemyIdentity.Name(Form),transform);LeftMotion=Node(flying?"Left wing":"Left gait",Body);RightMotion=Node(flying?"Right wing":"Right gait",Body);
            scale=flying?enemy.Spec.Radius*(Form==HowlForm.StormHerald?1.65f:1.45f):enemy.Spec.Radius*.96f/model.GroundExtent;
            // Height communicates the role; horizontal size still fits the simulation disc.
            float height=Form==HowlForm.Gatebreaker?.78f:Form==HowlForm.HollowWarden?.72f:Form==HowlForm.Cairnback?.37f:Form==HowlForm.Thornrunner?.34f:Form==HowlForm.Ashling?.24f:.28f;
            Body.localScale=new Vector3(scale,flying?scale:height*(enemy.Spec.Radius/.2f)/model.Height,scale);
            foreach(var part in model.Parts) {
                var parent=part.Joint==0?Body:part.Joint==1?LeftMotion:RightMotion;
                var node=Node("Howl batch "+part.Joint+" / "+part.Material,parent);
                node.gameObject.AddComponent<MeshFilter>().sharedMesh=part.Mesh;
                var renderer=node.gameObject.AddComponent<MeshRenderer>();renderer.sharedMaterial=palette[part.Material];
                if(part.Material==3)Signal=renderer;
            }
            slowHalo=Node("Frost status",transform).gameObject;
            slowHalo.AddComponent<MeshFilter>().sharedMesh=meshes.OwnerRing;var halo=slowHalo.AddComponent<MeshRenderer>();halo.sharedMaterial=frost;halo.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
            slowHalo.transform.localScale=Vector3.one*enemy.Spec.Radius*2.4f;slowHalo.transform.localPosition=new Vector3(0,flying?-.3f:.025f,0);slowHalo.SetActive(false);
        }
        static Transform Node(string name,Transform parent){var t=new GameObject(name).transform;t.SetParent(parent,false);return t;}
        public void Sync(Enemy enemy,long tick)
        {
            transform.position=new Vector3(enemy.Position.X,flying?1.7f:0,enemy.Position.Y);
            // Repeated render frames at one simulation tick keep the exact same pose and allocate nothing.
            if(visualTick==tick)return;
            float strike=!flying?Mathf.Clamp01(1-(tick-enemy.LastAttackTick)/6f):0;
            var direction=strike>0?enemy.AttackDirection:enemy.Velocity.Length>.03f?enemy.Velocity:enemy.IntendedDirection;
            if(direction.Length>.03f){var facing=Quaternion.LookRotation(new Vector3(direction.X,0,direction.Y));transform.rotation=visualTick<0||tick<visualTick?facing:Quaternion.RotateTowards(transform.rotation,facing,Mathf.Clamp(tick-visualTick,1,6)*World.FixedDelta*660);}
            visualTick=tick;
            if(enemy.Health<observedHealth)hitUntil=tick+4;
            observedHealth=enemy.Health;
            float hurt=Mathf.Clamp01((hitUntil-tick)/4f);
            float time=tick*World.FixedDelta,phase=time*(Form==HowlForm.Ashling?18:Form==HowlForm.Thornrunner?13:7)+enemy.Id*.73f;
            float moving=Mathf.Min(1,enemy.Velocity.Length);
            Body.localPosition=new Vector3(0,flying?Mathf.Sin(phase*.6f)*.06f:.025f+Mathf.Abs(Mathf.Sin(phase))*.012f*moving,0);
            Body.localRotation=Quaternion.Euler(0,strike*18-hurt*12,0);
            float gait=flying?Mathf.Sin(phase)*(Form==HowlForm.Gloamwing?28:18):Mathf.Sin(phase)*moving*(Form==HowlForm.Thornrunner?24:Form==HowlForm.Ashling?13:17);
            LeftMotion.localRotation=Quaternion.Euler(flying?0:gait,0,flying?gait:0);
            RightMotion.localRotation=Quaternion.Euler(flying?0:-gait,0,flying?-gait:0);
            Signal.sharedMaterial=tick<hitUntil?hit:enemy.Blocked?siege:normal;
            slowHalo.SetActive(enemy.SlowRemaining>0);
        }
    }
}
