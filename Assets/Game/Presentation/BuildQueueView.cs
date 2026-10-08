using System.Collections.Generic;
using UnityEngine;
using FrostMaze.Simulation;
namespace FrostMaze
{
    // Presentation only: queued footprints never become colliders or reserve money.
    public sealed class BuildQueueView : MonoBehaviour
    {
        public readonly struct Footprint
        {
            public readonly int Player, Number, Design;
            public readonly V2 Origin;
            public readonly TowerSpec Spec;
            public readonly string Label;
            public Footprint(int player,int number,int design,V2 origin,TowerSpec spec,bool team) {
                Player=player;Number=number;Design=design;Origin=origin;Spec=spec;
                Label=team?"P"+(player+1)+" · "+number:number.ToString();
            }
        }
        readonly List<Footprint> orders=new List<Footprint>();
        public IReadOnlyList<Footprint> Orders=>orders;
        Prototype game;World observed;
        readonly Mesh[] meshes=new Mesh[2];
        readonly GameObject[] surfaces=new GameObject[2];
        readonly List<Vector3> vertices=new List<Vector3>();
        readonly List<int> indices=new List<int>();
        public void Initialize(Prototype prototype) {
            game=prototype;
            for(int i=0;i<2;i++) {
                surfaces[i]=new GameObject(i==0?"Current build footprints":"Queued build footprints");surfaces[i].transform.SetParent(transform,false);
                meshes[i]=new Mesh{name=surfaces[i].name};meshes[i].indexFormat=UnityEngine.Rendering.IndexFormat.UInt32;
                surfaces[i].AddComponent<MeshFilter>().sharedMesh=meshes[i];
                var renderer=surfaces[i].AddComponent<MeshRenderer>();renderer.sharedMaterial=game.MakeMaterial(i==0?new Color(1,.76f,.25f):new Color(.25f,.9f,.8f),true);
                renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;
            }
        }
        void LateUpdate()=>Refresh();
        public void Refresh() {
            if(game==null||game.World==null)return;
            var w=game.World;int at=0;bool changed=observed!=w;observed=w;
            for(int player=0;player<w.Players.Length;player++) {
                var state=w.Players[player];if(!state.HasBuildOrder)continue;int number=1;
                Add(player,number++,state.OrderedDesign,state.BuildOrder);
                foreach(var task in state.Queue)Add(player,number++,task.Design,new V2(task.X,task.Y));
            }
            if(at<orders.Count){orders.RemoveRange(at,orders.Count-at);changed=true;}
            if(changed)for(int group=0;group<2;group++) {
                vertices.Clear();indices.Clear();
                foreach(var order in orders)if((order.Number==1?0:1)==group) {
                    float x=order.Origin.X,y=order.Origin.Y,width=order.Spec.Width,height=order.Spec.Height;
                    Strip(x,y,width,.045f);Strip(x,y+height-.045f,width,.045f);
                    Strip(x,y,.045f,height);Strip(x+width-.045f,y,.045f,height);
                }
                meshes[group].Clear();meshes[group].SetVertices(vertices);meshes[group].SetTriangles(indices,0);meshes[group].RecalculateBounds();
            }
            foreach(var surface in surfaces)surface.SetActive(!game.SetupOpen&&!w.Finished);
            void Add(int player,int number,int design,V2 origin) {
                var spec=w.Config.Catalog.Length==0?w.Config.Tower:w.Config.Catalog[design].Spec;
                if(at>=orders.Count){orders.Add(new Footprint(player,number,design,origin,spec,w.Players.Length>1));changed=true;}
                else {var old=orders[at];if(old.Player!=player||old.Number!=number||old.Design!=design||old.Origin.X!=origin.X||old.Origin.Y!=origin.Y||old.Spec!=spec||changed) {
                    orders[at]=new Footprint(player,number,design,origin,spec,w.Players.Length>1);changed=true;
                }}
                at++;
            }
        }
        void Strip(float x,float y,float width,float height) {
            int n=vertices.Count;vertices.Add(new Vector3(x,.075f,y));vertices.Add(new Vector3(x,.075f,y+height));
            vertices.Add(new Vector3(x+width,.075f,y+height));vertices.Add(new Vector3(x+width,.075f,y));
            indices.Add(n);indices.Add(n+1);indices.Add(n+2);indices.Add(n);indices.Add(n+2);indices.Add(n+3);
        }
        void OnDestroy(){foreach(var mesh in meshes)if(mesh!=null)Destroy(mesh);}
    }
}
