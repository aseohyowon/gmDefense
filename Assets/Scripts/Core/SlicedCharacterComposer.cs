using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace S2RD.Core
{
    public static class SlicedCharacterComposer
    {
        // 합성 결과 캐시 — 스폰 마다 재합성 방지 (GetPixels32 전체 탐색 비용 토로 제거)
        private static readonly Dictionary<string, Sprite[]> _frameCache = new Dictionary<string, Sprite[]>();
        private static readonly Dictionary<string, Sprite>   _singleCache = new Dictionary<string, Sprite>();

        /// <summary>
        /// 여러 프레임의 타일 이름 배열을 받아 각 프레임을 합성하고 Sprite[] 반환
        /// </summary>
        public static Sprite[] ComposeAnimationFrames(
            string resourceKey, float pixelsPerUnit, string animName,
            params string[][] frameNameGroups)
        {
            if (frameNameGroups == null || frameNameGroups.Length == 0)
                return null;

            // 캐시 히트
            if (_frameCache.TryGetValue(animName, out Sprite[] cached))
                return cached;

            Sprite[] allSprites = Resources.LoadAll<Sprite>(resourceKey);
            if (allSprites == null || allSprites.Length == 0)
                return null;

            var spriteMap = new Dictionary<string, Sprite>(allSprites.Length);
            for (int i = 0; i < allSprites.Length; i++)
            {
                Sprite s = allSprites[i];
                if (s != null && !string.IsNullOrEmpty(s.name))
                    spriteMap[s.name] = s;
            }

            var result = new List<Sprite>(frameNameGroups.Length);
            for (int f = 0; f < frameNameGroups.Length; f++)
            {
                string[] names = frameNameGroups[f];
                if (names == null || names.Length == 0) continue;

                var selected = new List<Sprite>(names.Length);
                for (int n = 0; n < names.Length; n++)
                {
                    Sprite s;
                    if (spriteMap.TryGetValue(names[n], out s))
                        selected.Add(s);
                }
                Sprite frame = ComposeFromSprites(selected.ToArray(), animName + "_" + f, pixelsPerUnit);
                if (frame != null) result.Add(frame);
            }
            Sprite[] frames = result.Count > 0 ? result.ToArray() : null;
            if (frames != null) _frameCache[animName] = frames;
            return frames;
        }

        public static Sprite ComposeFromNamedSprites(string resourceKey, string spriteName, float pixelsPerUnit, params string[] names)
        {
            if (names == null || names.Length == 0)
                return null;

            if (_singleCache.TryGetValue(spriteName, out Sprite cachedSingle))
                return cachedSingle;

            Sprite[] sprites = Resources.LoadAll<Sprite>(resourceKey);
            if (sprites == null || sprites.Length == 0)
                return null;

            var wanted = new HashSet<string>(names);
            Sprite[] selected = sprites.Where(s => s != null && wanted.Contains(s.name)).ToArray();
            Sprite result = ComposeFromSprites(selected, spriteName, pixelsPerUnit);
            if (result != null) _singleCache[spriteName] = result;
            return result;
        }

        public static Sprite ComposeFromResources(string resourceKey, string spriteName, float pixelsPerUnit = 32f)
        {
            Sprite[] sprites = Resources.LoadAll<Sprite>(resourceKey);
            if (sprites == null || sprites.Length == 0)
                return null;

            Sprite[] visible = sprites.Where(IsVisibleSprite).ToArray();
            if (visible.Length == 0)
                return null;

            Sprite[] component = FindLargestAdjacentComponent(visible);
            if (component == null || component.Length == 0)
                component = visible;

            return ComposeFromSprites(component, spriteName, pixelsPerUnit);
        }

        private static Sprite ComposeFromSprites(Sprite[] component, string spriteName, float pixelsPerUnit)
        {
            if (component == null || component.Length == 0)
                return null;

            if (!CanRead(component[0]))
                return component.OrderBy(ParseSpriteIndex).FirstOrDefault();

            float minX = component.Min(s => s.textureRect.x);
            float minY = component.Min(s => s.textureRect.y);
            float maxX = component.Max(s => s.textureRect.x + s.textureRect.width);
            float maxY = component.Max(s => s.textureRect.y + s.textureRect.height);

            int outW = Mathf.Max(1, Mathf.RoundToInt(maxX - minX));
            int outH = Mathf.Max(1, Mathf.RoundToInt(maxY - minY));

            Texture2D outTex = new Texture2D(outW, outH, TextureFormat.RGBA32, false);
            outTex.filterMode = FilterMode.Point;
            outTex.wrapMode = TextureWrapMode.Clamp;

            Color32[] outPixels = new Color32[outW * outH];
            for (int i = 0; i < outPixels.Length; i++)
                outPixels[i] = new Color32(0, 0, 0, 0);

            // 텍스처 픽셀 캐시 (같은 텍스처를 반복 로드 방지)
            var texCache = new Dictionary<int, Color32[]>();

            foreach (Sprite part in component.OrderBy(ParseSpriteIndex))
            {
                if (part == null || part.texture == null)
                    continue;

                Rect r = part.textureRect;
                int sx = Mathf.RoundToInt(r.x);
                int sy = Mathf.RoundToInt(r.y);
                int sw = Mathf.RoundToInt(r.width);
                int sh = Mathf.RoundToInt(r.height);
                if (sw <= 0 || sh <= 0)
                    continue;

                int texId = part.texture.GetInstanceID();
                Color32[] src;
                if (!texCache.TryGetValue(texId, out src))
                {
                    try { src = part.texture.GetPixels32(); }
                    catch { continue; }
                    texCache[texId] = src;
                }

                int texW = part.texture.width;

                // 코너에서 플러드필로 배경 마스크 계산
                // 체커보드(코너에서 연결된 회색 픽셀)만 제거, 캐릭터 회색 픽셀은 보존
                bool[] bgMask = FloodFillBackgroundMask(src, texW, sx, sy, sw, sh);

                int dx0 = Mathf.RoundToInt(r.x - minX);
                int dy0 = Mathf.RoundToInt(r.y - minY);

                for (int y = 0; y < sh; y++)
                {
                    for (int x = 0; x < sw; x++)
                    {
                        if (bgMask[y * sw + x]) continue;

                        int srcIndex = (sy + y) * texW + (sx + x);
                        if (srcIndex < 0 || srcIndex >= src.Length)
                            continue;

                        Color32 sc = src[srcIndex];
                        if (sc.a <= 0)
                            continue;

                        int ox = dx0 + x;
                        int oy = dy0 + y;
                        if (ox < 0 || oy < 0 || ox >= outW || oy >= outH)
                            continue;

                        int outIndex = oy * outW + ox;
                        Color32 dc = outPixels[outIndex];
                        outPixels[outIndex] = sc.a >= dc.a ? sc : dc;
                    }
                }
            }

            outTex.SetPixels32(outPixels);
            outTex.Apply();

            Sprite sprite = Sprite.Create(
                outTex,
                new Rect(0f, 0f, outW, outH),
                new Vector2(0.5f, 0f),
                pixelsPerUnit,
                0,
                SpriteMeshType.FullRect);

            sprite.name = spriteName;
            return sprite;
        }

        /// <summary>
        /// 타일의 4개 코너에서 플러드필로 배경(체커보드) 영역을 계산.
        /// 코너와 연결된 회색 픽셀만 배경으로 처리해, 캐릭터 내부의 회색 픽셀은 보존.
        /// </summary>
        private static bool[] FloodFillBackgroundMask(Color32[] src, int texW, int sx, int sy, int sw, int sh)
        {
            bool[] mask = new bool[sw * sh];
            var queue = new Queue<int>();

            void TryAdd(int px, int py)
            {
                int fi = py * sw + px;
                if (mask[fi]) return;
                int si = (sy + py) * texW + (sx + px);
                if (si < 0 || si >= src.Length) return;
                if (!IsBackgroundCandidate(src[si])) return;
                mask[fi] = true;
                queue.Enqueue(fi);
            }

            TryAdd(0, 0);
            TryAdd(sw - 1, 0);
            TryAdd(0, sh - 1);
            TryAdd(sw - 1, sh - 1);

            while (queue.Count > 0)
            {
                int idx = queue.Dequeue();
                int px = idx % sw;
                int py = idx / sw;
                if (px > 0)      TryAdd(px - 1, py);
                if (px < sw - 1) TryAdd(px + 1, py);
                if (py > 0)      TryAdd(px, py - 1);
                if (py < sh - 1) TryAdd(px, py + 1);
            }

            return mask;
        }

        /// <summary>
        /// 해당 픽셀이 배경(체커보드 회색) 후보인지 판단.
        /// 완전 무채색에 가깝고, Unity 체커보드 밝기 범위(90~200) 내여야 함.
        /// </summary>
        private static bool IsBackgroundCandidate(Color32 c)
        {
            int rg = Mathf.Abs(c.r - c.g);
            int gb = Mathf.Abs(c.g - c.b);
            int br = Mathf.Abs(c.b - c.r);
            if (rg > 10 || gb > 10 || br > 10) return false;
            int v = c.r; // r≈g≈b 이므로 대표값
            return v >= 90 && v <= 200; // Unity 체커보드: 119(어둠), 170(밝음)
        }

        private static Sprite[] FindLargestAdjacentComponent(Sprite[] sprites)
        {
            if (sprites == null || sprites.Length == 0)
                return null;

            float cellW = sprites.Min(s => s.textureRect.width);
            float cellH = sprites.Min(s => s.textureRect.height);
            if (cellW <= 0f || cellH <= 0f)
                return sprites;

            var visited = new HashSet<int>();
            var best = new List<Sprite>();

            for (int i = 0; i < sprites.Length; i++)
            {
                if (visited.Contains(i))
                    continue;

                var group = new List<Sprite>();
                var queue = new Queue<int>();
                queue.Enqueue(i);
                visited.Add(i);

                while (queue.Count > 0)
                {
                    int current = queue.Dequeue();
                    group.Add(sprites[current]);

                    for (int j = 0; j < sprites.Length; j++)
                    {
                        if (visited.Contains(j))
                            continue;
                        if (!IsAdjacent(sprites[current], sprites[j], cellW, cellH))
                            continue;

                        visited.Add(j);
                        queue.Enqueue(j);
                    }
                }

                if (group.Count > best.Count)
                    best = group;
            }

            return best.ToArray();
        }

        private static bool IsAdjacent(Sprite a, Sprite b, float cellW, float cellH)
        {
            Rect ar = a.textureRect;
            Rect br = b.textureRect;
            float dx = Mathf.Abs(ar.x - br.x);
            float dy = Mathf.Abs(ar.y - br.y);

            bool horiz = Mathf.Abs(dx - cellW) <= 0.5f && dy <= 0.5f;
            bool vert = Mathf.Abs(dy - cellH) <= 0.5f && dx <= 0.5f;
            return horiz || vert;
        }

        private static bool IsVisibleSprite(Sprite sprite)
        {
            if (sprite == null || sprite.texture == null)
                return false;

            Texture2D tex = sprite.texture;
            if (!tex.isReadable)
                return true;

            Rect r = sprite.textureRect;
            int x = Mathf.RoundToInt(r.x);
            int y = Mathf.RoundToInt(r.y);
            int w = Mathf.RoundToInt(r.width);
            int h = Mathf.RoundToInt(r.height);
            if (w <= 0 || h <= 0)
                return false;

            try
            {
                Color[] pixels = tex.GetPixels(x, y, w, h);
                int alphaCount = 0;
                for (int i = 0; i < pixels.Length; i++)
                {
                    if (pixels[i].a > 0.15f)
                        alphaCount++;
                }

                return alphaCount > Mathf.Max(16, pixels.Length / 20);
            }
            catch
            {
                return true;
            }
        }

        private static bool CanRead(Sprite sprite)
        {
            return sprite != null && sprite.texture != null && sprite.texture.isReadable;
        }

        private static int ParseSpriteIndex(Sprite sprite)
        {
            if (sprite == null || string.IsNullOrEmpty(sprite.name))
                return int.MaxValue;

            int underscore = sprite.name.LastIndexOf('_');
            if (underscore < 0 || underscore >= sprite.name.Length - 1)
                return int.MaxValue;

            return int.TryParse(sprite.name.Substring(underscore + 1), out int parsed)
                ? parsed
                : int.MaxValue;
        }
    }
}
