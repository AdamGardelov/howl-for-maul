using System.Collections.Generic;
using UnityEngine;
namespace FrostMaze
{
    // One renderer per event: solid head, short tapered wake and impact fragments share a mesh.
    // Damage remains authoritative and immediate; only this visual travels.
    public sealed class ProjectileCue : MonoBehaviour
    {
        public int Design {get;private set;}
        public Vector3 From {get;private set;}
        public Vector3 To {get;private set;}
        public ProjectileStyle Style {get;private set;}
        public float Progress {get;private set;}
        public bool Chained {get;private set;}
        LineRenderer line;Mesh mesh;Vector3 direction,side,up;
        readonly List<Vector3> vertices=new List<Vector3>(512);
        readonly List<int> triangles=new List<int>(768);
        public void Initialize(int design,ProjectileStyle style,Vector3 from,Vector3 to,bool chained,Material material)
        {
            Design=design;Style=style;From=from;To=to;Chained=chained;
            direction=(to-from).sqrMagnitude>.0001f?(to-from).normalized:Vector3.forward;
            side=Vector3.Cross(direction,Vector3.up);if(side.sqrMagnitude<.001f)side=Vector3.right;side.Normalize();up=Vector3.Cross(side,direction).normalized;
            Renderer renderer;
            if(chained) {
                line=gameObject.AddComponent<LineRenderer>();line.useWorldSpace=true;line.numCapVertices=2;line.numCornerVertices=2;
                line.startWidth=.045f;line.endWidth=.025f;line.positionCount=9;renderer=line;
            } else {
                mesh=new Mesh{name="Solid projectile "+design};mesh.MarkDynamic();gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
                renderer=gameObject.AddComponent<MeshRenderer>();
            }
            renderer.sharedMaterial=material;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;
            Render(0);
        }
        void Triangle(Vector3 a,Vector3 b,Vector3 c){int n=vertices.Count;vertices.Add(a);vertices.Add(b);vertices.Add(c);triangles.Add(n);triangles.Add(n+1);triangles.Add(n+2);}
        void Body(Vector3 center,float radius,float length,int points,bool star=false) {
            for(int i=0;i<points;i++) {
                float a=i*Mathf.PI*2/points,b=(i+1)*Mathf.PI*2/points;
                float ra=star&&i%2==1?radius*.46f:radius,rb=star&&(i+1)%2==1?radius*.46f:radius;
                var v=center+(side*Mathf.Cos(a)+up*Mathf.Sin(a))*ra;var w=center+(side*Mathf.Cos(b)+up*Mathf.Sin(b))*rb;
                Triangle(center+direction*length,w,v);Triangle(center-direction*length,v,w);
            }
        }
        public void Render(float progress)
        {
            Progress=Mathf.Clamp01(progress);
            if(Chained){for(int i=0;i<9;i++){float t=i/8f;line.SetPosition(i,Vector3.Lerp(From,To,t)+side*(i==0||i==8?0:(i%2==0?-1:1)*.12f*(1-Progress*.5f)));}return;}
            vertices.Clear();triangles.Clear();
            float travel=Mathf.Lerp(.08f,1,Mathf.Clamp01(Progress/.82f));var head=Vector3.Lerp(From,To,travel)+Vector3.up*(Mathf.Sin(travel*Mathf.PI)*Style.Arc);
            float impact=Mathf.Clamp01((Progress-.82f)/.18f),size=Style.Size*(1-impact*.6f);
            if(impact==0) {
                var tail=Vector3.Lerp(From,To,Mathf.Max(0,travel-.16f))+Vector3.up*(Mathf.Sin(Mathf.Max(0,travel-.16f)*Mathf.PI)*Style.Arc);
                Triangle(tail,head+side*.035f,head-side*.035f);Triangle(tail,head-side*.035f,head+side*.035f);
                Triangle(tail,head+up*.035f,head-up*.035f);Triangle(tail,head-up*.035f,head+up*.035f);
            }
            if(Style.Shape==ProjectileShape.Seed||Style.Shape==ProjectileShape.ThornPod||Style.Shape==ProjectileShape.Wingseed||Style.Shape==ProjectileShape.Sprout){
                Body(head,size*.58f,size*.86f,8);
                if(Style.Shape==ProjectileShape.Seed){
                    Body(head-direction*size*.55f,size*.70f,size*.22f,8);
                }else if(Style.Shape==ProjectileShape.ThornPod){
                    for(int i=0;i<7;i++){float a=i*Mathf.PI*2/7;var outwards=side*Mathf.Cos(a)+up*Mathf.Sin(a);
                        Body(head+outwards*size*.70f,size*.18f,size*.40f,4);
                    }
                }else{
                    int leaves=Style.Shape==ProjectileShape.Wingseed?2:4;
                    Body(head-direction*size,size*.10f,size*1.2f,5);
                    for(int i=0;i<leaves;i++){
                        float sign=i%2==0?-1:1;var root=head-direction*size*(i/2)*.9f;
                        var tip=root+side*sign*size*(leaves==2?1.75f:1.2f)-direction*size*.5f;
                        var belly=(root+tip)*.5f+up*size*.22f;
                        Triangle(root,tip,belly+direction*size*.4f);Triangle(root,belly+direction*size*.4f,tip);
                        Triangle(root,belly-direction*size*.4f,tip);Triangle(root,tip,belly-direction*size*.4f);
                    }
                }
            } else if(Style.Shape==ProjectileShape.Ring) {
                for(int i=0;i<12;i++) {
                    float a=i*Mathf.PI/6,b=(i+1)*Mathf.PI/6;var u=side*Mathf.Cos(a)+up*Mathf.Sin(a);var v=side*Mathf.Cos(b)+up*Mathf.Sin(b);
                    Vector3 outerA=head+u*size,outerB=head+v*size,innerA=head+u*size*.6f,innerB=head+v*size*.6f;
                    Triangle(outerA,outerB,innerB);Triangle(outerA,innerB,innerA);Triangle(outerB,outerA,innerB);Triangle(innerB,outerA,innerA);
                }
            } else {
                bool spear=Style.Shape==ProjectileShape.Spear||Style.Shape==ProjectileShape.Shard;
                Body(head,size*(spear?.42f:Style.Shape==ProjectileShape.Bolt?.55f:.82f),size*(spear?1.7f:1),Style.Shape==ProjectileShape.Star?10:Style.Shape==ProjectileShape.Orb||Style.Shape==ProjectileShape.Shell?8:5,Style.Shape==ProjectileShape.Star);
                if(Style.Shape==ProjectileShape.Ember)Body(head-direction*size*1.2f,size*.4f,size*.8f,5);
            }
            if(impact>0)for(int i=0;i<5;i++) {
                float a=i*Mathf.PI*2/5+Design;var dir=side*Mathf.Cos(a)+up*Mathf.Sin(a);
                Body(To+dir*impact*Style.Size*3.3f,size*.19f,size*.38f,4);
            }
            for(int i=0;i<vertices.Count;i++)vertices[i]=transform.InverseTransformPoint(vertices[i]);
            mesh.Clear();mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
        }
        void OnDestroy(){if(mesh!=null)Destroy(mesh);}
    }
}
