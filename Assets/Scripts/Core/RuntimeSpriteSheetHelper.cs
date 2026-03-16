using UnityEngine;
using System.Collections.Generic;

namespace S2RD.Core
{
    public static class RuntimeSpriteSheetHelper
    {
        private struct CellStats
        {
            public int Foreground;
            public int Opaque;
            public int MinX;
            public int MaxX;
            public int MinY;
            public int MaxY;
            public bool TouchLeft;
            public bool TouchRight;
            public bool TouchTop;
            public bool TouchBottom;

            public bool HasForeground => Foreground > 0 && MaxX >= MinX && MaxY >= MinY;
            public int Width => HasForeground ? (MaxX - MinX + 1) : 0;
            public int Height => HasForeground ? (MaxY - MinY + 1) : 0;
            public int Score => (Foreground * 6) + Opaque + (Width * Height);
        }

        public static Sprite CreateBestCharacterSprite(Texture2D source, int cellSize = 128, float pixelsPerUnit = 32f, bool removeChecker = true)
        {
            if (source == null || cellSize <= 0)
                return null;

            if (!source.isReadable)
                return null;

            int cols = Mathf.Max(1, source.width / cellSize);
            int rows = Mathf.Max(1, source.height / cellSize);
            Color32[] allPixels;
            try
            {
                allPixels = source.GetPixels32();
            }
            catch
            {
                return null;
            }

            FindCheckerColors(allPixels, out Color32 bg1, out Color32 bg2);

            int bestCol = 0;
            int bestRow = 0;
            int bestScore = int.MinValue;
            CellStats bestStats = default;

            for (int row = 0; row < rows; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    CellStats stats = AnalyzeCell(allPixels, source.width, source.height, col, row, cellSize, bg1, bg2);
                    if (!stats.HasForeground)
                        continue;

                    if (stats.Width < 36 || stats.Height < 48)
                        continue;

                    int score = stats.Score;
                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestCol = col;
                        bestRow = row;
                        bestStats = stats;
                    }
                }
            }

