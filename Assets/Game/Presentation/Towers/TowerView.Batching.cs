using System.Collections.Generic;
using UnityEngine;
namespace FrostMaze
{
    public sealed partial class TowerView
    {
        // Only direct rigid pieces participate. Weapon transforms and upgrade markers stay independent.
        // Shared per-design meshes belong to the map and are released by ModelMeshes.Dispose.
        void CombineRigidParts(Transform group,string key)
        {
            var materials=new List<Material>();
            var pieces=new List<List<CombineInstance>>();
            foreach(Transform part in group) {
                var filter=part.GetComponent<MeshFilter>();var renderer=part.GetComponent<MeshRenderer>();
                if(filter==null||renderer==null||!renderer.enabled)continue;
                int index=materials.IndexOf(renderer.sharedMaterial);
                if(index<0){index=materials.Count;materials.Add(renderer.sharedMaterial);pieces.Add(new List<CombineInstance>());}
                var matrix=Matrix4x4.TRS(part.localPosition,part.localRotation,part.localScale);
                if(group==transform)matrix=Matrix4x4.Scale(new Vector3(weaponWidth,1,weaponWidth))*matrix;
                pieces[index].Add(new CombineInstance{mesh=filter.sharedMesh,transform=matrix});
                renderer.enabled=false;
            }
            for(int i=0;i<materials.Count;i++) {
                var combined=new GameObject("Combined geometry "+i);combined.transform.SetParent(group,false);
                combined.AddComponent<MeshFilter>().sharedMesh=game.Models.Combine(key+"/"+i,pieces[i]);
                combined.AddComponent<MeshRenderer>().sharedMaterial=materials[i];
            }
        }
    }
}
