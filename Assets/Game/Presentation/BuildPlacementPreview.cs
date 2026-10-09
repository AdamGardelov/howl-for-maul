using System.Collections.Generic;
using FrostMaze.Simulation;
using UnityEngine;
namespace FrostMaze
{
    // A static presentation copy only. No simulation ID, occupied cell, targeting, shot,
    // ownership or order is created until the existing paid build flow completes.
    public sealed class BuildPlacementPreview : MonoBehaviour
    {
        readonly Dictionary<int,GameObject> models=new Dictionary<int,GameObject>();
        readonly ModelPreviewMaterials materials=new ModelPreviewMaterials(.76f);
        static readonly Color Ready=new Color(.37f,.91f,.73f,.22f),Blocked=new Color(1,.24f,.16f,.62f);
        Prototype game;
        public GameObject ActiveModel {get;private set;}
        public void Initialize(Prototype prototype){game=prototype;}
        void LateUpdate(){Refresh();}
        public void Refresh()
        {
            if(game==null||game.World==null||!game.HasHover||game.SetupOpen||game.MenuOpen||game.LobbyOpen||
                game.ChatCapturesInput||game.ResultOpen||game.World.Finished||game.MoveMode||game.SellMode||
                game.World.Grid.At(game.Hover.X,game.Hover.Y)!=null){Hide();return;}
            Show(game.World.SelectedDesign,game.Hover,game.HoverBuildValid);
        }
        public void Show(int design,V2 origin,bool valid)
        {
            if(!models.TryGetValue(design,out var model)) {
                var definition=game.World.Config.Catalog[design];int faction=0;
                for(int i=0;i<game.World.Config.Factions.Length;i++)
                    if(System.Array.IndexOf(game.World.Config.Factions[i].Designs,design)>=0)faction=i;
                model=new GameObject("Construction model "+definition.Name);model.transform.SetParent(transform,false);
                var tower=new Tower{Design=design,Spec=definition.Spec,Health=definition.Spec.Health,Name=definition.Name};
                model.AddComponent<TowerView>().Initialize(game,tower,definition,faction,true);
                materials.Apply(model);models.Add(design,model);
            }
            if(ActiveModel!=model){Hide();ActiveModel=model;model.SetActive(true);}
            var spec=game.World.Config.Catalog[design].Spec;
            model.transform.position=new Vector3(origin.X+spec.Width*.5f,.055f,origin.Y+spec.Height*.5f);
            materials.Tint(valid?Ready:Blocked);
        }
        public void Hide(){if(ActiveModel!=null)ActiveModel.SetActive(false);ActiveModel=null;}
        void OnDisable(){Hide();}
        void OnDestroy(){materials.Dispose();}
    }
}
