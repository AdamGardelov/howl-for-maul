using UnityEditor;
using UnityEngine;
namespace FrostMaze.EditorTools {
    public sealed class MusicImport : AssetPostprocessor {
        void OnPreprocessAudio(){if(!assetPath.Contains("/Resources/Music/"))return;var importer=(AudioImporter)assetImporter;var settings=importer.defaultSampleSettings;settings.loadType=AudioClipLoadType.Streaming;settings.compressionFormat=AudioCompressionFormat.Vorbis;settings.quality=.7f;importer.defaultSampleSettings=settings;}
    }
}
