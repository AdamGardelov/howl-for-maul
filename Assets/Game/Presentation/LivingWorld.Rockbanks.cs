using UnityEngine;
namespace FrostMaze
{
    public sealed partial class LivingWorld
    {
        readonly Geometry rockbanks=new Geometry(),bankMoss=new Geometry();
        public int RockBanks {get;private set;}
        readonly System.Collections.Generic.List<Vector4> shoreSites=new System.Collections.Generic.List<Vector4>();
        public System.Collections.Generic.IReadOnlyList<Vector4> ShoreSites=>shoreSites;
        // The dark pit cells remain non-walkable cooling channels. Sparse rooted outcrops may
        // occupy their banks, joining existing stone. Their entire footprint remains impassable.
        public static bool FitsShore(FrostMaze.Simulation.Scenario c,float x,float z,float radius) {
            if(c.Theme!="iron")return false;
            float cell=c.LayoutCellSize;int centreRow=c.LayoutRows.Length-1-Mathf.FloorToInt(z/cell),centreCol=Mathf.FloorToInt(x/cell);
            if(centreRow<0||centreRow>=c.LayoutRows.Length||centreCol<0||centreCol>=c.LayoutRows[centreRow].Length||c.LayoutRows[centreRow][centreCol]!='p')return false;
            radius+=WindEnvelope+.035f;
            for(int zz=Mathf.FloorToInt((z-radius)/cell);zz<=Mathf.FloorToInt((z+radius)/cell);zz++)
            for(int xx=Mathf.FloorToInt((x-radius)/cell);xx<=Mathf.FloorToInt((x+radius)/cell);xx++){
                int row=c.LayoutRows.Length-1-zz;
                if(row<0||row>=c.LayoutRows.Length||xx<0||xx>=c.LayoutRows[row].Length||(c.LayoutRows[row][xx]!='p'&&c.LayoutRows[row][xx]!='#'))return false;
            }
            // Every outcrop belongs to a nearby existing stone bank, never a random mid-channel island.
            int reach=Mathf.CeilToInt((radius+1.2f)/cell);
            int cx=Mathf.FloorToInt(x/cell),cz=Mathf.FloorToInt(z/cell);
            for(int dz=-reach;dz<=reach;dz++)for(int dx=-reach;dx<=reach;dx++){
                int row=c.LayoutRows.Length-1-cz-dz,col=cx+dx;
                if(row>=0&&row<c.LayoutRows.Length&&col>=0&&col<c.LayoutRows[row].Length&&c.LayoutRows[row][col]=='#'&&new Vector2(dx*cell,dz*cell).magnitude<radius*.70f)return true;
            }
            return false;
        }
        void BuildShoreBanks(float y) {
            for(float z=3;z<config.Height-3;z+=1.25f)for(float x=1.25f;x<width*.5f-1;x+=1.25f){
                float n=Noise(x,z),r=1.28f+n*.27f;
                if(shoreSites.Count>=14||RockBanks>=38||n<.45f||!FitsShore(config,x,z,r)||!Clear(x,z,r+.55f)||!ClearsFlight(config,x,z,r))continue;
                int seed=Mathf.RoundToInt(x*13+z*7);
                RockBank(new Vector3(x,y,z),r,seed);
                Tree(new Vector3(x,y+.35f,z),r,4.1f+r*.65f,seed,false);
                shoreSites.Add(new Vector4(x,y,z,r+WindEnvelope));
            }
        }
        // Broad shoulders grow from the existing blocked shelves. Complete groups, including
        // their moss crowns, are admitted before drawing; the walkable mask is never edited.
        void BuildRockShelves(MapScenery scenery,float y)
        {
            for(float z=4;z<config.Height-3;z+=3.2f)for(float x=3;x<width*.5f-1;x+=2.7f){
                float n=Noise(x,z);if(n<.33f)continue;
                float px=x+(n-.5f)*.9f,pz=z+(Noise(z,x)-.5f)*.8f,r=.86f+n*.55f;
                if(RockBanks>=38||!FitsMap(config,px,pz,r)||!Clear(px,pz,r))continue;
                bool reserved=false;
                foreach(var landmark in scenery.LandmarkPositions)if(new Vector2(px-landmark.x,pz-landmark.z).magnitude<r+2.25f)reserved=true;
                foreach(var fire in scenery.FirePositions)if(new Vector2(px-fire.x,pz-fire.z).magnitude<r+.65f)reserved=true;
                if(reserved)continue;
                int seed=Mathf.RoundToInt(px*13+pz*7);bool tall=ClearsFlight(config,px,pz,r);
                RockBank(new Vector3(px,y,pz),r,seed,!tall);
                if(tall&&n>.47f)Tree(new Vector3(px,y,pz),r,3.6f+r*.75f,seed,false);
                else Thicket(new Vector3(px,y,pz),Mathf.Min(.68f,r*.58f),seed);
                Site(px,y,pz,r);
            }
        }
        void RockBank(Vector3 root,float radius,int seed,bool low=false)
        {
            RockBanks++;
            // Interlocking, uneven strata with broad blunt tops instead of isolated cones.
            for(int i=0;i<3;i++){
                float turn=seed*.73f+i*2.1f;
                var offset=new Vector3(Mathf.Cos(turn),0,Mathf.Sin(turn))*radius*(i==0?.08f:.42f);
                float r=radius*(i==0?.60f:.43f),height=radius*(i==0?1.35f:.72f+(i%2)*.25f);
                Crag(root+offset,r,low?Mathf.Min(.61f,height):height,seed+i*7);
            }
        }
        void Crag(Vector3 root,float radius,float height,int seed)
        {
            const int sides=7,levels=4;var p=new Vector3[levels,sides];
            for(int level=0;level<levels;level++)for(int i=0;i<sides;i++){
                float angle=i*Mathf.PI*2/sides,variation=.87f+.10f*Mathf.Sin(i*2.3f+seed);
                float spread=radius*variation*(level==0?1:level==1?.96f:level==2?.84f:.65f);
                p[level,i]=root+new Vector3(Mathf.Cos(angle)*spread,height*(level==0?0:level==1?.17f:level==2?.85f:.97f+.03f*Mathf.Sin(i+seed)),Mathf.Sin(angle)*spread);
            }
            for(int i=0;i<sides;i++){
                int next=(i+1)%sides;
                var tone=Tint(.70f+(i%3)*.06f,.79f+(i%3)*.06f,.78f+(i%3)*.055f,0);
                for(int level=0;level<levels-1;level++){
                    int first=rockbanks.V.Count;
                    rockbanks.Quad(p[level,i],p[level+1,i],p[level+1,next],p[level,next],tone*(level==0?.73f:1));
                    // Stone grain follows the vertical face, with no stretched edge texels.
                    for(int v=first;v<rockbanks.V.Count;v++){var q=rockbanks.V[v];rockbanks.UV[v]=new Vector2((q.x+q.z)/2.5f,q.y/2.5f);}
                }
                var top=root+Vector3.up*height;int start=rockbanks.V.Count;
                rockbanks.Quad(p[3,i],top,top,p[3,next],tone*1.08f,Vector3.up);
                for(int v=start;v<rockbanks.V.Count;v++)rockbanks.UV[v]=new Vector2(rockbanks.V[v].x,rockbanks.V[v].z)/2.5f;
                // A soft irregular moss crown sits on the rock itself, never on a build cell.
                var a=Vector3.Lerp(top,p[3,i],.86f)+Vector3.up*.018f;
                var b=Vector3.Lerp(top,p[3,next],.86f)+Vector3.up*.018f;
                bankMoss.Quad(a,top+Vector3.up*.018f,top+Vector3.up*.018f,b,Tint(.51f,.71f,.63f,0),Vector3.up);
            }
        }
    }
}
