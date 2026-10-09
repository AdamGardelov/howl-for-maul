using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace FrostMaze
{
    // Preview materials are separate from the shared in-world palettes. Changing a preview
    // never recolors a paid tower or changes the scene's lights/render settings.
    public sealed class ModelPreviewMaterials : System.IDisposable
    {
        readonly Dictionary<Material,Material> copies=new Dictionary<Material,Material>();
        readonly float opacity;
        Color tint=Color.clear;
        public ModelPreviewMaterials(float opacity=1){this.opacity=opacity;}
        public void Tint(Color color){if(tint==color)return;tint=color;foreach(var copy in copies.Values)copy.SetColor("_Tint",tint);}
        public void Apply(GameObject root)
        {
            foreach(var renderer in root.GetComponentsInChildren<MeshRenderer>(true)) {
                if(!renderer.enabled)continue;
                var source=renderer.sharedMaterial;
                if(!copies.TryGetValue(source,out var copy)) {
                    copy=new Material(Resources.Load<Shader>("ModelPreview")){name="Preview / "+source.name};
                    copy.SetColor("_BaseColor",source.color);
                    copy.SetTexture("_BaseMap",source.mainTexture!=null?source.mainTexture:Texture2D.whiteTexture);
                    copy.SetTextureScale("_BaseMap",source.mainTextureScale);copy.SetTextureOffset("_BaseMap",source.mainTextureOffset);
                    copy.SetFloat("_Unlit",source.shader.name.Contains("Unlit")?1:0);
                    copy.SetFloat("_Opacity",opacity);copy.SetColor("_Tint",tint);
                    copies.Add(source,copy);
                }
                renderer.sharedMaterial=copy;
                renderer.shadowCastingMode=ShadowCastingMode.Off;renderer.receiveShadows=false;
                renderer.lightProbeUsage=LightProbeUsage.Off;renderer.reflectionProbeUsage=ReflectionProbeUsage.Off;
            }
        }
        public void Dispose(){foreach(var copy in copies.Values)if(copy!=null)Object.Destroy(copy);copies.Clear();}
    }
}
