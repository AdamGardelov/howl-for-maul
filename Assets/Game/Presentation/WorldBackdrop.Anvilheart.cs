using UnityEngine;
namespace FrostMaze
{
    public sealed partial class WorldBackdrop
    {
        static void RefugeBeam(Batch b,Vector3 a,Vector3 end,float width)
        {
            var direction=(end-a).normalized;var side=Vector3.Cross(direction,Vector3.forward).normalized*width*.5f;
            if(side.sqrMagnitude<.00001f)side=Vector3.right*width*.5f;
            var depth=Vector3.Cross(direction,side).normalized*width*.5f;
            b.Quad(a-side-depth,end-side-depth,end+side-depth,a+side-depth);
            b.Quad(a+side+depth,end+side+depth,end-side+depth,a-side+depth);
            b.Quad(a-side+depth,end-side+depth,end-side-depth,a-side-depth);
            b.Quad(a+side-depth,end+side-depth,end+side+depth,a+side+depth);
        }
        static float AnvilRoofHeight(float t)=>2.44f+2.28f*Mathf.Cos(t*Mathf.PI*.5f);
        // Each hand-shaped hall stays within its existing house/eave envelope. Paired by Save().
        static void BuildAnvilheartRoof(Batch stone,Batch copper,Batch roof,Batch timber,Batch glow,float x,float z,float w,float d)
        {
            const int rows=7,tiles=5;float centre=x+w*.5f,half=w*.5f+.4f;
            Vector3 P(int side,float t,float zz)=>new Vector3(centre+side*half*t,AnvilRoofHeight(t),zz);
            for(int side=-1;side<=1;side+=2)for(int row=0;row<rows;row++){
                float t0=row/(float)rows,t1=(row+1)/(float)rows;
                for(int tile=0;tile<tiles;tile++){
                    float za=z-.4f+tile*(d+.8f)/tiles,zb=za+(d+.8f)/tiles-.018f;
                    var a=P(side,t0,za);var b=P(side,t0,zb);var c=P(side,t1,zb);var e=P(side,t1,za);
                    if(side<0)roof.Quad(e,c,b,a);else roof.Quad(a,b,c,e);
                    RefugeBeam(copper,e,c,.045f);
                }
                for(int rib=0;rib<4;rib++){
                    float zz=z-.43f+rib*(d+.86f)/3;
                    RefugeBeam(copper,P(side,t0,zz)+Vector3.up*.04f,P(side,t1,zz)+Vector3.up*.04f,rib==0||rib==3?.16f:.10f);
                }
                for(int face=-1;face<=1;face+=2){
                    float zz=z+d*.5f+face*(d*.5f+.405f);
                    var a=P(side,t0,zz);var b=P(side,t1,zz);var c=new Vector3(b.x,2.34f,zz);var e=new Vector3(a.x,2.34f,zz);
                    if(side*face<0)stone.Quad(a,b,c,e);else stone.Quad(e,c,b,a);
                }
            }
            // A deep layered eave and raised ridge give the roof weight at normal camera distance.
            for(int side=-1;side<=1;side+=2){
                timber.Box(centre+side*half-.085f,z-.43f,.17f,d+.86f,2.24f,2.43f);
                for(int brace=0;brace<3;brace++){
                    float zz=z+.3f+brace*(d-.6f)/2;
                    RefugeBeam(timber,new Vector3(centre+side*(w*.5f-.08f),1.66f,zz),new Vector3(centre+side*half,2.32f,zz),.17f);
                }
            }
            copper.Box(centre-.16f,z-.48f,.32f,d+.96f,4.69f,4.83f);
            for(int face=-1;face<=1;face+=2){
                float zz=z+d*.5f+face*(d*.5f+.43f);
                RefugeBeam(copper,new Vector3(x-.38f,2.36f,zz),new Vector3(x+w+.38f,2.36f,zz),.16f);
                // Broad timber braces and two warm slit windows articulate the formerly flat gable.
                for(int side=-1;side<=1;side+=2){
                    RefugeBeam(timber,new Vector3(centre+side*1.6f,2.42f,zz),new Vector3(centre+side*.55f,3.94f,zz),.14f);
                    float wx=centre+side*1.15f;
                    copper.Box(wx-.20f,zz-.045f,.40f,.09f,2.81f,3.43f);
                    glow.Box(wx-.13f,zz+face*.055f,.26f,.028f,2.90f,3.35f);
                }
            }
        }
        static void RefugeRing(Batch b,float x,float z,float low,float high,float outer0,float outer1,float inner)
        {
            const int sides=10;
            Vector3 P(int i,float y,float r){float a=i*Mathf.PI*2/sides;return new Vector3(x+Mathf.Cos(a)*r,y,z+Mathf.Sin(a)*r);}
            for(int i=0;i<sides;i++){
                b.Quad(P(i,low,outer0),P(i,high,outer1),P(i+1,high,outer1),P(i+1,low,outer0));
                b.Quad(P(i,high,outer1),P(i,high,inner),P(i+1,high,inner),P(i+1,high,outer1));
                b.Quad(P(i,high,inner),P(i,low,inner),P(i+1,low,inner),P(i+1,high,inner));
            }
        }
        static void BuildAnvilheartChimney(Batch stone,Batch copper,Batch dark,float x,float z)
        {
            RefugeRing(stone,x,z,1.8f,4.88f,.61f,.46f,.29f);
            for(int course=0;course<5;course++)RefugeRing(copper,x,z,3.15f+course*.34f,3.23f+course*.34f,.51f,.51f,.29f);
            RefugeRing(stone,x,z,4.84f,5.07f,.47f,.62f,.31f);
            RefugeRing(stone,x,z,5.07f,5.24f,.62f,.62f,.35f);
            RefugeRing(copper,x,z,5.24f,5.32f,.63f,.63f,.35f);
            dark.Box(x-.24f,z-.24f,.48f,.48f,4.66f,4.68f);
        }
    }
}
