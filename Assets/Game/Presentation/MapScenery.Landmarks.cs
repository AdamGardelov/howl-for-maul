using UnityEngine;
namespace FrostMaze
{
    public sealed partial class MapScenery
    {
        const int SceneryBatchCount=35;
        Texture2D landmarkEmbers,landmarkWear;

        static void HipHood(Batch b,float x,float z,float y,float lowWidth,float lowDepth,float highWidth,float highDepth,float height)
        {
            var a=new Vector3(x-lowWidth,y,z-lowDepth);var d=new Vector3(x+lowWidth,y,z-lowDepth);
            var c=new Vector3(x+lowWidth,y,z+lowDepth);var e=new Vector3(x-lowWidth,y,z+lowDepth);
            var aa=new Vector3(x-highWidth,y+height,z-highDepth);var dd=new Vector3(x+highWidth,y+height,z-highDepth);
            var cc=new Vector3(x+highWidth,y+height,z+highDepth);var ee=new Vector3(x-highWidth,y+height,z+highDepth);
            b.Quad(a,aa,dd,d);b.Quad(d,dd,cc,c);b.Quad(c,cc,ee,e);b.Quad(e,ee,aa,a);b.Quad(aa,ee,cc,dd);
        }
        static void HearthArch(Batch stone,Batch glow,float x,float z,float y,float radius)
        {
            float spring=y+.68f,depth=.16f;
            glow.Box(x-radius,z+.02f,radius*2,.025f,y,spring);
            for(int i=0;i<11;i++){
                float a=i*Mathf.PI/11+.014f,b=(i+1)*Mathf.PI/11-.014f;
                Vector3 P(float angle,float r,float dz=0)=>new Vector3(x+Mathf.Cos(angle)*r,spring+Mathf.Sin(angle)*r,z+dz);
                stone.Quad(P(a,radius),P(b,radius),P(b,radius+.23f),P(a,radius+.23f));
                stone.Quad(P(a,radius+.23f),P(b,radius+.23f),P(b,radius+.23f,depth),P(a,radius+.23f,depth));
                stone.Quad(P(a,radius,depth),P(b,radius,depth),P(b,radius),P(a,radius));
                glow.Quad(new Vector3(x,spring,z+.018f),P(b,radius,.018f),P(a,radius,.018f),new Vector3(x,spring,z+.018f));
            }
            for(int side=-1;side<=1;side+=2)for(int course=0;course<3;course++)
                stone.Box(x+side*(radius+.12f)-.12f,z,.24f,depth,y+course*.23f,y+course*.23f+.215f);
        }
        static void ForgeLandmark(Batch[] b,float x,float z,float y,int variant)
        {
            var stone=b[30];var copper=b[31];var wood=b[32];var iron=b[33];var glow=b[29];
            // Three low steps, a recessed arch and shoulder piers anchor the forge to its shelf.
            for(int step=0;step<3;step++)stone.Box(x-1.1f+step*.06f,z-.97f+step*.055f,2.2f-step*.12f,1.90f-step*.10f,y+.10f+step*.10f,y+.19f+step*.10f);
            stone.Box(x-.78f,z-.57f,1.56f,1.24f,y+.35f,y+1.95f);
            for(int side=-1;side<=1;side+=2){
                stone.Box(x+side*.88f-.17f,z-.65f,.34f,1.40f,y+.28f,y+1.87f);
                copper.Box(x+side*.88f-.22f,z-.71f,.44f,1.52f,y+1.78f,y+1.91f);
            }
            iron.Box(x-.61f,z-.73f,1.22f,.04f,y+.43f,y+1.51f);
            HearthArch(stone,glow,x,z-.78f,y+.43f,.49f);
            for(int bar=-2;bar<=2;bar++)iron.Box(x+bar*.17f-.024f,z-.82f,.048f,.07f,y+.42f,y+1.31f+(2-Mathf.Abs(bar))*.10f);
            // A sloping copper hood feeds a masonry chimney, with layered collars and a real dark throat.
            HipHood(copper,x,z+.04f,y+1.94f,1.06f,.80f,.40f,.38f,.72f);
            iron.Box(x-1.08f,z-.79f,2.16f,.08f,y+1.91f,y+2.02f);
            float cx=x+.15f,cz=z+.06f,top=y+3.73f+(variant%2)*.18f;
            stone.Box(cx-.34f,cz-.33f,.68f,.66f,y+2.57f,top);
            for(int band=0;band<3;band++)copper.Box(cx-.39f,cz-.38f,.78f,.76f,y+2.70f+band*.40f,y+2.78f+band*.40f);
            // Hollow cap assembled from four cornice stones rather than a pointed solid flue.
            for(int level=0;level<2;level++){
                float r=.46f+level*.035f,h=top-.06f+level*.14f;
                stone.Box(cx-r,cz-r,r*2,.16f,h,h+.12f);stone.Box(cx-r,cz+r-.16f,r*2,.16f,h,h+.12f);
                stone.Box(cx-r,cz-r+.16f,.16f,r*2-.32f,h,h+.12f);stone.Box(cx+r-.16f,cz-r+.16f,.16f,r*2-.32f,h,h+.12f);
            }
            iron.Box(cx-.26f,cz-.25f,.52f,.50f,top-.09f,top-.07f);
            // Side anvil, a bound fuel stack and broad rivets make this a working place.
            wood.Box(x-1.66f,z-.29f,.45f,.67f,y+.25f,y+.79f);
            iron.Box(x-1.76f,z-.36f,.65f,.82f,y+.78f,y+.93f);
            HipHood(iron,x-1.43f,z-.01f,y+.94f,.31f,.37f,.18f,.18f,.18f);
            copper.Box(x-1.73f,z-.30f,.59f,.08f,y+.85f,y+.94f);
            for(int log=0;log<4;log++)wood.Box(x+1.22f+(log%2)*.23f,z-.33f+(log/2)*.38f,.21f,.71f,y+.23f,y+.43f+(log%2)*.18f);
            iron.Box(x+1.20f,z-.04f,.51f,.06f,y+.24f,y+.67f);
            for(int side=-1;side<=1;side+=2)for(int rivet=0;rivet<3;rivet++)
                copper.Box(x+side*.87f-.04f,z-.74f,.08f,.07f,y+.65f+rivet*.34f,y+.73f+rivet*.34f);
        }
        static void WayshrineLandmark(Batch[] b,float x,float z,float y,int variant)
        {
            var stone=b[30];var copper=b[31];var wood=b[32];var iron=b[33];var snow=b[34];
            for(int step=0;step<3;step++)stone.Box(x-1.16f+step*.09f,z-.80f+step*.05f,2.32f-step*.18f,1.60f-step*.10f,y+.10f+step*.11f,y+.19f+step*.11f);
            for(int side=-1;side<=1;side+=2){
                float px=x+side*.89f;
                stone.Box(px-.22f,z-.34f,.44f,.68f,y+.32f,y+1.25f);
                stone.Box(px-.28f,z-.40f,.56f,.80f,y+1.14f,y+1.34f);
                wood.Box(px-.15f,z-.20f,.30f,.40f,y+1.33f,y+2.77f);
                for(int band=0;band<2;band++)copper.Box(px-.175f,z-.23f,.35f,.46f,y+1.45f+band*.85f,y+1.55f+band*.85f);
                Beam(wood,new Vector3(px,y+2.12f,z),new Vector3(x+side*.38f,y+2.78f,z),.085f);
            }
            wood.Box(x-1.36f,z-.27f,2.72f,.54f,y+2.72f,y+2.93f);
            // Three swept roof courses, visible metal eaves and irregular shallow snow ledges.
            for(int side=-1;side<=1;side+=2)for(int row=0;row<3;row++)for(int tile=0;tile<5;tile++){
                float u0=row/3f,u1=(row+1)/3f;
                Vector3 P(float u,float zz)=>new Vector3(x+side*u*1.65f,y+3.65f-.95f*u+.15f*u*u,zz);
                float za=z-.84f+tile*.336f,zb=za+.314f;
                var a=P(u0,za);var d=P(u1,za);var c=P(u1,zb);var e=P(u0,zb);
                if(side<0)copper.Quad(d,c,e,a);else copper.Quad(a,e,c,d);
                Beam(wood,d,c,.045f);
                // Leave a little painted roof showing beneath each snow edge.
                var aa=Vector3.Lerp(a,d,.10f)+Vector3.up*.06f;var ee=Vector3.Lerp(e,c,.10f)+Vector3.up*.06f;
                var dd=Vector3.Lerp(a,d,.83f+(tile%2)*.08f)+Vector3.up*.06f;var cc=Vector3.Lerp(e,c,.86f)+Vector3.up*.06f;
                if(side<0)snow.Quad(dd,cc,ee,aa);else snow.Quad(aa,ee,cc,dd);
            }
            for(int face=-1;face<=1;face+=2){
                float zz=z+face*.86f;
                Beam(wood,new Vector3(x-1.65f,y+2.85f,zz),new Vector3(x,y+3.65f,zz),.09f);
                Beam(wood,new Vector3(x,y+3.65f,zz),new Vector3(x+1.65f,y+2.85f,zz),.09f);
                Beam(copper,new Vector3(x,y+3.45f,zz),new Vector3(x,y+3.05f,zz),.045f);
            }
            // A copper wardbell and clapper are readable under the open roof.
            iron.Box(x-.022f,z-.022f,.044f,.044f,y+2.20f,y+2.77f);
            Ring(copper,x,z,y+1.61f,.63f,.42f,.17f);Ring(copper,x,z,y+1.56f,.09f,.46f,.42f);
            iron.Box(x-.04f,z-.04f,.08f,.08f,y+1.48f,y+1.89f);
            stone.Box(x-.37f,z+.34f,.74f,.39f,y+.32f,y+.86f);
            if(variant!=2)b[29].Peak(x,z+.52f,.20f,y+.86f,variant==1?.45f:.64f);
            for(int side=-1;side<=1;side+=2)stone.Box(x+side*1.35f-.16f,z+.33f,.32f,.43f,y+.08f,y+.47f);
        }
        void LandmarkMaterial(Material material,int batch,bool ice)
        {
            if(batch==25){material.mainTexture=Resources.Load<Texture2D>("World/HearthSlate");material.color=ice?new Color(.58f,.72f,.77f):new Color(.62f,.65f,.53f);}
            if(batch==30){material.mainTexture=Resources.Load<Texture2D>("World/HearthMasonry");material.color=ice?new Color(.76f,.88f,.94f):new Color(.85f,.81f,.69f);}
            if(batch==31){material.mainTexture=Resources.Load<Texture2D>("World/HearthCopper");material.color=ice?new Color(.59f,.77f,.82f):new Color(.90f,.86f,.68f);material.SetFloat("_Metallic",.24f);material.SetFloat("_Smoothness",.23f);}
            if(batch==32){landmarkWear=WorldBackdrop.RefugePatina();material.mainTexture=landmarkWear;}
            if(batch==29&&!ice){
                landmarkEmbers=WorldBackdrop.HearthEmbers();material.mainTexture=landmarkEmbers;material.color=Color.white;
                material.EnableKeyword("_EMISSION");material.SetTexture("_EmissionMap",landmarkEmbers);material.SetColor("_EmissionColor",Color.white*.65f);
            }
        }
        void CompositionUV(Mesh mesh,float width,int batch,bool ice)
        {
            var vertices=mesh.vertices;var normals=mesh.normals;var uv=new Vector2[vertices.Length];
            for(int i=0;i<vertices.Length;i++){
                var p=vertices[i];var n=normals[i];
                if(batch==29&&!ice){
                    Vector3 anchor=default;float distance=float.MaxValue;
                    foreach(var site in landmarks){float d=new Vector2(p.x-site.x,p.z-site.z).sqrMagnitude;if(d<distance){distance=d;anchor=site;}}
                    uv[i]=new Vector2(Mathf.Clamp01((p.x-anchor.x)/(.98f*.72f)+.5f),Mathf.Clamp01((p.y-anchor.y-.43f)/1.17f));continue;
                }
                p.x=Mathf.Min(p.x,width-p.x);
                // Project each face so top surfaces retain painted detail instead of a stretched row.
                uv[i]=Mathf.Abs(n.y)>.65f?new Vector2(p.x,p.z)/2.8f:
                    Mathf.Abs(n.x)>Mathf.Abs(n.z)?new Vector2(p.z,p.y)/2.8f:new Vector2(p.x,p.y)/2.8f;
            }
            mesh.uv=uv;
        }
    }
}
