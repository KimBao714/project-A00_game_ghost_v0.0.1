using UnityEditor;
using UnityEngine;

namespace GhostRiver.Editor
{
    internal sealed class GhostRiverPixelArtImporter : AssetPostprocessor
    {
        private const string ArtFolder = "Assets/GhostRiver/Resources/GhostRiverArt/";

        private void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(ArtFolder, System.StringComparison.Ordinal))
                return;

            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = 100f;
            SetFullRectMesh(importer);
            importer.filterMode = FilterMode.Point;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.alphaIsTransparency = true;
            importer.isReadable = true;
            importer.wrapMode = TextureWrapMode.Clamp;
        }

        [InitializeOnLoadMethod]
        private static void ConfigureExistingPixelArt()
        {
            EditorApplication.delayCall += ReimportPixelArt;
        }

        private static void ReimportPixelArt()
        {
            var assets = AssetDatabase.FindAssets("t:Texture2D", new[] { ArtFolder.TrimEnd('/') });
            for (var i = 0; i < assets.Length; i++)
            {
                var path = AssetDatabase.GUIDToAssetPath(assets[i]);
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer == null)
                    continue;

                if (importer.textureType == TextureImporterType.Sprite &&
                    importer.spritePixelsPerUnit == 100f &&
                    HasFullRectMesh(importer) &&
                    importer.filterMode == FilterMode.Point &&
                    !importer.mipmapEnabled &&
                    importer.textureCompression == TextureImporterCompression.Uncompressed &&
                    importer.alphaIsTransparency &&
                    importer.isReadable)
                    continue;

                importer.textureType = TextureImporterType.Sprite;
                importer.spritePixelsPerUnit = 100f;
                SetFullRectMesh(importer);
                importer.filterMode = FilterMode.Point;
                importer.mipmapEnabled = false;
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                importer.alphaIsTransparency = true;
                importer.isReadable = true;
                importer.wrapMode = TextureWrapMode.Clamp;
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            }
        }

        private static bool HasFullRectMesh(TextureImporter importer)
        {
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            return settings.spriteMeshType == SpriteMeshType.FullRect;
        }

        private static void SetFullRectMesh(TextureImporter importer)
        {
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(settings);
        }
    }
}
