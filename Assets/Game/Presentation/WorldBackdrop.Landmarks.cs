using UnityEngine;
namespace FrostMaze
{
    public sealed partial class WorldBackdrop
    {
        Mesh pennants;
        Texture2D smokeTexture;
        Material smokeMaterial;
        Vector3[] flagRest,flagMoved;
        float breeze;
        Prototype landscapeGame;
        public int Buildings { get; private set; }
        // A sheltered settlement / old forge district outside the simulation rectangle.
        // Broad roof silhouettes and warm windows give the defended exit a human context.
        void BuildLandmarks(Prototype game,bool ice,float width,float height)
        {
            landscapeGame=game;
            smokeTexture=new Texture2D(32,32,TextureFormat.RGBA32,false){name="Original soft chimney plume"};
            var pixels=new Color[1024];for(int y=0;y<32;y++)for(int x=0;x<32;x++) {
                float radius=new Vector2((x-15.5f)/15.5f,(y-15.5f)/15.5f).magnitude;
                pixels[y*32+x]=new Color(1,1,1,Mathf.Pow(Mathf.Clamp01(1-radius),2));
            }
            smokeTexture.SetPixels(pixels);smokeTexture.Apply();
            smokeMaterial=game.MakeMaterial(Color.white);
            smokeMaterial.shader=Resources.Load<Material>("HearthSmoke").shader;
            smokeMaterial.SetTexture("_BaseMap",smokeTexture);smokeMaterial.SetFloat("_Surface",1);
            smokeMaterial.SetFloat("_SrcBlend",(float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            smokeMaterial.SetFloat("_DstBlend",(float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            smokeMaterial.SetFloat("_ZWrite",0);smokeMaterial.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");smokeMaterial.renderQueue=3000;
            var paths=new Batch();
            float exit=game.World.Config.Lanes[0].GroundRoute[game.World.Config.Lanes[0].GroundRoute.Length-1].X;
            float TerrainY(Vector3 p) {
                float Raw(float x,float z){float distance=Mathf.Max(Mathf.Max(-x,x-width),Mathf.Max(-z,z-height));return -.16f+Mathf.SmoothStep(0,1,Mathf.Clamp01((distance-5)/35))*Mathf.PerlinNoise((x+321)*.021f,(z+157)*.021f)*5;}
                // Match the actual eight-unit terrain triangles, not the unsampled height function.
                float x0=Mathf.Floor(p.x/8)*8,z0=Mathf.Floor(p.z/8)*8,u=(p.x-x0)/8,v=(p.z-z0)/8;
                float a=Raw(x0,z0),b=Raw(x0,z0+8),c=Raw(x0+8,z0+8),d=Raw(x0+8,z0);
                return (v>=u?a+(c-b)*u+(b-a)*v:a+(d-a)*u+(c-d)*v)+.05f;
            }
            Vector3 Ground(Vector3 p){p.y=TerrainY(p);return p;}
            void Trail(Vector3 a,Vector3 b,float wide) {var d=(b-a).normalized;var side=new Vector3(-d.z,0,d.x)*wide;
                int steps=Mathf.CeilToInt(Vector3.Distance(a,b)/.5f);for(int i=0;i<steps;i++){var p=Vector3.Lerp(a,b,i/(float)steps);var q=Vector3.Lerp(a,b,(i+1)/(float)steps);paths.Quad(Ground(p-side),Ground(p+side),Ground(q+side),Ground(q-side));}}
            Trail(new Vector3(exit,-.11f,-1.1f),new Vector3(exit,-.11f,-25),1);
            Trail(new Vector3(exit,-.105f,-16),new Vector3(14,-.105f,-11),.8f);
            Trail(new Vector3(exit,-.105f,-16),new Vector3(width-10,-.105f,-10),.8f);
            Save("Settlement paths",paths,game.MakeMaterial(ice?new Color(.30f,.37f,.35f):new Color(.26f,.25f,.21f)));
            var masonry=new Batch();var timber=new Batch();var roofs=new Batch();var caps=new Batch();var light=new Batch();var cloth=new Batch();
            void Roof(float x,float z,float w,float d,float eave,float ridge) {
                roofs.Quad(new Vector3(x,eave,z),new Vector3(x,eave,z+d),new Vector3(x+w*.5f,ridge,z+d),new Vector3(x+w*.5f,ridge,z));
                roofs.Quad(new Vector3(x+w*.5f,ridge,z),new Vector3(x+w*.5f,ridge,z+d),new Vector3(x+w,eave,z+d),new Vector3(x+w,eave,z));
                timber.Triangle(new Vector3(x,eave,z),new Vector3(x+w*.5f,ridge,z),new Vector3(x+w,eave,z));
                timber.Triangle(new Vector3(x+w,eave,z+d),new Vector3(x+w*.5f,ridge,z+d),new Vector3(x,eave,z+d));
                if(ice) {
                    float inset=.22f,low=eave+(ridge-eave)*inset/(w*.5f)+.035f;
                    caps.Quad(new Vector3(x+inset,low,z+.18f),new Vector3(x+inset,low,z+d-.23f),new Vector3(x+w*.5f,ridge+.04f,z+d-.08f),new Vector3(x+w*.5f,ridge+.04f,z+.1f));
                    caps.Quad(new Vector3(x+w*.5f,ridge+.04f,z+.1f),new Vector3(x+w*.5f,ridge+.04f,z+d-.08f),new Vector3(x+w-inset,low,z+d-.17f),new Vector3(x+w-inset,low,z+.27f));
                }
                // Uneven snow/copper ridge caps, restrained rather than a white roof blanket.
                caps.Quad(new Vector3(x+w*.5f-.13f,ridge+.025f,z-.04f),new Vector3(x+w*.5f-.13f,ridge+.025f,z+d+.04f),new Vector3(x+w*.5f+.13f,ridge+.025f,z+d+.04f),new Vector3(x+w*.5f+.13f,ridge+.025f,z-.04f));
            }
            void House(float x,float z,float w,float d,float tall) {
                masonry.Box(x-.2f,z-.2f,w+.4f,d+.4f,-.12f,.28f);
                timber.Box(x,z,w,d,.28f,tall);
                for(int side=0;side<2;side++)for(int i=0;i<3;i++) {
                    float xx=x+.5f+i*(w-1.2f)/2,zz=side==0?z-.025f:z+d+.025f;
                    light.Box(xx,zz,.43f,.035f,.95f,1.55f);
                    masonry.Box(xx-.04f,zz-.035f,.51f,.1f,.88f,.97f);
                    timber.Box(xx+.2f,zz-.05f,.035f,.13f,.95f,1.55f);
                }
                for(int i=0;i<3;i++)timber.Box(x-.025f,z+i*d/2,.07f,.1f,.3f,tall+.03f);
                Roof(x-.25f,z-.3f,w+.5f,d+.6f,tall,tall+(ice?1.8f:1.1f));
                float chimneyX=x+w*.68f,chimneyZ=z+d*.65f;
                masonry.Box(chimneyX,chimneyZ,.45f,.48f,tall,tall+2.15f);
                caps.Box(chimneyX-.08f,chimneyZ-.06f,.61f,.6f,tall+2.15f,tall+2.27f);
                // Stacked firewood / pipe bundles beside each building, still outside the map.
                for(int j=0;j<4;j++)timber.Box(x+w+.4f,z+.2f+j*.25f,.65f,.17f,.1f,.3f+(j%2)*.15f);
                float pole=x-.9f;
                masonry.Box(pole,z,.12f,.12f,0,tall+2.3f);
                var a=new Vector3(pole+.1f,tall+2.1f,z+.06f);var b=a+Vector3.right*1.25f;
                cloth.Quad(a,a+Vector3.down*.85f,b+Vector3.down*.65f,b);
                cloth.Quad(a,b,b+Vector3.down*.65f,a+Vector3.down*.85f);
                var chimney=new GameObject("Hearth smoke");chimney.transform.SetParent(transform,false);chimney.transform.position=new Vector3(chimneyX+.22f,tall+2.3f,chimneyZ+.24f);
                var smoke=chimney.AddComponent<ParticleSystem>();smoke.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
                var main=smoke.main;main.startLifetime=5;main.startSpeed=.6f;main.startSize=.9f;main.maxParticles=12;main.simulationSpace=ParticleSystemSimulationSpace.World;main.useUnscaledTime=true;main.startColor=ice?new Color(.65f,.72f,.73f,.32f):new Color(.47f,.43f,.38f,.35f);
                var emission=smoke.emission;emission.rateOverTime=1.4f;var shape=smoke.shape;shape.shapeType=ParticleSystemShapeType.Sphere;shape.radius=.1f;
                var velocity=smoke.velocityOverLifetime;velocity.enabled=true;velocity.space=ParticleSystemSimulationSpace.World;velocity.x=.2f;velocity.y=.5f;velocity.z=-.15f;
                var size=smoke.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,AnimationCurve.Linear(0,.4f,1,1.8f));
                var tint=smoke.colorOverLifetime;tint.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(.6f,.2f),new GradientAlphaKey(0,1)});tint.color=gradient;
                var renderer=smoke.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=smokeMaterial;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;smoke.Play();
                Buildings++;
            }
            // Hand-composed groups frame the south exit without filling every border with props.
            House(10,-9,3.7f,3.4f,2.6f);House(18,-14,4.5f,4,3.1f);
            House(width-21,-12,4.2f,3.7f,2.8f);House(width-12,-8,3.2f,3,2.5f);
            House(-8,18,3.4f,4.3f,3);House(width+4,26,3.8f,4.1f,2.7f);
            if(!ice) {
                // Taller industrial silhouette, glowing furnace vents and overhead copper pipes.
                for(int i=0;i<3;i++) {
                    float x=20+i*8,z=-23;
                    masonry.Box(x,z,2.3f,3,0,5+i%2*1.5f);
                    caps.Box(x-.2f,z-.2f,2.7f,3.4f,5+i%2*1.5f,5.25f+i%2*1.5f);
                    for(int v=0;v<3;v++)light.Box(x+.25f+v*.65f,z+3.025f,.3f,.06f,.6f,2.8f);
                    timber.Box(x+1,z+1,.65f,.65f,5,9+i%2);
                }
                caps.Box(19,-21.8f,20,.42f,4.5f,4.92f);
            }
            Save(ice?"Village foundations":"Foundry brickwork",masonry,game.MakeMaterial(ice?new Color(.25f,.32f,.34f):new Color(.24f,.20f,.20f)));
            Save(ice?"Timber lodges":"Forge workshops",timber,game.MakeMaterial(ice?new Color(.31f,.23f,.18f):new Color(.29f,.30f,.29f)));
            Save(ice?"Slate shelter roofs":"Copper workshop roofs",roofs,game.MakeMaterial(ice?new Color(.20f,.31f,.36f):new Color(.39f,.23f,.14f)));
            Save(ice?"Frosted ridges":"Oxidized copper trim",caps,game.MakeMaterial(ice?new Color(.57f,.66f,.68f):new Color(.23f,.40f,.35f)));
            var warm=game.MakeMaterial(new Color(1,.63f,.26f));warm.EnableKeyword("_EMISSION");warm.SetColor("_EmissionColor",new Color(1,.4f,.08f)*.7f);
            Save("Hearth windows",light,warm);
            Save("Wind pennants",cloth,game.MakeMaterial(ice?new Color(.29f,.45f,.49f):new Color(.62f,.31f,.15f)));
            foreach(var child in GetComponentsInChildren<MeshRenderer>())if(child.name!="Settlement paths")child.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.On;
            pennants=meshes[meshes.Count-1];flagRest=pennants.vertices;flagMoved=(Vector3[])flagRest.Clone();
        }
        void Update()
        {
            if(pennants==null||landscapeGame==null)return;
            // Ambient wind uses wall time, independent of the combat speed setting.
            breeze+=Time.unscaledDeltaTime;
            for(int i=0;i<flagRest.Length;i++) {
                var p=flagRest[i];float sway=.075f*Mathf.Sin(breeze*1.7f+p.x*2.3f+p.y*1.1f);
                flagMoved[i]=p+new Vector3(0,sway*.25f,sway);
            }
            pennants.vertices=flagMoved;pennants.RecalculateBounds();
        }
    }
}
