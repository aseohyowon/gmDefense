#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using S2RD.Managers;

namespace S2RD.EditorTools
{
    /// <summary>
    /// Hero/Monster 스프라이트를 자동 임포트/슬라이스하고 프리팹/데이터베이스를 생성하는 도구입니다.
    /// </summary>
    public static class CharacterSpriteAutoSetup
    {
        private const string HeroSpriteFolder = "Assets/Heroes";
        private const string MonsterSpriteFolder = "Assets/Monsters";
        private const string LegacyEnemySpriteFolder = "Assets/Enemies";
        private const string HeroPrefabFolder = "Assets/Resources/Prefabs/Heroes";
        private const string EnemyPrefabFolder = "Assets/Resources/Prefabs/Enemies";
        private const string DatabaseFolder = "Assets/Resources/Databases";

        [MenuItem("S2RD/캐릭터 자동 설정 실행")]
        public static void 수동실행()
        {
            자동설정실행();
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("캐릭터 자동 설정이 완료되었습니다.");
        }

        [InitializeOnLoadMethod]
        private static void 에디터로드시자동실행()
        {
            const string sessionKey = "S2RD.CharacterSpriteAutoSetup.RunOnce";
            if (SessionState.GetBool(sessionKey, false))
                return;

            SessionState.SetBool(sessionKey, true);
            EditorApplication.delayCall += () =>
            {
                자동설정실행();
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            };
        }

        public static void 자동설정실행()
        {
            폴더보장();

            List<GameObject> heroPrefabs = Hero프리팹생성();
            List<GameObject> enemyPrefabs = Enemy프리팹생성();

            HeroDatabase heroDatabase = 데이터베이스로드또는생성<HeroDatabase>($"{DatabaseFolder}/HeroDatabase.asset");
            EnemyDatabase enemyDatabase = 데이터베이스로드또는생성<EnemyDatabase>($"{DatabaseFolder}/EnemyDatabase.asset");

            heroDatabase.프리팹목록설정(heroPrefabs);
            enemyDatabase.프리팹목록설정(enemyPrefabs);
            EditorUtility.SetDirty(heroDatabase);
            EditorUtility.SetDirty(enemyDatabase);
        }

        private static List<GameObject> Hero프리팹생성()
        {
            List<GameObject> prefabs = new List<GameObject>();
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", 대상폴더가져오기(HeroSpriteFolder));

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string fileName = Path.GetFileName(path).ToLowerInvariant();
                if (!fileName.EndsWith(".png"))
                    continue;

                스프라이트임포트설정(path);
                List<Sprite> sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => s.name).ToList();
                if (sprites.Count == 0)
                    continue;

                string key = Path.GetFileNameWithoutExtension(path);
                GameObject prefab = 히어로프리팹생성($"{HeroPrefabFolder}/{key}.prefab", sprites.FirstOrDefault());
                if (prefab != null)
                    prefabs.Add(prefab);
            }

            return prefabs;
        }

        private static List<GameObject> Enemy프리팹생성()
        {
            List<GameObject> prefabs = new List<GameObject>();
            string[] guids = AssetDatabase.FindAssets("t:Texture2D", 대상폴더가져오기(MonsterSpriteFolder, LegacyEnemySpriteFolder));

            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string fileName = Path.GetFileName(path).ToLowerInvariant();
                if (!fileName.EndsWith(".png"))
                    continue;

                스프라이트임포트설정(path);
                List<Sprite> sprites = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Sprite>().OrderBy(s => s.name).ToList();
                if (sprites.Count == 0)
                    continue;

                string key = Path.GetFileNameWithoutExtension(path);
                GameObject prefab = 적프리팹생성($"{EnemyPrefabFolder}/{key}.prefab", sprites[0]);
                if (prefab != null)
                    prefabs.Add(prefab);
            }

            return prefabs;
        }

        private static void 폴더보장()
        {
            EnsureFolder("Assets", "Heroes");
            EnsureFolder("Assets", "Monsters");
            EnsureFolder("Assets", "Enemies");
            EnsureFolder("Assets", "Resources");
            EnsureFolder("Assets/Resources", "Prefabs");
            EnsureFolder("Assets/Resources/Prefabs", "Heroes");
            EnsureFolder("Assets/Resources/Prefabs", "Enemies");
            EnsureFolder("Assets/Resources", "Databases");
        }

        private static void EnsureFolder(string parent, string child)
        {
            string full = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(full))
                AssetDatabase.CreateFolder(parent, child);
        }

        private static void 스프라이트임포트설정(string path)
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

            if (importer.spriteImportMode != SpriteImportMode.Multiple)
            {
                importer.spriteImportMode = SpriteImportMode.Multiple;
                changed = true;
            }

            if (importer.spritePixelsPerUnit != 32f)
            {
                importer.spritePixelsPerUnit = 32f;
                changed = true;
            }

            if (importer.filterMode != FilterMode.Point)
            {
                importer.filterMode = FilterMode.Point;
                changed = true;
            }

            if (importer.textureCompression != TextureImporterCompression.Uncompressed)
            {
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                changed = true;
            }

            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            if (texture == null)
                return;

