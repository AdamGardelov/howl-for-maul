using UnityEngine;
namespace FrostMaze
{
    public sealed partial class LivingWorld
    {
        // Original split-hearth guardians. Low curved garden walls and tapered wardstones frame
        // the refuge without spanning its road, hiding the exit or adding a gameplay obstacle.
        void BuildSanctuary(WorldBackdrop backdrop)
        {
            var pale=ice?Tint(.88f,.98f,1,0):Tint(1.14f,1.10f,.87f,0);
            var trim=ice?Tint(.43f,.61f,.64f,0):Tint(.69f,.46f,.18f,0);
            var shadow=ice?Tint(.40f,.52f,.56f,0):Tint(.48f,.48f,.35f,0);
            float cx=width*.5f-13.3f,cz=-5.5f;
            Site(cx,0,cz,2.3f);
            // Seat-height curved retaining wall: open toward the halls and the central road.
            const int blocks=13;
            for(int i=0;i<blocks;i++){
                float a=Mathf.Lerp(.52f,Mathf.PI*1.60f,i/(float)blocks),b=Mathf.Lerp(.52f,Mathf.PI*1.60f,(i+1)/(float)blocks)-.017f;
                CurvedBlock(stone,cx,cz,2.05f,2.40f,a,b,-.08f,.54f,pale);
                CurvedBlock(stone,cx,cz,2.01f,2.44f,a,b,.54f,.64f,pale*1.10f);
                CurvedBlock(brass,cx,cz,2.40f,2.44f,a,b,.51f,.55f,trim);
            }
            for(int i=0;i<7;i++){
                float a=.65f+i*.53f;var p=new Vector3(cx+Mathf.Cos(a)*1.55f,.015f,cz+Mathf.Sin(a)*1.55f);
                Garden(p,.39f,17+i*5);
            }
            // The ember well is a small original landmark, not a second enemy exit.
            var basePoint=new Vector3(cx,-.08f,cz);
            Tube(stone,basePoint,basePoint+Vector3.up*.19f,1,.95f,pale,16);
            Tube(stone,basePoint+Vector3.up*.19f,basePoint+Vector3.up*.85f,.58f,.38f,shadow,12);
            Tube(brass,basePoint+Vector3.up*.83f,basePoint+Vector3.up*.98f,.55f,.56f,trim,16);
            for(int side=-1;side<=1;side+=2){
                Vector3 last=basePoint+new Vector3(side*.47f,.83f,0);
                for(int j=1;j<=10;j++){
                    float t=j/10f;var p=basePoint+new Vector3(side*(.47f+.33f*Mathf.Sin(t*Mathf.PI)-.13f*t),.83f+1.43f*t,.18f*Mathf.Sin(t*Mathf.PI));
                    Tube(stone,last,p,Mathf.Lerp(.18f,.035f,(j-1)/10f),Mathf.Lerp(.18f,.035f,t),pale,8);last=p;
                }
            }
            Tube(brass,basePoint+Vector3.up*1.14f,basePoint+Vector3.up*1.90f,.22f,0,Tint(1.45f,.78f,.26f,0),8);
            hearths.Add(new Vector3(cx,1.50f,cz));hearths.Add(new Vector3(width-cx,1.50f,cz));
            // Slim hearth sentinels flank the south road and echo the curved well silhouette.
            float x=width*.5f-2.5f,z=-8.2f;Site(x,0,z,.94f);
            hearths.Add(new Vector3(x,2.65f,z));hearths.Add(new Vector3(width-x,2.65f,z));
            var at=new Vector3(x,-.1f,z);
            Tube(stone,at,at+Vector3.up*.22f,.83f,.76f,pale,12);
            Tube(stone,at+Vector3.up*.22f,at+Vector3.up*2.1f,.47f,.30f,pale,10);
            Tube(brass,at+Vector3.up*.38f,at+Vector3.up*.52f,.49f,.47f,trim,10);
            Tube(brass,at+Vector3.up*1.93f,at+Vector3.up*2.08f,.36f,.43f,trim,10);
            Tube(stone,at+Vector3.up*2.08f,at+Vector3.up*2.40f,.43f,.55f,pale,10);
            Tube(brass,at+Vector3.up*2.41f,at+Vector3.up*2.78f,.28f,.14f,Tint(1.4f,.87f,.35f,0),8);
            for(int side=-1;side<=1;side+=2){
                var last=at+new Vector3(side*.44f,2.29f,0);
                for(int j=1;j<=6;j++){
                    float t=j/6f;var end=at+new Vector3(side*(.44f+.12f*Mathf.Sin(t*Mathf.PI)-.28f*t),2.29f+t*.90f,0);
                    Tube(stone,last,end,.11f*(1-t)+.015f,.09f*(1-t)+.008f,pale,8);last=end;
                }
            }
        }
        static void CurvedBlock(Geometry batch,float x,float z,float inner,float outer,float a,float b,float low,float high,Color color)
        {
            Vector3 P(float angle,float r,float y)=>new Vector3(x+Mathf.Cos(angle)*r,y,z+Mathf.Sin(angle)*r);
            batch.Quad(P(a,inner,high),P(b,inner,high),P(b,outer,high),P(a,outer,high),color,Vector3.up);
            batch.Quad(P(a,outer,low),P(a,outer,high),P(b,outer,high),P(b,outer,low),color);
            batch.Quad(P(b,inner,low),P(b,inner,high),P(a,inner,high),P(a,inner,low),color);
            batch.Quad(P(a,inner,low),P(a,inner,high),P(a,outer,high),P(a,outer,low),color);
            batch.Quad(P(b,outer,low),P(b,outer,high),P(b,inner,high),P(b,inner,low),color);
        }
    }
}
