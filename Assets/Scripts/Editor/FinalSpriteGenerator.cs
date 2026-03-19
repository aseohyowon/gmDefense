#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace S2RD.EditorTools
{
    public static class FinalSpriteGenerator
    {
        private const string AutoRunKey = "S2RD.FinalSpriteGenerator.AutoRun.v1";
        private const string HeroSource = "Assets/Resources/HeroArcher.png";
        private const string MonsterSource = "Assets/Resources/monster.png";
        private const string HeroOut = "Assets/Resources/HeroArcher_final.png";
        private const string MonsterOut = "Assets/Resources/Monster_final.png";

        [InitializeOnLoadMethod]
        private static void AutoGenerateOnEditorLoad()
        {
            // Run once per editor session unless output is missing/outdated.
            bool ran = SessionState.GetBool(AutoRunKey, false);
            if (ran && !NeedRegenerate())
                return;

            SessionState.SetBool(AutoRunKey, true);
            if (NeedRegenerate())
                GenerateFinalSprites();
        }

        [MenuItem("S2RD/최종 캐릭터 PNG 생성")]
        public static void GenerateFinalSprites()
        {
            GenerateOne(HeroSource, HeroOut);
            GenerateOne(MonsterSource, MonsterOut);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("최종 단일 캐릭터 PNG 생성 완료: HeroArcher_final, Monster_final");
        }

        private static bool NeedRegenerate()
        {
            return IsMissingOrOlder(HeroSource, HeroOut) || IsMissingOrOlder(MonsterSource, MonsterOut);
        }

        private static bool IsMissingOrOlder(string srcRelPath, string outRelPath)
        {
            string projectRoot = Directory.GetParent(Application.dataPath)?.FullName ?? string.Empty;
            if (string.IsNullOrEmpty(projectRoot))
                return true;

            string src = Path.Combine(projectRoot, srcRelPath.Replace('/', Path.DirectorySeparatorChar));
            string dst = Path.Combine(projectRoot, outRelPath.Replace('/', Path.DirectorySeparatorChar));

            if (!File.Exists(src) || !File.Exists(dst))
                return true;

            return File.GetLastWriteTimeUtc(src) > File.GetLastWriteTimeUtc(dst);
        }

        private static void GenerateOne(string srcPath, string outPath)
        {
            Texture2D src = AssetDatabase.LoadAssetAtPath<Texture2D>(srcPath);
            if (src == null)
            {
                Debug.LogWarning($"최종 스프라이트 생성 실패: 원본을 찾을 수 없습니다. path={srcPath}");
                return;
            }

            EnsureReadable(srcPath);
            src = AssetDatabase.LoadAssetAtPath<Texture2D>(srcPath);
            if (src == null || !src.isReadable)
            {
                Debug.LogWarning($"최종 스프라이트 생성 실패: 텍스처가 읽기 불가입니다. path={srcPath}");
                return;
            }

            Color32[] pixels = src.GetPixels32();
            int w = src.width;
            int h = src.height;

            DetectBgColors(pixels, out Color32 bg1, out Color32 bg2);
            bool[] bgMask = BuildBackgroundMask(pixels, w, h, bg1, bg2);
            FloodFillFromEdges(bgMask, pixels, w, h);

            Color32[] cleaned = new Color32[pixels.Length];
            for (int i = 0; i < pixels.Length; i++)
            {
                Color32 c = pixels[i];
                if (bgMask[i])
                    c.a = 0;
                cleaned[i] = c;
            }

            RectInt bounds = FindLargestOpaqueComponent(cleaned, w, h);
            if (bounds.width <= 0 || bounds.height <= 0)
            {
                Debug.LogWarning($"최종 스프라이트 생성 실패: 불투명 픽셀 영역을 찾지 못했습니다. path={srcPath}");
                return;
            }

            const int pad = 2;
            int x0 = Mathf.Max(0, bounds.xMin - pad);
            int y0 = Mathf.Max(0, bounds.yMin - pad);
            int x1 = Mathf.Min(w - 1, bounds.xMax + pad);
            int y1 = Mathf.Min(h - 1, bounds.yMax + pad);
            int outW = x1 - x0 + 1;
            int outH = y1 - y0 + 1;

            Texture2D outTex = new Texture2D(outW, outH, TextureFormat.RGBA32, false);
            outTex.filterMode = FilterMode.Point;
            outTex.wrapMode = TextureWrapMode.Clamp;

            Color32[] outPixels = new Color32[outW * outH];
            for (int y = 0; y < outH; y++)
            {
                for (int x = 0; x < outW; x++)
                    outPixels[(y * outW) + x] = cleaned[((y0 + y) * w) + (x0 + x)];
            }

            outTex.SetPixels32(outPixels);
            outTex.Apply();
            byte[] png = outTex.EncodeToPNG();
            Object.DestroyImmediate(outTex);

            if (png == null || png.Length == 0)
            {
                Debug.LogWarning($"최종 스프라이트 생성 실패: PNG 인코딩 결과가 비어 있습니다. path={outPath}");
                return;
            }

            File.WriteAllBytes(outPath, png);
            AssetDatabase.ImportAsset(outPath, ImportAssetOptions.ForceSynchronousImport);
            ConfigureOutputSprite(outPath);
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

        private static void ConfigureOutputSprite(string path)
        {
            TextureImporter importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null)
                return;

            bool changed = false;
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                changed = true;
            }

            if (importer.spriteImportMode != SpriteImportMode.Single)
            {
                importer.spriteImportMode = SpriteImportMode.Single;
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

            if (changed)
                importer.SaveAndReimport();
        }

        private static bool[] BuildBackgroundMask(Color32[] pixels, int w, int h, Color32 bg1, Color32 bg2)
        {
            bool[] mask = new bool[pixels.Length];
            for (int i = 0; i < pixels.Length; i++)
            {
                Color32 c = pixels[i];
                mask[i] = IsNearGray(c) && (ColorDist(c, bg1) < 28 || ColorDist(c, bg2) < 28);
            }

            return mask;
        }

        private static void FloodFillFromEdges(bool[] bgMask, Color32[] pixels, int w, int h)
        {
            bool[] visited = new bool[bgMask.Length];
            Queue<int> q = new Queue<int>();

            void Enqueue(int x, int y)
            {
                int idx = (y * w) + x;
                if (visited[idx])
                    return;
                if (!bgMask[idx])
                    return;
                visited[idx] = true;
                q.Enqueue(idx);
            }

            for (int x = 0; x < w; x++)
            {
                Enqueue(x, 0);
                Enqueue(x, h - 1);
            }

            for (int y = 0; y < h; y++)
            {
                Enqueue(0, y);
                Enqueue(w - 1, y);
            }

            while (q.Count > 0)
            {
                int idx = q.Dequeue();
                int x = idx % w;
                int y = idx / w;

                // keep as background only if connected to the border
                bgMask[idx] = true;

                if (x > 0) Enqueue(x - 1, y);
                if (x < w - 1) Enqueue(x + 1, y);
                if (y > 0) Enqueue(x, y - 1);
                if (y < h - 1) Enqueue(x, y + 1);
            }

            // Any unvisited candidate is likely internal detail; keep it.
            for (int i = 0; i < bgMask.Length; i++)
            {
                if (!visited[i])
                    bgMask[i] = false;
            }
        }

        private static RectInt FindLargestOpaqueComponent(Color32[] pixels, int w, int h)
        {
            bool[] visited = new bool[pixels.Length];
            RectInt best = new RectInt(0, 0, 0, 0);
            int bestCount = 0;

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    int start = (y * w) + x;
                    if (visited[start] || pixels[start].a <= 25)
                        continue;

                    Queue<int> q = new Queue<int>();
                    q.Enqueue(start);
                    visited[start] = true;

                    int count = 0;
                    int minX = x, maxX = x, minY = y, maxY = y;

                    while (q.Count > 0)
                    {
                        int idx = q.Dequeue();
                        int cx = idx % w;
                        int cy = idx / w;
                        count++;

                        if (cx < minX) minX = cx;
                        if (cx > maxX) maxX = cx;
                        if (cy < minY) minY = cy;
                        if (cy > maxY) maxY = cy;

                        TryVisit(cx - 1, cy);
                        TryVisit(cx + 1, cy);
                        TryVisit(cx, cy - 1);
                        TryVisit(cx, cy + 1);

                        void TryVisit(int nx, int ny)
                        {
                            if (nx < 0 || ny < 0 || nx >= w || ny >= h)
                                return;
                            int nidx = (ny * w) + nx;
                            if (visited[nidx] || pixels[nidx].a <= 25)
                                return;
                            visited[nidx] = true;
                            q.Enqueue(nidx);
                        }
                    }

                    if (count > bestCount)
                    {
                        bestCount = count;
                        best = new RectInt(minX, minY, (maxX - minX) + 1, (maxY - minY) + 1);
                    }
                }
            }

            return best;
        }

        private static void DetectBgColors(Color32[] pixels, out Color32 c1, out Color32 c2)
        {
            c1 = new Color32(170, 170, 170, 255);
            c2 = new Color32(120, 120, 120, 255);
            int best1 = -1;
            int best2 = -1;
            var counts = new Dictionary<int, int>();

            for (int i = 0; i < pixels.Length; i++)
            {
                Color32 c = pixels[i];
                if (!IsNearGray(c))
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
                    c2 = c1;
                    best1 = kv.Value;
                    c1 = new Color32((byte)((kv.Key >> 16) & 0xFF), (byte)((kv.Key >> 8) & 0xFF), (byte)(kv.Key & 0xFF), 255);
                }
                else if (kv.Value > best2)
                {
                    best2 = kv.Value;
                    c2 = new Color32((byte)((kv.Key >> 16) & 0xFF), (byte)((kv.Key >> 8) & 0xFF), (byte)(kv.Key & 0xFF), 255);
                }
            }
        }

        private static bool IsNearGray(Color32 c)
        {
            int rg = Mathf.Abs(c.r - c.g);
            int gb = Mathf.Abs(c.g - c.b);
            int br = Mathf.Abs(c.b - c.r);
            return rg < 12 && gb < 12 && br < 12;
        }

        private static int ColorDist(Color32 a, Color32 b)
        {
            return Mathf.Abs(a.r - b.r) + Mathf.Abs(a.g - b.g) + Mathf.Abs(a.b - b.b);
        }
    }
}
#endif