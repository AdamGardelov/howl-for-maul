using UnityEngine;
namespace FrostMaze
{
    public sealed partial class WorldBackdrop
    {
        // Hand-authored refuge architecture. Every vertex stays south of the simulation.
        // Save() reflects the left hall, preserving both the exit opening and map symmetry.
        Texture2D refugeSurface,refugeEmbers;
        Material refugeGlow;
        public int RefugeVertices { get; private set; }
        public System.Collections.Generic.IReadOnlyList<Vector3> RefugeHearths=>refugeHearths;
        readonly System.Collections.Generic.List<Vector3> refugeHearths=new System.Collections.Generic.List<Vector3>();
        void BuildRefuge(Prototype game,bool ice,float width)
        {
            var stone=new Batch();var joints=new Batch();var metal=new Batch();var roof=new Batch();
            var roofEdges=new Batch();var timber=new Batch();var glow=new Batch();var dark=new Batch();
            float x=width*.5f-9.8f,z=-6.1f,w=6.2f,d=4.7f;
            refugeHearths.Add(new Vector3(x+w*.5f,1.2f,z+d+.6f));refugeHearths.Add(new Vector3(width-x-w*.5f,1.2f,z+d+.6f));
            void Beam(Batch batch,Vector3 a,Vector3 b,float thickness) {
                var along=(b-a).normalized;var side=Vector3.Cross(along,Vector3.forward).normalized*thickness*.5f;
                if(side.sqrMagnitude<.00001f)side=Vector3.right*thickness*.5f;
                var depth=Vector3.Cross(along,side).normalized*thickness*.5f;
                batch.Quad(a-side-depth,b-side-depth,b+side-depth,a+side-depth);
                batch.Quad(a+side+depth,b+side+depth,b-side+depth,a-side+depth);
                batch.Quad(a-side+depth,b-side+depth,b-side-depth,a-side-depth);
                batch.Quad(a+side-depth,b+side-depth,b+side+depth,a+side+depth);
            }
            // Broad chamfered foundation / three low steps, with a completely open central road.
            for(int tier=0;tier<3;tier++)stone.Box(x-.35f+tier*.12f,z-.3f+tier*.1f,w+.7f-tier*.24f,d+.65f-tier*.2f,-.12f+tier*.13f,.04f+tier*.13f);
            joints.Box(x,z,w,d,.2f,2.45f);
            for(int course=0;course<5;course++)for(int i=0;i<7;i++) {
                float start=x+i*.91f+(course%2)*.43f,wide=Mathf.Min(.85f,x+w-start);
                if(wide<=0)continue;
                for(int face=0;face<2;face++)stone.Box(start,z+(face==0?-.045f:d-.12f),wide,.17f,.3f+course*.41f,.66f+course*.41f);
            }
            // Buttresses and corner caps are intentionally chunky at the ordinary RTS camera.
            for(int side=0;side<2;side++)for(int face=0;face<2;face++) {
                float xx=x+side*(w-.32f)-.12f,zz=z+face*(d-.3f)-.1f;
                stone.Box(xx,zz,.56f,.52f,.18f,2.58f);
                metal.Box(xx-.06f,zz-.05f,.68f,.62f,2.46f,2.6f);
            }
            // Slate/copper shingles overlap down two pitched roof slopes; no flat box roof.
            for(int side=0;side<2;side++)for(int row=0;row<4;row++) {
                float u0=row/4f,u1=(row+1)/4f;
                float xa=x+w*.5f+(side==0?-1:1)*(w*.5f+.4f)*u0;
                float xb=x+w*.5f+(side==0?-1:1)*(w*.5f+.4f)*u1;
                float ya=4.12f-u0*1.68f,yb=4.12f-u1*1.68f;
                for(int tile=0;tile<6;tile++) {
                    float za=z-.4f+tile*(d+.8f)/6,zb=za+(d+.8f)/6-.028f;
                    var a=new Vector3(xa,ya+.035f,za);var b=new Vector3(xa,ya+.035f,zb);
                    var c=new Vector3(xb,yb+.035f,zb);var e=new Vector3(xb,yb+.035f,za);
                    if(side==0)roof.Quad(e,c,b,a);else roof.Quad(a,b,c,e);
                    Beam(roofEdges,e,c,.075f);
                    if(ice&&row<3) {
                        // A shallow irregular snow strip leaves the layered slate edge visible.
                        var snowA=Vector3.Lerp(a,e,.1f)+Vector3.up*.035f;var snowB=Vector3.Lerp(b,c,.1f)+Vector3.up*.035f;
                        if(side==0)roofEdges.Quad(e+Vector3.up*.03f,c+Vector3.up*.03f,snowB,snowA);
                        else roofEdges.Quad(snowA,snowB,c+Vector3.up*.03f,e+Vector3.up*.03f);
                    }
                }
            }
            for(int face=0;face<2;face++) {
                float zz=z+(face==0?-.41f:d+.41f);
                var a=new Vector3(x-.4f,2.44f,zz);var tip=new Vector3(x+w*.5f,4.12f,zz);var b=new Vector3(x+w+.4f,2.44f,zz);
                if(face==0)timber.Triangle(a,tip,b);else timber.Triangle(b,tip,a);
                Beam(metal,a,tip,.18f);Beam(metal,tip,b,.18f);Beam(metal,a,b,.16f);
                // Furnace/ward arch: luminous core recessed behind a many-sided stone ring.
                float cx=x+w*.5f,baseY=.42f,archY=1.5f,r=.87f;
                float front=zz+(face==0?-.025f:.025f);
                glow.Box(cx-r,front,r*2,.025f,baseY,archY);
                for(int n=0;n<9;n++) {
                    float a0=n*Mathf.PI/9,a1=(n+1)*Mathf.PI/9;
                    Vector3 P(float angle,float rad)=>new Vector3(cx+Mathf.Cos(angle)*rad,archY+Mathf.Sin(angle)*rad,front);
                    var p0=P(a0,r);var p1=P(a1,r);var q0=P(a0,r+.23f);var q1=P(a1,r+.23f);
                    if(face==0){glow.Triangle(new Vector3(cx,archY,front),p1,p0);stone.Quad(p1,q1,q0,p0);}
                    else{glow.Triangle(new Vector3(cx,archY,front),p0,p1);stone.Quad(p0,q0,q1,p1);}
                }
                for(int side=-1;side<=1;side+=2)stone.Box(cx+side*(r+.1f)-.12f,front-.04f,.24f,.15f,baseY,archY);
                for(int bar=-2;bar<=2;bar++)metal.Box(cx+bar*.29f-.035f,front+(face==0?-.035f:.035f),.07f,.045f,baseY,1.65f+(2-Mathf.Abs(bar))*.19f);
                // A simple, original split-hearth insignia above the arch.
                float signY=3.05f;
                for(int side=-1;side<=1;side+=2) {
                    Beam(metal,new Vector3(cx,signY-.12f,front),new Vector3(cx+side*.36f,signY+.18f,front),.105f);
                    Beam(metal,new Vector3(cx+side*.36f,signY+.18f,front),new Vector3(cx,signY+.64f,front),.105f);
                }
                glow.Box(cx-.055f,front-.015f,.11f,.07f,signY,signY+.42f);
            }
            // Tapered chimney, stacked cornices and a dark flue opening.
            float chimneyX=x+.7f,chimneyZ=z+1.4f;
            stone.Box(chimneyX,chimneyZ,.82f,.92f,1.8f,4.65f);
            for(int i=0;i<3;i++)metal.Box(chimneyX-.1f-i*.035f,chimneyZ-.1f-i*.035f,1.02f+i*.07f,1.12f+i*.07f,4.45f+i*.16f,4.55f+i*.16f);
            dark.Box(chimneyX+.12f,chimneyZ+.12f,.58f,.68f,4.88f,4.895f);
            // A hanging wardbell beside the road, outside the map and outside the exit corridor.
            float bellX=x+w+.65f,bellZ=-3.3f;
            for(int side=-1;side<=1;side+=2)timber.Box(bellX+side*.58f-.07f,bellZ,.14f,.18f,-.1f,2.35f);
            timber.Box(bellX-.78f,bellZ-.04f,1.56f,.26f,2.27f,2.48f);
            metal.Peak(bellX,bellZ+.09f,.42f,1.38f,.58f);
            metal.Box(bellX-.38f,bellZ-.26f,.76f,.70f,1.35f,1.44f);
            dark.Box(bellX-.06f,bellZ+.03f,.12f,.12f,1.19f,1.5f);
            metal.Box(bellX-.025f,bellZ+.065f,.05f,.05f,1.85f,2.3f);
            var warm=game.MakeMaterial(ice?new Color(1,.70f,.34f):new Color(1,.56f,.16f));
            refugeEmbers=HearthEmbers();warm.color=Color.white;warm.mainTexture=refugeEmbers;
            warm.EnableKeyword("_EMISSION");warm.SetTexture("_EmissionMap",refugeEmbers);warm.SetColor("_EmissionColor",Color.white*.7f);refugeGlow=warm;
            var batches=new[]{stone,joints,metal,roof,roofEdges,timber,glow,dark};
            var colors=new[]{ice?new Color(.40f,.49f,.50f):new Color(.39f,.37f,.30f),new Color(.15f,.19f,.20f),new Color(.51f,.36f,.16f),ice?new Color(.20f,.32f,.37f):new Color(.27f,.38f,.33f),ice?new Color(.73f,.83f,.81f):new Color(.40f,.46f,.32f),new Color(.24f,.20f,.15f),Color.white,new Color(.055f,.07f,.075f)};
            refugeSurface=RefugePatina();
            for(int i=0;i<batches.Length;i++) {
                var material=i==6?warm:game.MakeMaterial(colors[i]);
                if(i!=6&&i!=7)material.mainTexture=refugeSurface;
                if(i==0){material.mainTexture=Resources.Load<Texture2D>("World/HearthMasonry");material.color=ice?new Color(.76f,.89f,.96f):new Color(.93f,.89f,.76f);}
                if(i==3){material.mainTexture=Resources.Load<Texture2D>("World/HearthCopper");material.color=ice?new Color(.52f,.73f,.82f):new Color(.93f,1,.91f);}
                Save((ice?"Hearthward":"Anvilheart")+" refuge "+i,batches[i],material);
                RefugeVertices+=meshes[meshes.Count-1].vertexCount;
                transform.GetChild(transform.childCount-1).GetComponent<MeshRenderer>().shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.On;
            }
        }
        static Texture2D HearthEmbers() {
            const int width=128,height=128;var pixels=new Color[width*height];
            for(int y=0;y<height;y++)for(int x=0;x<width;x++) {
                float u=x/(width-1f),v=y/(height-1f),rim=Mathf.Sin(u*Mathf.PI);
                float tongues=Mathf.PerlinNoise(u*7+17,v*3+5);
                float heat=Mathf.Clamp01(rim*1.25f-v*.55f+(tongues-.5f)*.55f);
                float coals=Mathf.PerlinNoise(u*29+31,v*21+7);
                var color=Color.Lerp(new Color(.07f,.035f,.02f),new Color(.94f,.22f,.015f),Mathf.SmoothStep(0,1,heat));
                color=Color.Lerp(color,new Color(1,.78f,.26f),Mathf.Pow(heat,5)*(.5f+coals*.5f));
                pixels[y*width+x]=color;
            }
            var texture=new Texture2D(width,height,TextureFormat.RGB24,true){name="Original banked hearth embers",wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Trilinear};texture.SetPixels(pixels);texture.Apply(true,true);return texture;
        }
        static Texture2D RefugePatina() {
            const int size=256;var pixels=new Color[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++) {
                float u=x/(float)size,v=y/(float)size;
                float broad=MapScenery.SnowNoise(u*64,v*64,.24f,21,11);
                float wear=MapScenery.SnowNoise(u*64,v*64,1.1f,15,4);
                float pigment=.70f+broad*.30f+wear*.10f;
                pixels[y*size+x]=new Color(pigment*.97f,pigment,pigment*.94f);
            }
            var texture=new Texture2D(size,size,TextureFormat.RGB24,true){name="Original painted refuge patina",wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Trilinear,anisoLevel=4};
            texture.SetPixels(pixels);texture.Apply(true,true);return texture;
        }
    }
}
