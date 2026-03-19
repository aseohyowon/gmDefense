using UnityEngine;
using UnityEngine.Tilemaps;

namespace S2RD.Core
{
    /// <summary>
    /// 미리 설계한 50x50 타일맵을 생성하는 빌더입니다.
    /// 랜덤이 아닌 고정 구조로 숲/던전 느낌의 길, 장애물, 장식을 배치합니다.
    /// </summary>
    public class TilemapMapBuilder : MonoBehaviour
    {
        private const int 맵크기 = 50;

        public Vector3 플레이어시작위치 => new Vector3(0f, 0f, 0f);

        public void 맵생성()
        {
            Grid grid = new GameObject("Grid", typeof(Grid)).GetComponent<Grid>();
            grid.cellSize = Vector3.one;

            Tilemap 바닥맵 = 타일맵생성(grid.transform, "GroundTilemap", 0);
            Tilemap 길맵 = 타일맵생성(grid.transform, "RoadTilemap", 1);
            Tilemap 장애물맵 = 타일맵생성(grid.transform, "ObstacleTilemap", 2);
            Tilemap 장식맵 = 타일맵생성(grid.transform, "DecorTilemap", 3);

            Tile 잔디 = PixelArtFactory.잔디타일생성();
            Tile 길 = PixelArtFactory.길타일생성();
            Tile 장애물 = PixelArtFactory.장애물타일생성();
            Tile 장식 = PixelArtFactory.장식타일생성();

            int 반 = 맵크기 / 2;
            for (int y = -반; y < 반; y++)
            {
                for (int x = -반; x < 반; x++)
                {
                    바닥맵.SetTile(new Vector3Int(x, y, 0), 잔디);
                }
            }

            길배치(길맵, 길);
            장애물배치(장애물맵, 장애물);
            장식배치(장식맵, 장식);
        }

        private static Tilemap 타일맵생성(Transform parent, string 이름, int 정렬순서)
        {
            GameObject tilemapObject = new GameObject(이름, typeof(Tilemap), typeof(TilemapRenderer));
            tilemapObject.transform.SetParent(parent, false);

            TilemapRenderer renderer = tilemapObject.GetComponent<TilemapRenderer>();
            renderer.sortOrder = TilemapRenderer.SortOrder.TopRight;
            renderer.sortingOrder = 정렬순서;

            return tilemapObject.GetComponent<Tilemap>();
        }

        private void 길배치(Tilemap 길맵, Tile 길타일)
        {
            // 중앙 원형 광장
            for (int y = -4; y <= 4; y++)
                for (int x = -4; x <= 4; x++)
                    if (x * x + y * y <= 22)
                        길맵.SetTile(new Vector3Int(x, y, 0), 길타일);

            // 상하좌우 연결 길
            직사각길(길맵, 길타일, -2, 2, 5, 24);
            직사각길(길맵, 길타일, -2, 2, -24, -5);
            직사각길(길맵, 길타일, -24, -5, -2, 2);
            직사각길(길맵, 길타일, 5, 24, -2, 2);

            // 자연스러운 굽은 길 형태
            직사각길(길맵, 길타일, -18, -10, 8, 12);
            직사각길(길맵, 길타일, 10, 18, -12, -8);
            직사각길(길맵, 길타일, -12, -8, -18, -10);
            직사각길(길맵, 길타일, 8, 12, 10, 18);
        }

        private void 장애물배치(Tilemap 장애물맵, Tile 장애물타일)
        {
            // 중앙 주변 바위 지대
            직사각장애물(장애물맵, 장애물타일, -10, -7, -10, -6);
            직사각장애물(장애물맵, 장애물타일, 7, 10, 6, 10);
            직사각장애물(장애물맵, 장애물타일, -12, -8, 7, 11);
            직사각장애물(장애물맵, 장애물타일, 8, 12, -11, -7);

            // 외곽 차단물
            직사각장애물(장애물맵, 장애물타일, -24, -22, -24, 24);
            직사각장애물(장애물맵, 장애물타일, 22, 24, -24, 24);
            직사각장애물(장애물맵, 장애물타일, -24, 24, 22, 24);
            직사각장애물(장애물맵, 장애물타일, -24, 24, -24, -22);
        }

        private void 장식배치(Tilemap 장식맵, Tile 장식타일)
        {
            // 숲 느낌의 덤불/풀 장식
            for (int x = -20; x <= 20; x += 4)
            {
                if (Mathf.Abs(x) < 6)
                    continue;

                장식맵.SetTile(new Vector3Int(x, 16, 0), 장식타일);
                장식맵.SetTile(new Vector3Int(x, -16, 0), 장식타일);
            }

            for (int y = -20; y <= 20; y += 4)
            {
                if (Mathf.Abs(y) < 6)
                    continue;

                장식맵.SetTile(new Vector3Int(16, y, 0), 장식타일);
                장식맵.SetTile(new Vector3Int(-16, y, 0), 장식타일);
            }
        }

        private static void 직사각길(Tilemap 길맵, Tile 길타일, int minX, int maxX, int minY, int maxY)
        {
            for (int y = minY; y <= maxY; y++)
                for (int x = minX; x <= maxX; x++)
                    길맵.SetTile(new Vector3Int(x, y, 0), 길타일);
        }

        private static void 직사각장애물(Tilemap 장애물맵, Tile 장애물타일, int minX, int maxX, int minY, int maxY)
        {
            for (int y = minY; y <= maxY; y++)
                for (int x = minX; x <= maxX; x++)
                    장애물맵.SetTile(new Vector3Int(x, y, 0), 장애물타일);
        }
    }
}