            Rect rect = BuildExpandedRect(bestCol, bestRow, cols, rows, cellSize, source.height, bestStats);
            return CreateProcessedSprite(source, rect, pixelsPerUnit, removeChecker, bg1, bg2, true);
        }

        public static Sprite[] CreateCharacterFrames(Texture2D source, int cellSize = 128, float pixelsPerUnit = 32f, bool removeChecker = true, bool topHalfOnly = false)
        {
            if (source == null || cellSize <= 0)
                return System.Array.Empty<Sprite>();

            if (!source.isReadable)
                return System.Array.Empty<Sprite>();

            int cols = Mathf.Max(1, source.width / cellSize);
            int rows = Mathf.Max(1, source.height / cellSize);
            Color32[] allPixels;
            try
            {
                allPixels = source.GetPixels32();
            }
            catch
            {
                return System.Array.Empty<Sprite>();
            }
            FindCheckerColors(allPixels, out Color32 bg1, out Color32 bg2);

            List<Sprite> frames = new List<Sprite>();
            HashSet<string> addedRects = new HashSet<string>();
            int maxRow = topHalfOnly ? Mathf.Max(1, rows / 2) : rows;

            for (int row = 0; row < maxRow; row++)
            {
                for (int col = 0; col < cols; col++)
                {
                    CellStats stats = AnalyzeCell(allPixels, source.width, source.height, col, row, cellSize, bg1, bg2);
                    if (!stats.HasForeground)
                        continue;

                    if (stats.Width < 36 || stats.Height < 48)
                        continue;

                    if (stats.Foreground < (cellSize * cellSize) / 8)
                        continue;

                    Rect rect = BuildExpandedRect(col, row, cols, rows, cellSize, source.height, stats);
                    string key = $"{rect.x}_{rect.y}_{rect.width}_{rect.height}";
                    if (!addedRects.Add(key))
                        continue;

                    Sprite sprite = CreateProcessedSprite(source, rect, pixelsPerUnit, removeChecker, bg1, bg2, true);
                    if (sprite != null)
                        frames.Add(sprite);
                }
            }

            return frames.ToArray();
        }

        private static Sprite CreateProcessedSprite(Texture2D source, Rect rect, float ppu, bool removeChecker, Color32 bg1, Color32 bg2, bool trimToForeground)
        {
            int w = Mathf.RoundToInt(rect.width);
            int h = Mathf.RoundToInt(rect.height);
            int x = Mathf.RoundToInt(rect.x);
            int y = Mathf.RoundToInt(rect.y);

            Texture2D cut = new Texture2D(w, h, TextureFormat.RGBA32, false);
            cut.filterMode = FilterMode.Point;
            cut.wrapMode = TextureWrapMode.Clamp;

            Color32[] cell = source.GetPixels32();
            Color32[] dst = new Color32[w * h];
            bool[] likelyBg = new bool[w * h];
            int minX = w;
            int maxX = -1;
            int minY = h;
            int maxY = -1;

            for (int iy = 0; iy < h; iy++)
            {
                int sy = y + iy;
                for (int ix = 0; ix < w; ix++)
                {
                    int sx = x + ix;
                    Color32 c = cell[(sy * source.width) + sx];
                    bool checkerLike = IsCheckerLike(c, bg1, bg2);
                    bool bgTone = IsLikelyBackgroundTone(c);
                    likelyBg[(iy * w) + ix] = checkerLike || bgTone;

                    if (removeChecker && checkerLike)
                        c.a = 0;

                    if (c.a > 20)
                    {
                        if (ix < minX) minX = ix;
                        if (ix > maxX) maxX = ix;
                        if (iy < minY) minY = iy;
                        if (iy > maxY) maxY = iy;
                    }

                    dst[(iy * w) + ix] = c;
                }
            }

            if (removeChecker)
                RemoveBorderConnectedBackground(dst, likelyBg, w, h);

            cut.SetPixels32(dst);
            cut.Apply();

            if (!trimToForeground)
                return Sprite.Create(cut, new Rect(0f, 0f, w, h), new Vector2(0.5f, 0.5f), ppu);

            if (maxX < minX || maxY < minY)
            {
                return null;
            }

            const int padding = 2;
            minX = Mathf.Max(0, minX - padding);
            minY = Mathf.Max(0, minY - padding);
            maxX = Mathf.Min(w - 1, maxX + padding);
            maxY = Mathf.Min(h - 1, maxY + padding);

            int outW = maxX - minX + 1;
            int outH = maxY - minY + 1;
            Texture2D trimmed = new Texture2D(outW, outH, TextureFormat.RGBA32, false);
            trimmed.filterMode = FilterMode.Point;
            trimmed.wrapMode = TextureWrapMode.Clamp;

            Color32[] cropped = new Color32[outW * outH];
            for (int iy = 0; iy < outH; iy++)
            {
                for (int ix = 0; ix < outW; ix++)
                {
                    cropped[(iy * outW) + ix] = dst[((minY + iy) * w) + (minX + ix)];
                }
            }

            trimmed.SetPixels32(cropped);
            trimmed.Apply();
            return Sprite.Create(trimmed, new Rect(0f, 0f, outW, outH), new Vector2(0.5f, 0.5f), ppu);
        }

        private static Rect BuildExpandedRect(int col, int row, int cols, int rows, int cellSize, int texHeight, CellStats stats)
        {
            int minCol = col;
            int maxCol = col;
            int minRow = row;
            int maxRow = row;

            if (stats.TouchLeft && col > 0)
                minCol = col - 1;
            if (stats.TouchRight && col < cols - 1)
                maxCol = col + 1;
            if (stats.TouchTop && row > 0)
                minRow = row - 1;
            if (stats.TouchBottom && row < rows - 1)
                maxRow = row + 1;

            float x = minCol * cellSize;
            float y = texHeight - ((maxRow + 1) * cellSize);
            float w = (maxCol - minCol + 1) * cellSize;
            float h = (maxRow - minRow + 1) * cellSize;
            return new Rect(x, y, w, h);
        }

        private static void RemoveBorderConnectedBackground(Color32[] pixels, bool[] likelyBg, int width, int height)
        {
            if (pixels == null || likelyBg == null || pixels.Length != likelyBg.Length)
                return;

            bool[] visited = new bool[pixels.Length];
            Queue<int> queue = new Queue<int>();

            void EnqueueIfBg(int x, int y)
            {
                int idx = (y * width) + x;
                if (visited[idx])
                    return;

                if (!likelyBg[idx])
                    return;

                visited[idx] = true;
                queue.Enqueue(idx);
            }

            for (int x = 0; x < width; x++)
            {
                EnqueueIfBg(x, 0);
                EnqueueIfBg(x, height - 1);
            }

            for (int y = 0; y < height; y++)
            {
                EnqueueIfBg(0, y);
                EnqueueIfBg(width - 1, y);
            }

            while (queue.Count > 0)
            {
                int idx = queue.Dequeue();
                int x = idx % width;
                int y = idx / width;

                Color32 c = pixels[idx];
                c.a = 0;
                pixels[idx] = c;

                if (x > 0) EnqueueIfBg(x - 1, y);
                if (x < width - 1) EnqueueIfBg(x + 1, y);
                if (y > 0) EnqueueIfBg(x, y - 1);
                if (y < height - 1) EnqueueIfBg(x, y + 1);
            }
        }

        private static CellStats AnalyzeCell(Color32[] pixels, int texWidth, int texHeight, int col, int row, int cellSize, Color32 bg1, Color32 bg2)
        {
            int x0 = col * cellSize;
            int y0 = texHeight - ((row + 1) * cellSize);
            CellStats stats = new CellStats
            {
                Foreground = 0,
                Opaque = 0,
                MinX = cellSize,
                MaxX = -1,
                MinY = cellSize,
                MaxY = -1,
                TouchLeft = false,
                TouchRight = false,
                TouchTop = false,
                TouchBottom = false
            };
            const int edgeMargin = 2;

            for (int y = 0; y < cellSize; y++)
            {
                int sy = y0 + y;
                for (int x = 0; x < cellSize; x++)
                {
                    int sx = x0 + x;
                    Color32 c = pixels[(sy * texWidth) + sx];
                    if (c.a > 15)
                        stats.Opaque++;

                    if (c.a > 15 && !IsCheckerLike(c, bg1, bg2))
                    {
                        stats.Foreground++;
                        if (x < stats.MinX) stats.MinX = x;
                        if (x > stats.MaxX) stats.MaxX = x;
                        if (y < stats.MinY) stats.MinY = y;
                        if (y > stats.MaxY) stats.MaxY = y;

                        if (x <= edgeMargin) stats.TouchLeft = true;
                        if (x >= cellSize - 1 - edgeMargin) stats.TouchRight = true;
                        if (y <= edgeMargin) stats.TouchBottom = true;
                        if (y >= cellSize - 1 - edgeMargin) stats.TouchTop = true;
                    }
                }
            }

            return stats;
        }

        private static void FindCheckerColors(Color32[] pixels, out Color32 c1, out Color32 c2)
        {
            c1 = new Color32(170, 170, 170, 255);
            c2 = new Color32(120, 120, 120, 255);

            int best1 = -1;
            int best2 = -1;
            Color32 col1 = c1;
            Color32 col2 = c2;

            var counts = new System.Collections.Generic.Dictionary<int, int>();
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
                    col2 = col1;
                    best1 = kv.Value;
                    col1 = new Color32((byte)((kv.Key >> 16) & 0xFF), (byte)((kv.Key >> 8) & 0xFF), (byte)(kv.Key & 0xFF), 255);
                }
                else if (kv.Value > best2)
                {
                    best2 = kv.Value;
                    col2 = new Color32((byte)((kv.Key >> 16) & 0xFF), (byte)((kv.Key >> 8) & 0xFF), (byte)(kv.Key & 0xFF), 255);
                }
            }

            c1 = col1;
            c2 = col2;
        }

        private static bool IsCheckerLike(Color32 c, Color32 bg1, Color32 bg2)
        {
            if (!IsNearGray(c))
                return false;

            return ColorDistance(c, bg1) < 24 || ColorDistance(c, bg2) < 24;
        }

        private static bool IsLikelyBackgroundTone(Color32 c)
        {
            if (!IsNearGray(c))
                return false;

            int brightness = (c.r + c.g + c.b) / 3;
            return brightness >= 70 && brightness <= 210;
        }

        private static bool IsNearGray(Color32 c)
        {
            int rg = Mathf.Abs(c.r - c.g);
            int gb = Mathf.Abs(c.g - c.b);
            int br = Mathf.Abs(c.b - c.r);
            return rg < 10 && gb < 10 && br < 10;
        }

        private static int ColorDistance(Color32 a, Color32 b)
        {
            int dr = a.r - b.r;
            int dg = a.g - b.g;
            int db = a.b - b.b;
            return Mathf.Abs(dr) + Mathf.Abs(dg) + Mathf.Abs(db);
        }
    }
}