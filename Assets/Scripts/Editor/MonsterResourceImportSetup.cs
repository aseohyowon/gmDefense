#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace S2RD.EditorTools
{
    public static class MonsterResourceImportSetup
    {
        private static readonly string[] Paths =
        {
            "Assets/Resources/EnemyGoblin.png",
            "Assets/Resources/EnemyBoss.png"
        };

        [InitializeOnLoadMethod]
        private static void OnLoad()
        {
            EditorApplication.delayCall += Apply;
        }

        [MenuItem("S2RD/몬스터 리소스 임포트 고정")]
        public static void Apply()
        {
            for (int i = 0; i < Paths.Length; i++)
                Configure(Paths[i]);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void Configure(string path)
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

            if (!importer.isReadable)
            {
                importer.isReadable = true;
                changed = true;
            }

            if (changed)
                importer.SaveAndReimport();
        }
    }
}
#endif