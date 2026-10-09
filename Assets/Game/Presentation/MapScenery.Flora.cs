using UnityEngine;
using FrostMaze.Simulation;
namespace FrostMaze
{
    public sealed partial class MapScenery
    {
        Prototype sceneryGame;
        public int PlantClusters { get; private set; }
        public int Braziers { get; private set; }

        // Complete footprints must stay inside raised terrain, including on half-cell maps.
        static bool ScenicFootprint(Scenario c,float x,float z,float radius)
        {
            float cell=c.LayoutCellSize;
            for(int y=Mathf.FloorToInt((z-radius)/cell);y<=Mathf.FloorToInt((z+radius)/cell);y++)
            for(int col=Mathf.FloorToInt((x-radius)/cell);col<=Mathf.FloorToInt((x+radius)/cell);col++) {
                int row=c.LayoutRows.Length-1-y;
                if(row<0||row>=c.LayoutRows.Length||col<0||col>=c.LayoutRows[row].Length)return false;
                char k=c.LayoutRows[row][col];
                if(c.WalkableSymbols.IndexOf(k)>=0||k=='D'||k=='W'||k=='p')return false;
            }
            return true;
        }
        static void Leaf(Batch b,Vector3 root,Vector3 tip,float width)
        {
            var direction=tip-root;var side=new Vector3(-direction.z,0,direction.x).normalized*width;
            var middle=Vector3.Lerp(root,tip,.55f)+Vector3.up*.14f;
            b.Quad(root,middle-side,tip,middle+side);
            b.Quad(root,middle+side,tip,middle-side);
        }
        static void Ring(Batch b,float x,float z,float bottom,float height,float low,float high)
        {
            for(int i=0;i<8;i++) {
                float a=i*Mathf.PI/4,c=(i+1)*Mathf.PI/4;
                b.Quad(new Vector3(x+Mathf.Cos(a)*low,bottom,z+Mathf.Sin(a)*low),
                    new Vector3(x+Mathf.Cos(a)*high,bottom+height,z+Mathf.Sin(a)*high),
                    new Vector3(x+Mathf.Cos(c)*high,bottom+height,z+Mathf.Sin(c)*high),
                    new Vector3(x+Mathf.Cos(c)*low,bottom,z+Mathf.Sin(c)*low));
            }
        }
        void BuildFlora(Batch[] b,Scenario c,bool ice)
        {
            float surface=ice?.72f:.6f;
            bool ClearExisting(float x,float z) {
                foreach(int batch in new[]{3,11,12})foreach(var v in b[batch].V)
                    if((v.x-x)*(v.x-x)+(v.z-z)*(v.z-z)<1.15f*1.15f)return false;
                return true;
            }
            void Plant(float x,float z,int seed) {
                float turn=seed*.73f,size=.7f+(seed%6)*.08f;
                for(int i=0;i<7;i++) {
                    float a=turn+i*Mathf.PI*2/7,length=(.42f+(i%3)*.09f)*size;
                    var root=new Vector3(x,surface+.025f,z);
                    var tip=root+new Vector3(Mathf.Cos(a)*length,(.28f+(i%3)*.12f)*size,Mathf.Sin(a)*length);
                    Leaf(b[14],root,tip,(ice?.14f:.105f)*size);
                    Leaf(b[15],root+Vector3.up*.012f,tip+Vector3.up*.014f,.016f*size);
                    Leaf(b[15],Vector3.Lerp(root,tip,.66f),tip+Vector3.up*.015f,(ice?.065f:.045f)*size);
                }
                // Ice blooms / copper seed heads: small accents, never a glowing gameplay marker.
                for(int i=0;i<3;i++) {
                    float a=turn+i*2.1f;
                    b[15].Peak(x+Mathf.Cos(a)*.18f,z+Mathf.Sin(a)*.18f,.09f,surface+.18f,.3f+i*.05f);
                }
                // Low hardy grass and broken slate fragments add depth inside the same safe footprint.
                if(seed%3==0)for(int i=0;i<9;i++) {
                    float a=turn+i*2.4f;var root=new Vector3(x+Mathf.Cos(a)*.26f,surface+.02f,z+Mathf.Sin(a)*.26f);
                    var tip=root+new Vector3(Mathf.Cos(a)*.19f,.18f+(i%4)*.055f,Mathf.Sin(a)*.19f);
                    Leaf(b[20],root,tip,.033f);
                    Leaf(b[21],Vector3.Lerp(root,tip,.7f),tip,.022f);
                }
                if(seed%5==0)for(int i=0;i<3;i++) {
                    float a=turn+i*2.1f,rx=x+Mathf.Cos(a)*.39f,rz=z+Mathf.Sin(a)*.39f;
                    b[22].Peak(rx,rz,.17f,surface,.15f+i*.035f);
                    b[23].Peak(rx,rz,.12f,surface+.055f,.105f+i*.025f);
                }
                PlantClusters++;
            }
            void Brazier(float x,float z) {
                Ring(b[16],x,z,surface,.16f,.31f,.23f);
                b[16].Peak(x,z,.23f,surface+.16f,.005f);
                Ring(b[16],x,z,surface+.16f,.48f,.15f,.13f);
                Ring(b[16],x,z,surface+.64f,.2f,.17f,.34f);
                Ring(b[15],x,z,surface+.82f,.055f,.35f,.35f);
                b[16].Peak(x,z,.32f,surface+.83f,.005f);
                b[17].Peak(x,z,.23f,surface+.82f,.65f);
                b[18].Peak(x+.035f,z-.16f,.12f,surface+.84f,.46f);
                for(int i=0;i<3;i++) {
                    float a=i*Mathf.PI*2/3;
                    b[17].Peak(x+Mathf.Cos(a)*.14f,z+Mathf.Sin(a)*.14f,.095f,surface+.84f,.32f);
                }
                fireAnchors.Add(new Vector3(x,surface+.82f,z));
                Braziers++;
            }
            // Deterministic sparse dressing. No random state shared with simulation and no physical colliders.
            for(int iz=0;iz<Mathf.CeilToInt((c.Height-4)/3.5f);iz++)for(int ix=0;ix<Mathf.CeilToInt((c.Width-4)/3.5f);ix++) {
                int seed=(ix*73+iz*139+ix*iz*11)%97;
                float x=2+ix*3.5f+(seed%7-3)*.3f,z=2+iz*3.5f+(seed%11-5)*.18f;
                if(!ScenicFootprint(c,x,z,.76f)||!ClearExisting(x,z))continue;
                // Focus clusters near lanes; retain a few deeper plants to break up broad flat caps.
                bool nearLane=false;
                for(int j=0;j<8;j++) {
                    float a=j*Mathf.PI/4,px=x+Mathf.Cos(a)*2.5f,pz=z+Mathf.Sin(a)*2.5f;
                    int row=c.LayoutRows.Length-1-Mathf.FloorToInt(pz/c.LayoutCellSize),col=Mathf.FloorToInt(px/c.LayoutCellSize);
                    if(row>=0&&row<c.LayoutRows.Length&&col>=0&&col<c.LayoutRows[row].Length&&c.WalkableSymbols.IndexOf(c.LayoutRows[row][col])>=0)nearLane=true;
                }
                if(!nearLane&&seed%4!=0)continue;
                if(nearLane&&seed%4==0&&Braziers<24)Brazier(x,z);
                else if(PlantClusters<90)Plant(x,z,seed);
            }
        }
    }
}
