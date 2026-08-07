using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace CodexGame.Editor
{
    internal sealed class CodexGameArtImporter : AssetPostprocessor
    {
        private const string PrototypeArtRoot = "Assets/Art/Prototype/";

        private void OnPreprocessTexture()
        {
            string normalizedAssetPath = assetPath.Replace('\\', '/');
            if (!normalizedAssetPath.StartsWith(PrototypeArtRoot, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            TextureImporter importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.textureShape = TextureImporterShape.Texture2D;
            importer.spritePixelsPerUnit = 1f;
            importer.spritePivot = new Vector2(0.5f, 0.5f);
            importer.filterMode = FilterMode.Point;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.mipmapEnabled = false;
            importer.alphaSource = TextureImporterAlphaSource.FromInput;
            importer.alphaIsTransparency = true;
            importer.sRGBTexture = true;
            importer.isReadable = false;
            importer.anisoLevel = 0;
            importer.npotScale = TextureImporterNPOTScale.None;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.crunchedCompression = false;
            importer.maxTextureSize = 8192;

            TextureImporterSettings settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int)SpriteAlignment.Center;
            settings.spritePivot = new Vector2(0.5f, 0.5f);
            importer.SetTextureSettings(settings);

            AsepriteSheet sheet = ReadAsepriteSheet(normalizedAssetPath);
            if (sheet == null || sheet.frames == null || sheet.frames.Length <= 1)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                return;
            }

            List<SpriteMetaData> sprites = new List<SpriteMetaData>(sheet.frames.Length);
            string baseName = Path.GetFileNameWithoutExtension(normalizedAssetPath);
            int sheetHeight = sheet.meta != null && sheet.meta.size != null ? sheet.meta.size.h : 0;

            for (int index = 0; index < sheet.frames.Length; index++)
            {
                AsepriteFrame sourceFrame = sheet.frames[index];
                if (sourceFrame == null || sourceFrame.frame == null ||
                    sourceFrame.frame.w <= 0 || sourceFrame.frame.h <= 0 || sheetHeight <= 0)
                {
                    continue;
                }

                SpriteMetaData sprite = new SpriteMetaData
                {
                    name = baseName + "_" + index.ToString("D3"),
                    rect = new Rect(
                        sourceFrame.frame.x,
                        sheetHeight - sourceFrame.frame.y - sourceFrame.frame.h,
                        sourceFrame.frame.w,
                        sourceFrame.frame.h),
                    alignment = (int)SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f),
                    border = Vector4.zero
                };
                sprites.Add(sprite);
            }

            if (sprites.Count <= 1)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
                return;
            }

            importer.spriteImportMode = SpriteImportMode.Multiple;
#pragma warning disable 0618
            importer.spritesheet = sprites.ToArray();
#pragma warning restore 0618
        }

        private static AsepriteSheet ReadAsepriteSheet(string normalizedAssetPath)
        {
            string projectRoot = Directory.GetParent(UnityEngine.Application.dataPath).FullName;
            string texturePath = Path.Combine(projectRoot, normalizedAssetPath.Replace('/', Path.DirectorySeparatorChar));
            string jsonPath = Path.ChangeExtension(texturePath, ".json");
            if (!File.Exists(jsonPath))
            {
                return null;
            }

            try
            {
                return JsonUtility.FromJson<AsepriteSheet>(File.ReadAllText(jsonPath));
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Unable to read Aseprite frame data for " + normalizedAssetPath + ": " + exception.Message);
                return null;
            }
        }

        [Serializable]
        private sealed class AsepriteSheet
        {
            public AsepriteFrame[] frames;
            public AsepriteMeta meta;
        }

        [Serializable]
        private sealed class AsepriteFrame
        {
            public AsepriteRect frame;
            public int duration;
        }

        [Serializable]
        private sealed class AsepriteRect
        {
            public int x;
            public int y;
            public int w;
            public int h;
        }

        [Serializable]
        private sealed class AsepriteMeta
        {
            public AsepriteSize size;
        }

        [Serializable]
        private sealed class AsepriteSize
        {
            public int w;
            public int h;
        }
    }
}
