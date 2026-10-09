using UnityEngine;
namespace FrostMaze
{
    public sealed partial class WorldBackdrop
    {
        void ApplyMeadowDetail(Material material,bool ice)
        {
            if(ice)return;
            var template=Resources.Load<Material>("World/HearthMeadowDetail");
            if(template==null)return;
            var paint=material.mainTexture;material.CopyPropertiesFromMaterial(template);material.mainTexture=paint;
            // Coarse baked trails remain intact; repeating brushwork supplies close-view detail.
            material.SetTextureScale("_DetailAlbedoMap",new Vector2((mirrorWidth*.5f+128)/7,(exteriorHeight+256)/7));
        }
    }
}
