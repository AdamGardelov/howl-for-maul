using System;
using UnityEngine;
using FrostMaze.Simulation;
namespace FrostMaze
{
    // Permanent terrain only: moving markers and the camera outline are drawn separately.
    public sealed class MinimapTerrain : IDisposable
    {
        public static readonly Color32 Ground=new Color(.4f,.57f,.6f);
        public static readonly Color32 Wall=new Color(.08f,.17f,.2f);
        MazeGrid source;
        int terrainCount;
        float step;
        Texture2D texture;
        public Texture2D Get(MazeGrid grid,float cellSize)
        {
            cellSize=Mathf.Max(.01f,cellSize);
            if(texture!=null&&source==grid&&step==cellSize&&terrainCount==grid.Terrain.Count)return texture;
            Dispose();source=grid;step=cellSize;terrainCount=grid.Terrain.Count;
            int width=Mathf.CeilToInt(grid.Width/cellSize),height=Mathf.CeilToInt(grid.Height/cellSize);
            texture=new Texture2D(width,height,TextureFormat.RGBA32,false,true){name="Permanent minimap terrain",filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp};
            var pixels=new Color32[width*height];for(int i=0;i<pixels.Length;i++)pixels[i]=Ground;
            foreach(var block in grid.Terrain) {
                int x0=Mathf.Clamp(Mathf.FloorToInt(block.X/cellSize),0,width),x1=Mathf.Clamp(Mathf.CeilToInt((block.X+block.Width)/cellSize),0,width);
                int y0=Mathf.Clamp(Mathf.FloorToInt(block.Y/cellSize),0,height),y1=Mathf.Clamp(Mathf.CeilToInt((block.Y+block.Height)/cellSize),0,height);
                for(int y=y0;y<y1;y++)for(int x=x0;x<x1;x++)pixels[y*width+x]=Wall;
            }
            texture.SetPixels32(pixels);texture.Apply(false,false);return texture;
        }
        public void Dispose()
        {
            if(texture!=null){if(Application.isPlaying)UnityEngine.Object.Destroy(texture);else UnityEngine.Object.DestroyImmediate(texture);}
            texture=null;source=null;
        }
    }
}
