using UnityEditor;
using UnityEngine;
namespace FrostMaze.Editor
{
    public sealed class LivingWorldAssets : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            bool leaf=assetPath.EndsWith("/HearthLeaves.png")||assetPath.EndsWith("/HearthSpruce.png");
            bool meadow=assetPath.EndsWith("/HearthMeadow.png")||assetPath.EndsWith("/HearthWaystone.png");
            if(!leaf&&!meadow)return;
            var importer=(TextureImporter)assetImporter;
            importer.textureType=TextureImporterType.Default;importer.sRGBTexture=true;
            importer.isReadable=meadow;importer.alphaIsTransparency=leaf;importer.mipmapEnabled=true;
            importer.mipMapsPreserveCoverage=leaf;importer.alphaTestReferenceValue=.38f;
            importer.wrapMode=leaf?TextureWrapMode.Clamp:TextureWrapMode.Repeat;
            importer.filterMode=FilterMode.Trilinear;importer.anisoLevel=4;importer.maxTextureSize=1024;
            importer.textureCompression=TextureImporterCompression.CompressedHQ;
        }
    }
}
