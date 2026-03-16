using UnityEngine;
using UnityEngine.Tilemaps;

namespace S2RD.Core
{
    /// <summary>
    /// 픽셀 아트 스타일 리소스를 코드로 생성하는 팩토리입니다.
    /// 모든 캐릭터 스프라이트는 128x128 이상으로 생성합니다.
    /// </summary>
    public static class PixelArtFactory
    {
        public static Tile 잔디타일생성()
        {
            Texture2D texture = 타일텍스처생성(new Color(0.18f, 0.47f, 0.20f, 1f), new Color(0.22f, 0.56f, 0.26f, 1f));
            return 타일생성(texture);
        }

        public static Tile 길타일생성()
        {
            Texture2D texture = 타일텍스처생성(new Color(0.34f, 0.24f, 0.16f, 1f), new Color(0.40f, 0.29f, 0.19f, 1f));
            return 타일생성(texture);
        }

        public static Tile 장애물타일생성()
        {
            Texture2D texture = 타일텍스처생성(new Color(0.14f, 0.16f, 0.17f, 1f), new Color(0.23f, 0.24f, 0.25f, 1f));
            return 타일생성(texture);
        }

        public static Tile 장식타일생성()
        {
            Texture2D texture = 타일텍스처생성(new Color(0.20f, 0.40f, 0.18f, 1f), new Color(0.30f, 0.62f, 0.23f, 1f));
            return 타일생성(texture);
        }

        public static Sprite[] 영웅애니메이션프레임생성(Color 주색, Color 보조색)
        {
            return new[]
            {
                캐릭터프레임생성(주색, 보조색, 0),
                캐릭터프레임생성(주색, 보조색, 1),
                캐릭터프레임생성(주색, 보조색, 2)
            };
        }

        public static Sprite[] 몬스터애니메이션프레임생성(Color 주색, Color 보조색)
        {
            return new[]
            {
                몬스터프레임생성(주색, 보조색, 0),
                몬스터프레임생성(주색, 보조색, 1),
                몬스터프레임생성(주색, 보조색, 2)
            };
        }

        private static Sprite 캐릭터프레임생성(Color 주색, Color 보조색, int 프레임)
        {
            Texture2D texture = 새텍스처(128, 128);
            Color 피부색 = new Color(0.95f, 0.86f, 0.73f, 1f);

            칠(texture, 44, 28, 40, 56, 주색);
            칠(texture, 46, 84, 36, 28, 피부색);
            칠(texture, 40, 46, 8, 26, 보조색);
            칠(texture, 80, 46, 8, 26, 보조색);
            칠(texture, 50, 12, 12, 18, 보조색);
            칠(texture, 66, 12, 12, 18, 보조색);

            if (프레임 == 1)
            {
                칠(texture, 40, 50, 10, 30, 보조색);
                칠(texture, 78, 42, 12, 28, 보조색);
            }
            else if (프레임 == 2)
            {
                칠(texture, 38, 40, 12, 28, 보조색);
                칠(texture, 78, 52, 12, 28, 보조색);
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.1f), 128f);
        }

        private static Sprite 몬스터프레임생성(Color 주색, Color 보조색, int 프레임)
        {
            Texture2D texture = 새텍스처(128, 128);

            칠(texture, 28, 26, 72, 64, 주색);
            칠(texture, 18, 52, 14, 24, 보조색);
            칠(texture, 96, 52, 14, 24, 보조색);
            칠(texture, 44, 90, 12, 10, Color.white);
            칠(texture, 72, 90, 12, 10, Color.white);
            칠(texture, 48, 92, 6, 6, Color.black);
            칠(texture, 76, 92, 6, 6, Color.black);

            if (프레임 == 1)
            {
                칠(texture, 16, 46, 16, 20, 보조색);
                칠(texture, 96, 60, 16, 20, 보조색);
            }
            else if (프레임 == 2)
            {
                칠(texture, 16, 60, 16, 20, 보조색);
                칠(texture, 96, 46, 16, 20, 보조색);
            }

            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 128, 128), new Vector2(0.5f, 0.1f), 128f);
        }

        private static Texture2D 타일텍스처생성(Color baseColor, Color noiseColor)
        {
            Texture2D texture = 새텍스처(32, 32);
            for (int y = 0; y < 32; y++)
            {
                for (int x = 0; x < 32; x++)
                {
                    float noise = Mathf.PerlinNoise((x + 3f) * 0.23f, (y + 7f) * 0.19f);
                    texture.SetPixel(x, y, noise > 0.55f ? noiseColor : baseColor);
                }
            }

            texture.Apply();
            return texture;
        }

        private static Texture2D 새텍스처(int width, int height)
        {
            Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
            texture.filterMode = FilterMode.Point;
            texture.wrapMode = TextureWrapMode.Clamp;

            Color transparent = new Color(0f, 0f, 0f, 0f);
            for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                    texture.SetPixel(x, y, transparent);

            return texture;
        }

        private static Tile 타일생성(Texture2D texture)
        {
            Tile tile = ScriptableObject.CreateInstance<Tile>();
            tile.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 32f);
            tile.color = Color.white;
            return tile;
        }

        private static void 칠(Texture2D texture, int x, int y, int width, int height, Color color)
        {
            for (int iy = y; iy < y + height; iy++)
            {
                for (int ix = x; ix < x + width; ix++)
                {
                    if (ix < 0 || iy < 0 || ix >= texture.width || iy >= texture.height)
                        continue;

                    texture.SetPixel(ix, iy, color);
                }
            }
        }
    }
}