#pragma warning disable CS0618
            SpriteMetaData[] metas = 그리드슬라이스(texture, Path.GetFileNameWithoutExtension(path));
            if (metas.Length > 0)
            {
                importer.spritesheet = metas;
                changed = true;
            }
#pragma warning restore CS0618

            if (changed)
                importer.SaveAndReimport();
        }

        private static SpriteMetaData[] 그리드슬라이스(Texture2D texture, string baseName)
        {
            const int cellSize = 32;
            int columns = Mathf.Max(1, texture.width / cellSize);
            int rows = Mathf.Max(1, texture.height / cellSize);

            List<SpriteMetaData> metas = new List<SpriteMetaData>();
            int index = 0;
            for (int row = rows - 1; row >= 0; row--)
            {
                for (int col = 0; col < columns; col++)
                {
                    Rect rect = new Rect(col * cellSize, row * cellSize, cellSize, cellSize);
                    if (rect.xMax > texture.width || rect.yMax > texture.height)
                        continue;

                    metas.Add(new SpriteMetaData
                    {
                        name = $"{baseName}_{index}",
                        rect = rect,
                        alignment = (int)SpriteAlignment.Center,
                        pivot = new Vector2(0.5f, 0.5f)
                    });
                    index++;
                }
            }

            return metas.ToArray();
        }

        private static GameObject 히어로프리팹생성(string prefabPath, Sprite defaultSprite)
        {
            GameObject go = new GameObject(Path.GetFileNameWithoutExtension(prefabPath), typeof(SpriteRenderer), typeof(S2RD.Hero.Hero));

            SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = defaultSprite;
            renderer.sortingOrder = 20;

            S2RD.Hero.Hero hero = go.GetComponent<S2RD.Hero.Hero>();
            _ = hero;

            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);
            return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }

        private static GameObject 적프리팹생성(string prefabPath, Sprite defaultSprite)
        {
            GameObject go = new GameObject(Path.GetFileNameWithoutExtension(prefabPath), typeof(SpriteRenderer), typeof(S2RD.Enemy.Enemy));

            SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = defaultSprite;
            renderer.sortingOrder = 18;

            S2RD.Enemy.Enemy enemy = go.GetComponent<S2RD.Enemy.Enemy>();
            _ = enemy;

            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);
            return AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        }

        private static T 데이터베이스로드또는생성<T>(string assetPath) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(assetPath);
            if (asset != null)
                return asset;

            T created = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(created, assetPath);
            return created;
        }

        private static string[] 대상폴더가져오기(params string[] folders)
        {
            List<string> validFolders = new List<string>();
            for (int i = 0; i < folders.Length; i++)
            {
                string folder = folders[i];
                if (AssetDatabase.IsValidFolder(folder))
                    validFolders.Add(folder);
            }

            return validFolders.Count > 0 ? validFolders.ToArray() : folders;
        }
    }

    /// <summary>
    /// Heroes/Monsters(및 레거시 Enemies) 폴더의 PNG 신규 임포트 시 자동 프리팹/DB 갱신을 수행합니다.
    /// </summary>
    public class CharacterSpritePostprocessor : AssetPostprocessor
    {
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            _ = deletedAssets;
            _ = movedAssets;
            _ = movedFromAssetPaths;

            bool hasCharacterPng = importedAssets.Any(path =>
            {
                string fileName = Path.GetFileName(path).ToLowerInvariant();
                bool isHero = path.StartsWith("Assets/Heroes") && fileName.EndsWith(".png");
                bool isMonster = path.StartsWith("Assets/Monsters") && fileName.EndsWith(".png");
                bool isLegacyEnemy = path.StartsWith("Assets/Enemies") && fileName.EndsWith(".png");
                return isHero || isMonster || isLegacyEnemy;
            });

            if (!hasCharacterPng)
                return;

            CharacterSpriteAutoSetup.자동설정실행();
            AssetDatabase.SaveAssets();
        }
    }
}
#endif
