#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace S2RD.EditorTools
{
    /// <summary>
    /// Enemies/Heroes 폴더의 스프라이트를 SpriteRenderer 프리팹으로 자동 생성합니다.
    /// </summary>
    public static class SpriteFolderPrefabTool
    {
        private const string EnemySpriteFolder = "Assets/Enemies";
        private const string HeroSpriteFolder = "Assets/Heroes";

        private const string EnemyPrefabFolder = "Assets/Prefabs/Enemies";
        private const string HeroPrefabFolder = "Assets/Prefabs/Heroes";

        [MenuItem("Tools/Generate Prefabs From Enemies Heroes")]
        public static void 프리팹자동생성()
        {
            EnsureFolder("Assets", "Prefabs");
            EnsureFolder("Assets/Prefabs", "Enemies");
            EnsureFolder("Assets/Prefabs", "Heroes");

            int enemyCount = 폴더기반프리팹생성(EnemySpriteFolder, EnemyPrefabFolder);
            int heroCount = 폴더기반프리팹생성(HeroSpriteFolder, HeroPrefabFolder);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[SpriteFolderPrefabTool] Enemy Prefab: {enemyCount}개, Hero Prefab: {heroCount}개 생성/갱신 완료");
        }

        private static int 폴더기반프리팹생성(string spriteFolder, string prefabFolder)
        {
            if (!AssetDatabase.IsValidFolder(spriteFolder))
                return 0;

            int createdCount = 0;
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", new[] { spriteFolder });

            for (int i = 0; i < guids.Length; i++)
            {
                string texturePath = AssetDatabase.GUIDToAssetPath(guids[i]);
                if (!texturePath.EndsWith(".png", System.StringComparison.OrdinalIgnoreCase))
                    continue;

                List<Sprite> sprites = new List<Sprite>(AssetDatabase.LoadAllAssetsAtPath(texturePath) as Object[] == null
                    ? new Sprite[0]
                    : System.Array.ConvertAll(AssetDatabase.LoadAllAssetsAtPath(texturePath), x => x as Sprite));

                if (sprites.Count == 0)
                {
                    Sprite singleSprite = AssetDatabase.LoadAssetAtPath<Sprite>(texturePath);
                    if (singleSprite != null)
                        sprites.Add(singleSprite);
                }
                else
                {
                    sprites.RemoveAll(s => s == null);
                }

                if (sprites.Count == 0)
                    continue;

                for (int s = 0; s < sprites.Count; s++)
                {
                    Sprite sprite = sprites[s];
                    string prefabName = sprite.name;
                    if (string.IsNullOrWhiteSpace(prefabName))
                        prefabName = Path.GetFileNameWithoutExtension(texturePath);

                    string prefabPath = $"{prefabFolder}/{prefabName}.prefab";
                    GameObject temp = new GameObject(prefabName, typeof(SpriteRenderer));

                    SpriteRenderer renderer = temp.GetComponent<SpriteRenderer>();
                    renderer.sprite = sprite;

                    PrefabUtility.SaveAsPrefabAsset(temp, prefabPath);
                    Object.DestroyImmediate(temp);
                    createdCount++;
                }
            }

            return createdCount;
        }

        private static void EnsureFolder(string parent, string child)
        {
            string full = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(full))
                AssetDatabase.CreateFolder(parent, child);
        }
    }
}
#endif
