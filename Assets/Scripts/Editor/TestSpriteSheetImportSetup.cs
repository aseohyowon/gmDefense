#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace S2RD.EditorTools
{
    public static class TestSpriteSheetImportSetup
    {
        private const string HeroPath = "Assets/Resources/HeroArcher.png";
        private const string MonsterPath = "Assets/Resources/monster.png";
        private const int CellSize = 128;

        [InitializeOnLoadMethod]
        private static void OnEditorLoad()
        {
            EditorApplication.delayCall += Apply;
        }

        [MenuItem("S2RD/테스트 스프라이트 슬라이스 적용")]
        public static void Apply()
        {
            Configure(HeroPath, "HeroArcher");
            Configure(MonsterPath, "monster");
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void Configure(string path, string baseName)
        {
            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex == null)
                return;

            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
                return;

            bool changed = false;

            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                changed = true;
            }

            if (importer.spriteImportMode != SpriteImportMode.Multiple)
            {
                importer.spriteImportMode = SpriteImportMode.Multiple;
                changed = true;
            }

            if (!Mathf.Approximately(importer.spritePixelsPerUnit, 32f))
            {
                importer.spritePixelsPerUnit = 32f;
                changed = true;
            }

            if (!importer.alphaIsTransparency)
            {
                importer.alphaIsTransparency = true;
                changed = true;
            }

            if (importer.filterMode != FilterMode.Point)
            {
                importer.filterMode = FilterMode.Point;
                changed = true;
            }

            if (!importer.isReadable)
            {
                importer.isReadable = true;
                changed = true;
            }

#pragma warning disable CS0618
            SpriteMetaData[] grid = BuildGrid(tex, baseName);
            importer.spritesheet = grid;
#pragma warning restore CS0618
            changed = true;

            if (changed)
                importer.SaveAndReimport();
        }

        private static SpriteMetaData[] BuildGrid(Texture2D tex, string baseName)
        {
            int cols = Mathf.Max(1, tex.width / CellSize);
            int rows = Mathf.Max(1, tex.height / CellSize);
            List<SpriteMetaData> list = new List<SpriteMetaData>(cols * rows);

            int idx = 0;
            for (int row = rows - 1; row >= 0; row--)
            {
                for (int col = 0; col < cols; col++)
                {
                    Rect rect = new Rect(col * CellSize, row * CellSize, CellSize, CellSize);
                    if (rect.xMax > tex.width || rect.yMax > tex.height)
                        continue;

                    list.Add(new SpriteMetaData
                    {
                        name = $"{baseName}_{idx}",
                        rect = rect,
                        alignment = (int)SpriteAlignment.Center,
                        pivot = new Vector2(0.5f, 0.5f)
                    });
                    idx++;
                }
            }

            return list.ToArray();
        }
    }
}
#endif