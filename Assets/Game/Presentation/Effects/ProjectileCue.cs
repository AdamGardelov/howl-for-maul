using UnityEngine;
namespace FrostMaze
{
    // A single bounded renderer per shot. No physics, colliders, particles or damage timing.
    public sealed class ProjectileCue : MonoBehaviour
    {
        public int Design {get;private set;}
        public Vector3 From {get;private set;}
        public Vector3 To {get;private set;}
        public ProjectileStyle Style {get;private set;}
        public float Progress {get;private set;}
        public bool Chained {get;private set;}
        LineRenderer line;Vector3 direction,side,up;
        public void Initialize(int design,ProjectileStyle style,Vector3 from,Vector3 to,bool chained,Material material)
        {
            Design=design;Style=style;From=from;To=to;Chained=chained;
            direction=(to-from).sqrMagnitude>.0001f?(to-from).normalized:Vector3.forward;
            side=Vector3.Cross(direction,Vector3.up);if(side.sqrMagnitude<.001f)side=Vector3.right;side.Normalize();up=Vector3.Cross(side,direction).normalized;
            line=gameObject.AddComponent<LineRenderer>();line.sharedMaterial=material;line.useWorldSpace=true;
            line.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;line.receiveShadows=false;
            line.numCapVertices=2;line.numCornerVertices=2;line.startWidth=chained?.045f:.042f;line.endWidth=chained?.025f:.042f;
            Render(0);
        }
        public void Render(float progress)
        {
            Progress=Mathf.Clamp01(progress);
            if(Chained) {
                line.positionCount=9;
                for(int i=0;i<9;i++){float t=i/8f;line.SetPosition(i,Vector3.Lerp(From,To,t)+side*(i==0||i==8?0:(i%2==0?-1:1)*.12f*(1-Progress*.5f)));}
                return;
            }
            // A short traveling tracer follows the already-resolved shot event. It never delays damage.
            float travel=Mathf.Lerp(.08f,1,Mathf.Clamp01(Progress/.82f));var head=Vector3.Lerp(From,To,travel)+Vector3.up*(Mathf.Sin(travel*Mathf.PI)*Style.Arc);
            float impact=Mathf.Clamp01((Progress-.82f)/.18f);
            line.startWidth=line.endWidth=.042f*(1-impact*.8f);
            float size=Style.Size*(1+impact*.9f);int points=Style.Shape==ProjectileShape.Ring?16:Style.Shape==ProjectileShape.Star?10:Style.Shape==ProjectileShape.Orb||Style.Shape==ProjectileShape.Shell?8:4;
            line.positionCount=points+4;
            line.SetPosition(0,Vector3.Lerp(From,To,Mathf.Max(0,travel-.23f))+Vector3.up*(Mathf.Sin(Mathf.Max(0,travel-.23f)*Mathf.PI)*Style.Arc));
            line.SetPosition(1,head-direction*size);
            for(int i=0;i<=points;i++) {
                float a=i*Mathf.PI*2/points;
                Vector3 offset;
                if(Style.Shape==ProjectileShape.Ring)offset=(side*Mathf.Cos(a)+up*Mathf.Sin(a))*size;
                else {
                    float radius=Style.Shape==ProjectileShape.Star&&(i%2)==1?.42f:1;
                    float length=Style.Shape==ProjectileShape.Spear?2:Style.Shape==ProjectileShape.Shard?1.45f:Style.Shape==ProjectileShape.Bolt?1.1f:1;
                    float width=Style.Shape==ProjectileShape.Spear?.30f:Style.Shape==ProjectileShape.Bolt?.50f:.75f;
                    offset=(direction*Mathf.Cos(a)*length+side*Mathf.Sin(a)*width)*size*radius;
                    if(Style.Shape==ProjectileShape.Ember)offset-=direction*(1-Mathf.Cos(a))*size*.55f;
                    if(Style.Shape==ProjectileShape.Shell)offset+=up*Mathf.Sin(a*2)*size*.32f;
                }
                line.SetPosition(i+2,head+offset);
            }
            line.SetPosition(points+3,head);
        }
    }
}
