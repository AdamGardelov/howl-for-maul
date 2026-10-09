using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace FrostMaze
{
    public sealed partial class MapScenery
    {
        readonly List<Vector3> fireAnchors=new List<Vector3>();
        sealed class FlameMesh { public Mesh Mesh;public Vector3[] Rest,Moved;public int[] Anchor; }
        readonly List<FlameMesh> flames=new List<FlameMesh>();
        readonly List<Light> fireLights=new List<Light>();
        readonly List<Transform> fireHalos=new List<Transform>();
        Texture2D fireTexture;
        Mesh canopy;Vector3[] canopyRest,canopyMoved;
        void RememberCanopy(Mesh mesh){canopy=mesh;canopyRest=mesh.vertices;canopyMoved=new Vector3[canopyRest.Length];mesh.MarkDynamic();}
        float fireTime,lightRefresh;
        readonly int[] nearestFires=new int[4];
        readonly float[] nearestDistances=new float[4];
        public int ActiveFireLights { get; private set; }
        void RememberFlame(Mesh mesh)
        {
            var rest=mesh.vertices;var owners=new int[rest.Length];
            for(int i=0;i<rest.Length;i++) {
                float best=float.MaxValue;
                for(int j=0;j<fireAnchors.Count;j++) {
                    var d=rest[i]-fireAnchors[j];float distance=d.x*d.x+d.z*d.z;
                    if(distance<best){best=distance;owners[i]=j;}
                }
            }
            mesh.MarkDynamic();flames.Add(new FlameMesh{Mesh=mesh,Rest=rest,Moved=new Vector3[rest.Length],Anchor=owners});
        }
        void BuildLivingFire(Prototype game,bool ice)
        {
            fireTexture=new Texture2D(32,32,TextureFormat.RGBA32,false){name="Original soft fire glow",wrapMode=TextureWrapMode.Clamp};
            var pixels=new Color[1024];
            for(int y=0;y<32;y++)for(int x=0;x<32;x++) {
                float radius=new Vector2((x-15.5f)/15.5f,(y-15.5f)/15.5f).magnitude;
                pixels[y*32+x]=new Color(1,1,1,Mathf.Pow(Mathf.Clamp01(1-radius),2));
            }
            fireTexture.SetPixels(pixels);fireTexture.Apply();
            var material=game.MakeMaterial(Color.white);
            material.shader=Resources.Load<Material>("HearthSmoke").shader;
            material.SetTexture("_BaseMap",fireTexture);material.SetColor("_BaseColor",Color.white);
            material.SetFloat("_Surface",1);material.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend",(float)BlendMode.One);material.SetFloat("_ZWrite",0);
            material.SetFloat("_Cull",0);material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");material.renderQueue=3000;
            var color=ice?new Color(1,.55f,.20f):new Color(1,.34f,.045f);
            var haloMesh=new Mesh{name="Fire halo quad"};
            haloMesh.vertices=new[]{new Vector3(-.5f,-.5f,0),new Vector3(.5f,-.5f,0),new Vector3(.5f,.5f,0),new Vector3(-.5f,.5f,0)};
            haloMesh.uv=new[]{Vector2.zero,Vector2.right,Vector2.one,Vector2.up};
            haloMesh.colors=new[]{color,color,color,color};haloMesh.triangles=new[]{0,1,2,0,2,3};haloMesh.RecalculateBounds();meshes.Add(haloMesh);
            for(int i=0;i<fireAnchors.Count;i++) {
                // Effects use the world layer, excluded from the layer-30 tactical map capture.
                var root=new GameObject("Living brazier "+i);root.transform.SetParent(transform,false);root.transform.localPosition=fireAnchors[i];
                var halo=new GameObject("Fire halo");halo.transform.SetParent(root.transform,false);halo.transform.localPosition=Vector3.up*.3f;
                halo.AddComponent<MeshFilter>().sharedMesh=haloMesh;
                var haloRenderer=halo.AddComponent<MeshRenderer>();haloRenderer.sharedMaterial=material;haloRenderer.shadowCastingMode=ShadowCastingMode.Off;haloRenderer.receiveShadows=false;
                fireHalos.Add(halo.transform);
                var lamp=root.AddComponent<Light>();lamp.type=LightType.Point;lamp.color=color;lamp.range=3.5f;lamp.intensity=1.15f;lamp.shadows=LightShadows.None;lamp.enabled=false;fireLights.Add(lamp);
                // Small, bounded particle budget. Seeds are presentation-only and reproducible.
                var embers=new GameObject("Rising embers");embers.transform.SetParent(root.transform,false);embers.transform.localPosition=Vector3.up*.25f;
                var particles=embers.AddComponent<ParticleSystem>();particles.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
                particles.useAutoRandomSeed=false;particles.randomSeed=(uint)(103+i*71);
                var main=particles.main;main.startLifetime=new ParticleSystem.MinMaxCurve(.65f,1.3f);main.startSpeed=0;main.startSize=new ParticleSystem.MinMaxCurve(.045f,.09f);main.maxParticles=10;main.useUnscaledTime=true;main.cullingMode=ParticleSystemCullingMode.AlwaysSimulate;main.simulationSpace=ParticleSystemSimulationSpace.Local;main.startColor=Color.Lerp(color,Color.white,.45f);
                var shape=particles.shape;shape.shapeType=ParticleSystemShapeType.Sphere;shape.radius=.13f;
                var emission=particles.emission;emission.rateOverTime=5;
                var velocity=particles.velocityOverLifetime;velocity.enabled=true;velocity.space=ParticleSystemSimulationSpace.Local;
                velocity.x=new ParticleSystem.MinMaxCurve(-.15f,.15f);velocity.y=new ParticleSystem.MinMaxCurve(.65f,1.1f);velocity.z=new ParticleSystem.MinMaxCurve(-.12f,.12f);
                var tint=particles.colorOverLifetime;tint.enabled=true;var fade=new Gradient();fade.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(color,1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(1,.15f),new GradientAlphaKey(0,1)});tint.color=fade;
                var renderer=particles.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
                particles.Play();
            }
        }
        void Update()
        {
            if(sceneryGame==null||fireAnchors.Count==0)return;
            // Ambient fire stays alive during pause and uses real time at every combat speed.
            fireTime+=Time.unscaledDeltaTime;
            if(canopy!=null) {
                for(int i=0;i<canopyRest.Length;i++) {
                    var p=canopyRest[i];float flex=Mathf.Clamp01((p.y-1.4f)/2.3f);
                    p.x+=flex*.025f*Mathf.Sin(fireTime*1.25f+p.x*.45f+p.z*.31f);
                    p.z+=flex*.018f*Mathf.Sin(fireTime*.9f+p.z*.5f);canopyMoved[i]=p;
                }
                canopy.vertices=canopyMoved;canopy.RecalculateBounds();
            }
            foreach(var flame in flames) {
                for(int i=0;i<flame.Rest.Length;i++) {
                    var p=flame.Rest[i];var origin=fireAnchors[flame.Anchor[i]];float h=Mathf.Max(0,p.y-origin.y),phase=flame.Anchor[i]*2.37f;
                    p.y=origin.y+h*(1+.13f*Mathf.Sin(fireTime*7.3f+phase)+.06f*Mathf.Sin(fireTime*13.1f+phase));
                    p.x+=h*.12f*Mathf.Sin(fireTime*3.7f+phase);p.z+=h*.08f*Mathf.Cos(fireTime*5.1f+phase);
                    flame.Moved[i]=p;
                }
                flame.Mesh.vertices=flame.Moved;flame.Mesh.RecalculateBounds();
            }
            var camera=sceneryGame.View;if(camera==null)return;
            lightRefresh-=Time.unscaledDeltaTime;
            if(lightRefresh<=0) {
                lightRefresh=.2f;
                for(int i=0;i<4;i++){nearestFires[i]=-1;nearestDistances[i]=float.MaxValue;}
                for(int i=0;i<fireLights.Count;i++) {
                    var position=fireHalos[i].position;var viewport=camera.WorldToViewportPoint(position);
                    if(viewport.z<=0||viewport.x<-.1f||viewport.x>1.1f||viewport.y<-.1f||viewport.y>1.1f)continue;
                    float distance=(position-camera.transform.position).sqrMagnitude;
                    for(int slot=0;slot<4;slot++)if(distance<nearestDistances[slot]) {
                        for(int j=3;j>slot;j--){nearestDistances[j]=nearestDistances[j-1];nearestFires[j]=nearestFires[j-1];}
                        nearestDistances[slot]=distance;nearestFires[slot]=i;break;
                    }
                }
                ActiveFireLights=0;
                for(int i=0;i<fireLights.Count;i++){bool active=false;for(int j=0;j<4;j++)active|=nearestFires[j]==i;fireLights[i].enabled=active;if(active)ActiveFireLights++;}
            }
            for(int i=0;i<fireHalos.Count;i++) {
                float pulse=1+.1f*Mathf.Sin(fireTime*7.3f+i*2.37f)+.04f*Mathf.Sin(fireTime*13.1f+i);
                fireHalos[i].rotation=camera.transform.rotation;fireHalos[i].localScale=Vector3.one*(.90f*pulse);
                fireLights[i].intensity=1.15f*pulse;
            }
        }
    }
}
