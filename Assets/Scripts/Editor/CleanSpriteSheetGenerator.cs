#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace S2RD.EditorTools
{
    public static class CleanSpriteSheetGenerator
    {
        private const string HeroSourcePath = "Assets/Sprites/HeroArcher.png";
        private const string MonsterSourcePath = "Assets/Sprites/monster.png";
        private const string HeroOutputPath = "Assets/Resources/HeroArcher_clean.png";
        private const string MonsterOutputPath = "Assets/Resources/monster_clean.png";

        [MenuItem("S2RD/클린 투명 PNG 생성")]
        public static void GenerateCleanSheets()
        {
            GenerateOne(HeroSourcePath, HeroOutputPath);
            GenerateOne(MonsterSourcePath, MonsterOutputPath);
            AssetDatabase.Refresh();
            Debug.Log("클린 투명 PNG 생성 완료: HeroArcher_clean, monster_clean");
        }

        [InitializeOnLoadMethod]
        private static void AutoGenerateOnEditorLoad()
        {
            const string sessionKey = "S2RD.CleanSpriteSheetGenerator.RunOnce";
            if (SessionState.GetBool(sessionKey, false))
                return;

            SessionState.SetBool(sessionKey, true);
            EditorApplication.delayCall += GenerateCleanSheets;
        }

        private static void GenerateOne(string sourcePath, string outputPath)
        {
            Texture2D src = AssetDatabase.LoadAssetAtPath<Texture2D>(sourcePath);
            if (src == null)
            {
                Debug.LogWarning($"소스 이미지 없음: {sourcePath}");
                return;
            }

            EnsureReadable(sourcePath);
            src = AssetDatabase.LoadAssetAtPath<Texture2D>(sourcePath);
            if (src == null)
                return;

            Texture2D clean = RemoveCheckerBackground(src);
            byte[] png = clean.EncodeToPNG();
            if (png == null || png.Length == 0)
                return;

            File.WriteAllBytes(outputPath, png);
            Object.DestroyImmediate(clean);
            AssetDatabase.ImportAsset(outputPath, ImportAssetOptions.ForceSynchronousImport);
            ConfigureAsSprite(outputPath);
        }

        private static void EnsureReadable(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
                return;

            bool changed = false;
            if (!importer.isReadable)
            {
                importer.isReadable = true;
                changed = true;
            }

            if (importer.textureCompression != TextureImporterCompression.Uncompressed)
            {
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                changed = true;
            }

            if (changed)
                importer.SaveAndReimport();
        }

        private static Texture2D RemoveCheckerBackground(Texture2D src)
        {
            int w = src.width;
            int h = src.height;
            Color32[] pixels = src.GetPixels32();
            DetectBgColors(pixels, out Color32 bg1, out Color32 bg2);

            Texture2D outTex = new Texture2D(w, h, TextureFormat.RGBA32, false);
            outTex.filterMode = FilterMode.Point;
            outTex.wrapMode = TextureWrapMode.Clamp;

            Color32[] dst = new Color32[pixels.Length];
            for (int i = 0; i < pixels.Length; i++)
            {
                Color32 c = pixels[i];
                if (IsBg(c, bg1, bg2))
                    c.a = 0;

                dst[i] = c;
            }

            outTex.SetPixels32(dst);
            outTex.Apply();
            return outTex;
        }

        private static void ConfigureAsSprite(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
                return;

            Texture2D tex = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (tex == null)
                return;

            bool changed = false;
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                changed = true;
            }

            if (!importer.alphaIsTransparency)
            {
                importer.alphaIsTransparency = true;
                changed = true;
            }

            if (!Mathf.Approximately(importer.spritePixelsPerUnit, 32f))
            {
                importer.spritePixelsPerUnit = 32f;
                changed = true;
            }

            if (importer.spriteImportMode != SpriteImportMode.Multiple)
            {
                importer.spriteImportMode = SpriteImportMode.Multiple;
                changed = true;
            }

            if (!importer.isReadable)
            {
                importer.isReadable = true;
                changed = true;
            }

            if (importer.filterMode != FilterMode.Point)
            {
                importer.filterMode = FilterMode.Point;
                changed = true;
            }

#pragma warning disable CS0618
            SpriteMetaData[] metas = BuildGrid(tex, Path.GetFileNameWithoutExtension(path));
            importer.spritesheet = metas;
#pragma warning restore CS0618
            changed = true;

            if (changed)
                importer.SaveAndReimport();
        }

        private static SpriteMetaData[] BuildGrid(Texture2D tex, string baseName)
        {
            const int cell = 32;   // 32px 그리드 (게임 코드의 타일 인덱스 기준)
            int cols = Mathf.Max(1, tex.width / cell);
            int rows = Mathf.Max(1, tex.height / cell);
            var list = new System.Collections.Generic.List<SpriteMetaData>();
            int idx = 0;

            for (int row = rows - 1; row >= 0; row--)
            {
                for (int col = 0; col < cols; col++)
                {
                    Rect rect = new Rect(col * cell, row * cell, cell, cell);
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

        private static void DetectBgColors(Color32[] pixels, out Color32 c1, out Color32 c2)
        {
            c1 = new Color32(170, 170, 170, 255);
            c2 = new Color32(120, 120, 120, 255);

            int best1 = -1;
            int best2 = -1;
            Color32 bc1 = c1;
            Color32 bc2 = c2;
            var counts = new System.Collections.Generic.Dictionary<int, int>();

            for (int i = 0; i < pixels.Length; i++)
            {
                Color32 c = pixels[i];
                if (!IsGray(c))
                    continue;

                int key = (c.r << 16) | (c.g << 8) | c.b;
                if (!counts.ContainsKey(key))
                    counts[key] = 0;

                counts[key]++;
            }

            foreach (var kv in counts)
            {
                if (kv.Value > best1)
                {
                    best2 = best1;
                    bc2 = bc1;
                    best1 = kv.Value;
                    bc1 = new Color32((byte)((kv.Key >> 16) & 0xFF), (byte)((kv.Key >> 8) & 0xFF), (byte)(kv.Key & 0xFF), 255);
                }
                else if (kv.Value > best2)
                {
                    best2 = kv.Value;
                    bc2 = new Color32((byte)((kv.Key >> 16) & 0xFF), (byte)((kv.Key >> 8) & 0xFF), (byte)(kv.Key & 0xFF), 255);
                }
            }

            c1 = bc1;
            c2 = bc2;
        }

        private static bool IsBg(Color32 c, Color32 bg1, Color32 bg2)
        {
            if (!IsGray(c))
                return false;

            return Dist(c, bg1) < 28 || Dist(c, bg2) < 28;
        }

        private static bool IsGray(Color32 c)
        {
            int rg = Mathf.Abs(c.r - c.g);
            int gb = Mathf.Abs(c.g - c.b);
            int br = Mathf.Abs(c.b - c.r);
            return rg < 12 && gb < 12 && br < 12;
        }

        private static int Dist(Color32 a, Color32 b)
        {
            return Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b);
        }
    }
}
#endif