using System.Collections.Generic;
using UnityEngine;
namespace FrostMaze
{
    // Original scenery is generated separately from the authoritative navigation mask.
    public sealed partial class MapScenery : MonoBehaviour
    {
        readonly List<Mesh> meshes=new List<Mesh>();
        Texture2D groundTexture,capTexture,wallTexture,waterTexture;
        sealed class Batch {
            public readonly List<Vector3> V=new List<Vector3>();public readonly List<int> T=new List<int>();
            public void Quad(Vector3 a,Vector3 b,Vector3 c,Vector3 d){int n=V.Count;V.AddRange(new[]{a,b,c,d});T.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});}
            public void Box(float x,float z,float w,float d,float bottom,float top){
                var a=new Vector3(x,bottom,z);var b=new Vector3(x+w,bottom,z);var c=new Vector3(x+w,bottom,z+d);var e=new Vector3(x,bottom,z+d);var u=Vector3.up*(top-bottom);
                Quad(a+u,e+u,c+u,b+u);Quad(a,b,b+u,a+u);Quad(b,c,c+u,b+u);Quad(c,e,e+u,c+u);Quad(e,a,a+u,e+u);
            }
            public void Peak(float x,float z,float radius,float bottom,float height) {
                for(int i=0;i<6;i++){float a=i*Mathf.PI/3,b=(i+1)*Mathf.PI/3;int n=V.Count;V.Add(new Vector3(x,bottom+height,z));V.Add(new Vector3(x+Mathf.Cos(b)*radius,bottom,z+Mathf.Sin(b)*radius));V.Add(new Vector3(x+Mathf.Cos(a)*radius,bottom,z+Mathf.Sin(a)*radius));T.AddRange(new[]{n,n+1,n+2});}
            }
        }
        public void Build(Prototype game) {
            var c=game.World.Config;bool ice=c.Theme!="iron";var batches=new Batch[30];for(int i=0;i<batches.Length;i++)batches[i]=new Batch();
            // Draw the source cells as one continuous surface: only exposed edges receive bevels.
            bool Solid(int row,int col) {
                if(row<0||row>=c.LayoutRows.Length||col<0||col>=c.LayoutRows[row].Length)return false;
                char k=c.LayoutRows[row][col];return c.WalkableSymbols.IndexOf(k)<0&&k!='D'&&k!='W'&&k!='p';
            }
            for(int row=0;row<c.LayoutRows.Length;row++)for(int col=0;col<c.LayoutRows[row].Length;col++) {
                char k=c.LayoutRows[row][col];float cell=c.LayoutCellSize,x=col*cell,z=(c.LayoutRows.Length-row-1)*cell;
                if(c.WalkableSymbols.IndexOf(k)>=0) {
                    // Broad, quiet color patches replace the tiny checkerboard. No surface colliders.
                    int tone=0;
                    batches[6+tone].Quad(new Vector3(x,-.006f,z),new Vector3(x,-.006f,z+cell),new Vector3(x+cell,-.006f,z+cell),new Vector3(x+cell,-.006f,z));
                    continue;
                }
                if(!Solid(row,col)){batches[2].Box(x,z,cell,cell,-.04f,.025f);continue;}
                float h=ice?.72f:.6f,bevel=cell*.18f;
                float l=Solid(row,col-1)?0:bevel,rr=Solid(row,col+1)?0:bevel;
                float down=Solid(row+1,col)?0:bevel,up=Solid(row-1,col)?0:bevel;
                var a0=new Vector3(x+l,h,z+down);var b0=new Vector3(x+l,h,z+cell-up);
                var c0=new Vector3(x+cell-rr,h,z+cell-up);var d0=new Vector3(x+cell-rr,h,z+down);
                batches[1].Quad(a0,b0,c0,d0);
                if(l>0){Cliff(batches[0],new Vector3(x,0,z),new Vector3(x,0,z+cell),b0,a0);batches[19].Box(x+l+.025f,z+down+.025f,.045f,cell-down-up-.05f,h,h+.022f);}
                if(rr>0){Cliff(batches[0],new Vector3(x+cell,0,z+cell),new Vector3(x+cell,0,z),d0,c0);batches[19].Box(x+cell-rr-.07f,z+down+.025f,.045f,cell-down-up-.05f,h,h+.022f);}
                if(down>0){Cliff(batches[0],new Vector3(x+cell,0,z),new Vector3(x,0,z),a0,d0);batches[19].Box(x+l+.025f,z+down+.025f,cell-l-rr-.05f,.045f,h,h+.022f);}
                if(up>0){Cliff(batches[0],new Vector3(x,0,z+cell),new Vector3(x+cell,0,z+cell),c0,b0);batches[19].Box(x+l+.025f,z+cell-up-.07f,cell-l-rr-.05f,.045f,h,h+.022f);}
                // Modest faceted rocks stay within blocked cells, away from buildable ground.
                if(ice&&k=='#'&&col%5==0&&row%5==0&&l+rr+down+up==0) {
                    batches[11].Peak(x+cell*.5f,z+cell*.5f,cell*.4f,h,.5f);
                    batches[1].Peak(x+cell*.5f,z+cell*.5f,cell*.27f,h+.2f,.32f);
                }
            }
            BuildComposition(batches,c,ice);
            // Tall landmarks sit wholly on blocked source cells. Never reserve or cover a build cell.
            // Keep a small gap to the source-cell edge even on Ironfold's half-unit mask.
            void Beacon(FrostMaze.Simulation.V2 origin,int side,bool exit) {
                if(!TryLandmarkAnchor(c,origin,side,out var anchor))return;
                float x=anchor.X,z=anchor.Y,h=ice?.72f:.6f;
                if(exit) {
                    batches[12].Box(x-.22f,z-.22f,.44f,.44f,h,h+1.9f);
                    batches[13].Box(x-.12f,z-.23f,.24f,.46f,h+.5f,h+1.6f);
                    batches[13].Peak(x,z,.23f,h+1.9f,.35f);
                } else {
                    batches[12].Box(x-.15f,z-.15f,.3f,.3f,h,h+1.2f);
                    batches[13].Peak(x,z,.23f,h+1.2f,.4f);
                }
            }
            foreach(var lane in c.Lanes)for(int side=-1;side<=1;side+=2)Beacon(lane.Spawn,side,false);
            var exit=c.GroundRoute[c.GroundRoute.Length-1];
            for(int side=-1;side<=1;side+=2)Beacon(exit,side,true);
            BuildFlora(batches,c,ice);
            sceneryGame=game;
            Color[] colors=ice?new[]{new Color(.22f,.34f,.36f),new Color(.65f,.74f,.79f),new Color(.045f,.13f,.17f),new Color(.27f,.29f,.25f),new Color(.11f,.25f,.22f),new Color(.34f,.72f,.63f)}:new[]{new Color(.19f,.22f,.27f),new Color(.36f,.4f,.43f),new Color(.045f,.065f,.09f),new Color(.27f,.25f,.22f),new Color(.2f,.26f,.29f),new Color(.83f,.55f,.25f)};
            var palette=new List<Color>(colors);
            for(int i=0;i<5;i++)palette.Add(Color.Lerp(ice?new Color(.35f,.46f,.46f):new Color(.19f,.25f,.29f),ice?new Color(.43f,.54f,.52f):new Color(.24f,.3f,.34f),i/4f));
            palette.Add(new Color(.31f,.39f,.39f));
            palette.Add(colors[3]);palette.Add(colors[5]);
            palette.Add(ice?new Color(.12f,.34f,.32f):new Color(.3f,.22f,.16f));
            palette.Add(ice?new Color(.56f,.69f,.79f):new Color(.69f,.4f,.17f));
            palette.Add(ice?new Color(.24f,.34f,.38f):new Color(.22f,.23f,.25f));
            palette.Add(ice?new Color(.95f,.43f,.13f):new Color(1,.39f,.075f));
            palette.Add(ice?new Color(1,.82f,.43f):new Color(1,.85f,.36f));
            palette.Add(ice?new Color(.54f,.74f,.79f):new Color(.43f,.3f,.19f));
            palette.Add(ice?new Color(.30f,.34f,.24f):new Color(.22f,.32f,.18f));
            palette.Add(ice?new Color(.54f,.59f,.46f):new Color(.46f,.48f,.28f));
            palette.Add(ice?new Color(.23f,.32f,.35f):new Color(.28f,.30f,.27f));
            palette.Add(ice?new Color(.49f,.62f,.66f):new Color(.46f,.36f,.23f));
            palette.Add(ice?new Color(.43f,.53f,.53f):new Color(.24f,.30f,.21f)); // layered banks
            palette.Add(ice?new Color(.26f,.36f,.39f):new Color(.28f,.29f,.25f)); // weathered stone
            palette.Add(ice?new Color(.28f,.22f,.17f):new Color(.37f,.25f,.15f)); // timber/copper
            palette.Add(ice?new Color(.12f,.25f,.23f):new Color(.17f,.29f,.18f)); // canopy
            palette.Add(ice?new Color(.60f,.68f,.67f):new Color(.34f,.42f,.27f)); // crowns
            palette.Add(ice?new Color(.34f,.59f,.64f):new Color(.85f,.34f,.06f)); // landmark heart
            foreach(var batch in batches)MirroredGeometry.Apply(batch.V,batch.T,c.Width);
            MirroredGeometry.Points(fireAnchors,c.Width);MirroredGeometry.Points(landmarks,c.Width);Braziers=fireAnchors.Count;
            PaintTerrain(c,ice);
            for(int i=0;i<batches.Length;i++) {
                var b=batches[i];if(b.V.Count==0)continue;var mesh=new Mesh{name="Original terrain batch "+i,indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};mesh.SetVertices(b.V);mesh.SetTriangles(b.T,0);mesh.RecalculateNormals();mesh.RecalculateBounds();meshes.Add(mesh);
                if(i>=24&&i<=28){var uv=new List<Vector2>();foreach(var vertex in b.V)uv.Add(new Vector2((Mathf.Min(vertex.x,c.Width-vertex.x)+vertex.z)/4,vertex.y/3));mesh.SetUVs(0,uv);}
                if(i==0){var uv=new List<Vector2>();foreach(var vertex in b.V)uv.Add(new Vector2((Mathf.Min(vertex.x,c.Width-vertex.x)+vertex.z)/4,vertex.y/(ice?.72f:.6f)));mesh.SetUVs(0,uv);}
                if(i==6||i==1||i==2){var uv=new List<Vector2>();foreach(var vertex in b.V)uv.Add(new Vector2(Mathf.Min(vertex.x,c.Width-vertex.x)/c.Width,vertex.z/c.Height));mesh.SetUVs(0,uv);}
                var obj=new GameObject("Scenery "+i);obj.layer=30;obj.transform.SetParent(transform,false);obj.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=obj.AddComponent<MeshRenderer>();renderer.sharedMaterial=game.MakeMaterial(i==0||i==1||i==6?Color.white:palette[i],i==13||i==17||i==18);
                if(i==17||i==18)RememberFlame(mesh);
                if(i==27)RememberCanopy(mesh);
                if(i==17||i==18)renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;
                if(i==2){renderer.sharedMaterial.color=Color.white;renderer.sharedMaterial.mainTexture=waterTexture;}
                if(i==0)renderer.sharedMaterial.mainTexture=wallTexture;
                if(i==1)renderer.sharedMaterial.mainTexture=capTexture;
                if(i==6){renderer.sharedMaterial.mainTexture=groundTexture;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;}
            }

        }
        static bool TryLandmarkAnchor(FrostMaze.Simulation.Scenario c,FrostMaze.Simulation.V2 origin,int side,out FrostMaze.Simulation.V2 anchor)
        {
            anchor=default;float best=256;bool found=false;float cell=c.LayoutCellSize;
            for(int row=0;row<c.LayoutRows.Length;row++)for(int col=0;col<c.LayoutRows[row].Length;col++) {
                char k=c.LayoutRows[row][col];
                if(c.WalkableSymbols.IndexOf(k)>=0||k=='D'||k=='W'||k=='p')continue;
                var point=new FrostMaze.Simulation.V2((col+.5f)*cell,(c.LayoutRows.Length-row-.5f)*cell);
                if((point.X-origin.X)*side<.75f)continue;
                float distance=(point-origin).LengthSquared;
                if(distance>=best||cell<.5f)continue;
                best=distance;anchor=point;found=true;
            }
            return found;
        }
        void OnDestroy(){if(fireTexture!=null)Destroy(fireTexture);foreach(var mesh in meshes)Destroy(mesh);if(groundTexture!=null)Destroy(groundTexture);if(capTexture!=null)Destroy(capTexture);if(wallTexture!=null)Destroy(wallTexture);if(waterTexture!=null)Destroy(waterTexture);}
    }
}
