#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace S2RD.EditorTools
{
    /// <summary>
    /// 지정된 폴더의 PNG를 스프라이트 규칙으로 일괄 설정하는 에디터 도구입니다.
    /// </summary>
    public static class AutoSpriteImportSetup
    {
        // 자동 설정 대상 폴더 목록
        private static readonly string[] 대상폴더 =
        {
            "Assets/Heroes",
            "Assets/Monsters",
            "Assets/Enemies",
            "Assets/Sprites"
        };

        // 스프라이트 시트 슬라이스 셀 크기
        private const int 셀너비 = 32;
        private const int 셀높이 = 32;

        [MenuItem("Tools/Auto Setup Sprites")]
        public static void 자동설정실행()
        {
            int 처리수 = 0;
            int 변경수 = 0;

            string[] guids = AssetDatabase.FindAssets("t:Texture2D", 대상폴더);
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                if (!path.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
                    continue;

                처리수++;
                if (개별스프라이트설정(path))
                    변경수++;
            }

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log($"[Auto Setup Sprites] 처리: {처리수}개, 변경: {변경수}개");
        }

        /// <summary>
        /// 단일 PNG 파일에 임포트 설정과 슬라이스를 적용합니다.
        /// </summary>
        private static bool 개별스프라이트설정(string assetPath)
        {
            TextureImporter importer = AssetImporter.GetAtPath(assetPath) as TextureImporter;
            if (importer == null)
                return false;

            bool changed = false;

            // Texture Type = Sprite (2D and UI)
            if (importer.textureType != TextureImporterType.Sprite)
            {
                importer.textureType = TextureImporterType.Sprite;
                changed = true;
            }

            // Sprite Mode = Multiple
            if (importer.spriteImportMode != SpriteImportMode.Multiple)
            {
                importer.spriteImportMode = SpriteImportMode.Multiple;
                changed = true;
            }

            // Pixels Per Unit = 32
            if (!Mathf.Approximately(importer.spritePixelsPerUnit, 32f))
            {
                importer.spritePixelsPerUnit = 32f;
                changed = true;
            }

            // Filter Mode = Point (No Filter)
            if (importer.filterMode != FilterMode.Point)
            {
                importer.filterMode = FilterMode.Point;
                changed = true;
            }

            // Compression = None
            if (importer.textureCompression != TextureImporterCompression.Uncompressed)
            {
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                changed = true;
            }

            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(assetPath);
            if (texture == null)
                return false;

            // Sprite Sheet 자동 Grid Slice (Cell 32x32)
#pragma warning disable CS0618
            SpriteMetaData[] metas = 그리드슬라이스메타생성(texture, Path.GetFileNameWithoutExtension(assetPath));
            if (metas.Length > 0)
            {
                importer.spritesheet = metas;
                changed = true;
            }
#pragma warning restore CS0618

            if (!changed)
                return false;

            importer.SaveAndReimport();
            return true;
        }

        /// <summary>
        /// 텍스처를 32x32 기준으로 그리드 슬라이스 메타 데이터로 변환합니다.
        /// </summary>
        private static SpriteMetaData[] 그리드슬라이스메타생성(Texture2D texture, string baseName)
        {
            List<SpriteMetaData> result = new List<SpriteMetaData>();

            // 32보다 작은 이미지는 단일 스프라이트로 처리
            if (texture.width < 셀너비 || texture.height < 셀높이)
            {
                result.Add(new SpriteMetaData
                {
                    name = $"{baseName}_0",
                    rect = new Rect(0f, 0f, texture.width, texture.height),
                    alignment = (int)SpriteAlignment.Center,
                    pivot = new Vector2(0.5f, 0.5f)
                });

                return result.ToArray();
            }

            int columns = texture.width / 셀너비;
            int rows = texture.height / 셀높이;
            int index = 0;

            // 유니티 좌표계 기준으로 위에서 아래 방향으로 슬라이스 이름을 부여
            for (int row = rows - 1; row >= 0; row--)
            {
                for (int col = 0; col < columns; col++)
                {
                    Rect rect = new Rect(col * 셀너비, row * 셀높이, 셀너비, 셀높이);
                    result.Add(new SpriteMetaData
                    {
                        name = $"{baseName}_{index}",
                        rect = rect,
                        alignment = (int)SpriteAlignment.Center,
                        pivot = new Vector2(0.5f, 0.5f)
                    });
                    index++;
                }
            }

            return result.ToArray();
        }
    }
}
#endif
