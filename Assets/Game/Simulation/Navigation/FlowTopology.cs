using System;
namespace FrostMaze.Simulation
{
    // Destination-independent collision geometry, shared by all fields of one radius.
    // A tower edit dirties only edges near its footprint; terrain survives tower edits.
    internal sealed class FlowTopology
    {
        readonly MazeGrid grid;
        readonly float step,radius;
        readonly int columns,rows;
        readonly V2[] points;
        public readonly bool[] TerrainEdges;
        public readonly int[] Neighbors;
        public readonly float[] EdgeLengths;
        public readonly int[] TowerCounts;
        public long GeometryChecks { get; private set; }
        int terrainVersion=-1,minX,maxX,minY,maxY;
        public FlowTopology(MazeGrid grid,float step,float radius,int columns,int rows)
        {
            this.grid=grid;this.step=step;this.radius=radius;this.columns=columns;this.rows=rows;
            points=new V2[columns*rows];TerrainEdges=new bool[points.Length*8];TowerCounts=new int[TerrainEdges.Length];
            Neighbors=new int[TerrainEdges.Length];EdgeLengths=new float[TerrainEdges.Length];
            for(int i=0;i<points.Length;i++)points[i]=new V2((i%columns+.5f)*step,(i/columns+.5f)*step);
            for(int at=0;at<points.Length;at++) {
                int x=at%columns,y=at/columns,edge=0;
                for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++) {
                    if(dx==0&&dy==0)continue;
                    int index=at*8+edge++,nx=x+dx,ny=y+dy;
                    Neighbors[index]=nx<0||ny<0||nx>=columns||ny>=rows?-1:nx+ny*columns;
                    if(Neighbors[index]>=0)EdgeLengths[index]=V2.Distance(points[at],points[Neighbors[index]]);
                }
            }
            ClearDirty();grid.TowerChanged+=Dirty;
        }
        void ClearDirty(){minX=columns;minY=rows;maxX=maxY=-1;}
        void Dirty(Tower tower)
        {
            // Include either end of a diagonal edge, its swept disc, and a full
            // sample margin. Conservative bounds never alter exact clearance.
            var lo=tower.Center-tower.Half;var hi=tower.Center+tower.Half;
            minX=Math.Min(minX,Math.Max(0,(int)Math.Floor((lo.X-radius)/step)-2));
            minY=Math.Min(minY,Math.Max(0,(int)Math.Floor((lo.Y-radius)/step)-2));
            maxX=Math.Max(maxX,Math.Min(columns-1,(int)Math.Ceiling((hi.X+radius)/step)+2));
            maxY=Math.Max(maxY,Math.Min(rows-1,(int)Math.Ceiling((hi.Y+radius)/step)+2));
        }
        public void Refresh()
        {
            bool terrainChanged=terrainVersion!=grid.TerrainVersion;
            if(terrainChanged){minX=minY=0;maxX=columns-1;maxY=rows-1;}
            for(int y=minY;y<=maxY;y++)for(int x=minX;x<=maxX;x++) {
                int at=x+y*columns,edge=0;var a=points[at];
                for(int dy=-1;dy<=1;dy++)for(int dx=-1;dx<=1;dx++) {
                    if(dx==0&&dy==0)continue;
                    int index=at*8+edge++,nx=x+dx,ny=y+dy;
                    if(nx<0||ny<0||nx>=columns||ny>=rows)continue;
                    var b=points[nx+ny*columns];
                    if(terrainChanged){TerrainEdges[index]=grid.TerrainClear(a,b,radius);GeometryChecks++;}
                    if(TerrainEdges[index]){TowerCounts[index]=grid.TowerIntersections(a,b,radius);GeometryChecks++;}
                }
            }
            terrainVersion=grid.TerrainVersion;ClearDirty();
        }
    }
}
