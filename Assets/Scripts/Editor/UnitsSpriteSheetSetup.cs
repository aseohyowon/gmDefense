#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using S2RD.Hero;
using S2RD.Managers;

namespace S2RD.EditorTools
{
    /// <summary>
    /// units_sprite_sheet.png를 기반으로 캐릭터 애니메이션/프리팹/DB를 구성합니다.
    /// </summary>
    public static class UnitsSpriteSheetSetup
    {
        private const string SpriteSheetPath = "Assets/Resources/Units_sprite_sheet.png";
        private const string SourceSpriteSheetPath = "Assets/Sprites/units_sprite_sheet.png";
        private const string AnimationFolder = "Assets/Animations/Units";
        private const string ControllerFolder = "Assets/AnimatorControllers/Units";
        private const string HeroPrefabFolder = "Assets/Resources/Prefabs/Heroes";
        private const string EnemyPrefabFolder = "Assets/Resources/Prefabs/Enemies";
        private const string DatabaseFolder = "Assets/Resources/Databases";

        [MenuItem("S2RD/유닛 스프라이트시트 설정 실행")]
        public static void 실행()
        {
            리소스시트보장();

            if (!AssetDatabase.LoadAssetAtPath<Texture2D>(SpriteSheetPath))
            {
                Debug.LogError($"스프라이트시트를 찾을 수 없습니다: {SpriteSheetPath}");
                return;
            }

            폴더보장();
            스프라이트시트설정및슬라이스();

            List<Sprite> frames = 프레임목록가져오기();
            if (frames.Count < 22)
            {
                Debug.LogError($"슬라이스된 프레임 수가 부족합니다. 필요: 22, 현재: {frames.Count}");
                return;
            }

            GameObject heroArcher = 히어로프리팹생성("HeroArcher", frames, 0, 3, HeroType.Archer);
            GameObject heroMage = 히어로프리팹생성("HeroMage", frames, 8, 11, HeroType.Mage);

            GameObject enemyGoblin = 적프리팹생성("EnemyGoblin", frames, 4, 7, false);
            GameObject enemyOrc = 적프리팹생성("EnemyOrc", frames, 12, 15, false);
            GameObject enemySlime = 적프리팹생성("EnemySlime", frames, 16, 19, false);
            GameObject enemyBoss = 적프리팹생성("EnemyBoss", frames, 20, 21, true);

            HeroDatabase heroDb = 데이터베이스로드또는생성<HeroDatabase>($"{DatabaseFolder}/HeroDatabase.asset");
            heroDb.프리팹목록설정(new List<GameObject> { heroArcher, heroMage }.Where(x => x != null).ToList());
            EditorUtility.SetDirty(heroDb);

            EnemyDatabase enemyDb = 데이터베이스로드또는생성<EnemyDatabase>($"{DatabaseFolder}/EnemyDatabase.asset");
            enemyDb.프리팹목록설정(new List<GameObject> { enemyGoblin, enemyOrc, enemySlime, enemyBoss }.Where(x => x != null).ToList());
            EditorUtility.SetDirty(enemyDb);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("units_sprite_sheet 기반 캐릭터 설정이 완료되었습니다.");
        }

        [InitializeOnLoadMethod]
        private static void 에디터로드시자동실행()
        {
            const string sessionKey = "S2RD.UnitsSpriteSheetSetup.RunOnce";
            if (SessionState.GetBool(sessionKey, false))
                return;

            SessionState.SetBool(sessionKey, true);
            EditorApplication.delayCall += () =>
            {
                bool hasResourceSheet = AssetDatabase.LoadAssetAtPath<Texture2D>(SpriteSheetPath) != null;
                bool hasSourceSheet = AssetDatabase.LoadAssetAtPath<Texture2D>(SourceSpriteSheetPath) != null;
                if (hasResourceSheet || hasSourceSheet)
                    실행();
            };
        }

        private static void 폴더보장()
        {
            EnsureFolder("Assets", "Resources");
            EnsureFolder("Assets", "Animations");
            EnsureFolder("Assets/Animations", "Units");
            EnsureFolder("Assets", "AnimatorControllers");
            EnsureFolder("Assets/AnimatorControllers", "Units");
            EnsureFolder("Assets/Resources", "Prefabs");
            EnsureFolder("Assets/Resources/Prefabs", "Heroes");
            EnsureFolder("Assets/Resources/Prefabs", "Enemies");
            EnsureFolder("Assets/Resources", "Databases");
        }

        private static void 리소스시트보장()
        {
            if (AssetDatabase.LoadAssetAtPath<Texture2D>(SpriteSheetPath) != null)
                return;

            Texture2D source = AssetDatabase.LoadAssetAtPath<Texture2D>(SourceSpriteSheetPath);
            if (source == null)
                return;

            폴더보장();
            AssetDatabase.CopyAsset(SourceSpriteSheetPath, SpriteSheetPath);
            AssetDatabase.ImportAsset(SpriteSheetPath, ImportAssetOptions.ForceSynchronousImport);
        }

        private static void EnsureFolder(string parent, string child)
        {
            string full = $"{parent}/{child}";
            if (!AssetDatabase.IsValidFolder(full))
                AssetDatabase.CreateFolder(parent, child);
        }

        private static void 스프라이트시트설정및슬라이스()
        {
            TextureImporter importer = AssetImporter.GetAtPath(SpriteSheetPath) as TextureImporter;
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

            if (!Mathf.Approximately(importer.spritePixelsPerUnit, 32f))
            {
                importer.spritePixelsPerUnit = 32f;
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

            if (importer.textureCompression != TextureImporterCompression.Uncompressed)
            {
                importer.textureCompression = TextureImporterCompression.Uncompressed;
                changed = true;
            }

            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(SpriteSheetPath);
            if (texture == null)
                return;

#pragma warning disable CS0618
            SpriteMetaData[] metas = 그리드슬라이스(texture, "units_sprite_sheet");
            importer.spritesheet = metas;
            changed = true;
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

        private static List<Sprite> 프레임목록가져오기()
        {
            List<Sprite> sprites = AssetDatabase.LoadAllAssetsAtPath(SpriteSheetPath)
                .OfType<Sprite>()
                .OrderBy(인덱스파싱)
                .ToList();
            return sprites;
        }

        private static int 인덱스파싱(Sprite sprite)
        {
            if (sprite == null || string.IsNullOrEmpty(sprite.name))
                return int.MaxValue;

            int underscore = sprite.name.LastIndexOf('_');
            if (underscore < 0 || underscore >= sprite.name.Length - 1)
                return int.MaxValue;

            return int.TryParse(sprite.name.Substring(underscore + 1), out int index)
                ? index
                : int.MaxValue;
        }

        private static GameObject 히어로프리팹생성(string prefabName, List<Sprite> frames, int start, int end, HeroType type)
        {
            List<Sprite> selected = 프레임선택(frames, start, end);
            if (selected.Count == 0)
                return null;

            AnimationClip idle = 클립생성($"{AnimationFolder}/{prefabName}_Idle.anim", selected, 8f, true);
            AnimationClip attack = 클립생성($"{AnimationFolder}/{prefabName}_Attack.anim", selected, 10f, false);
            AnimatorController controller = 히어로컨트롤러생성($"{ControllerFolder}/{prefabName}.controller", idle, attack);

            string prefabPath = $"{HeroPrefabFolder}/{prefabName}.prefab";
            GameObject go = new GameObject(prefabName, typeof(SpriteRenderer), typeof(Animator), typeof(S2RD.Hero.Hero));
            SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = selected[0];
            renderer.sortingOrder = 20;

            Animator animator = go.GetComponent<Animator>();
            animator.runtimeAnimatorController = controller;

            S2RD.Hero.Hero hero = go.GetComponent<S2RD.Hero.Hero>();
            _ = hero;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static GameObject 적프리팹생성(string prefabName, List<Sprite> frames, int start, int end, bool isBoss)
        {
            List<Sprite> selected = 프레임선택(frames, start, end);
            if (selected.Count == 0)
                return null;

            AnimationClip clip = 클립생성($"{AnimationFolder}/{prefabName}_Move.anim", selected, 8f, true);
            AnimatorController controller = 적컨트롤러생성($"{ControllerFolder}/{prefabName}.controller", clip);

            string prefabPath = $"{EnemyPrefabFolder}/{prefabName}.prefab";
            System.Type enemyType = isBoss ? typeof(S2RD.Enemy.BossEnemy) : typeof(S2RD.Enemy.Enemy);
            GameObject go = new GameObject(prefabName, typeof(SpriteRenderer), typeof(Animator), enemyType);

            SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = selected[0];
            renderer.sortingOrder = 18;

            Animator animator = go.GetComponent<Animator>();
            animator.runtimeAnimatorController = controller;

            GameObject prefab = PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);
            return prefab;
        }

        private static List<Sprite> 프레임선택(List<Sprite> source, int start, int end)
        {
            List<Sprite> selected = new List<Sprite>();
            for (int i = start; i <= end; i++)
            {
                if (i >= 0 && i < source.Count)
                    selected.Add(source[i]);
            }

            return selected;
        }

        private static AnimationClip 클립생성(string path, List<Sprite> frames, float fps, bool loop)
        {
            AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(path);
            if (clip == null)
            {
                clip = new AnimationClip();
                AssetDatabase.CreateAsset(clip, path);
            }

            clip.frameRate = fps;

            EditorCurveBinding binding = new EditorCurveBinding
            {
                path = string.Empty,
                type = typeof(SpriteRenderer),
                propertyName = "m_Sprite"
            };

            ObjectReferenceKeyframe[] keys = new ObjectReferenceKeyframe[frames.Count];
            for (int i = 0; i < frames.Count; i++)
            {
                keys[i] = new ObjectReferenceKeyframe
                {
                    time = i / fps,
                    value = frames[i]
                };
            }

            AnimationUtility.SetObjectReferenceCurve(clip, binding, keys);

            SerializedObject serializedClip = new SerializedObject(clip);
            SerializedProperty settings = serializedClip.FindProperty("m_AnimationClipSettings");
            if (settings != null)
            {
                settings.FindPropertyRelative("m_LoopTime").boolValue = loop;
                serializedClip.ApplyModifiedProperties();
            }

            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static AnimatorController 히어로컨트롤러생성(string path, AnimationClip idle, AnimationClip attack)
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (controller == null)
                controller = AnimatorController.CreateAnimatorControllerAtPath(path);

            AnimatorControllerLayer layer = controller.layers[0];
            AnimatorStateMachine sm = layer.stateMachine;
            sm.states = new ChildAnimatorState[0];

            AnimatorState idleState = sm.AddState("Idle");
            AnimatorState attackState = sm.AddState("Attack");
            idleState.motion = idle;
            attackState.motion = attack;
            sm.defaultState = idleState;

            if (!controller.parameters.Any(p => p.name == "Attack"))
                controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);

            AnimatorStateTransition toAttack = idleState.AddTransition(attackState);
            toAttack.hasExitTime = false;
            toAttack.duration = 0f;
            toAttack.AddCondition(AnimatorConditionMode.If, 0f, "Attack");

            AnimatorStateTransition toIdle = attackState.AddTransition(idleState);
            toIdle.hasExitTime = true;
            toIdle.exitTime = 0.95f;
            toIdle.duration = 0.05f;

            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static AnimatorController 적컨트롤러생성(string path, AnimationClip move)
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (controller == null)
                controller = AnimatorController.CreateAnimatorControllerAtPath(path);

            AnimatorControllerLayer layer = controller.layers[0];
            AnimatorStateMachine sm = layer.stateMachine;
            sm.states = new ChildAnimatorState[0];

            AnimatorState moveState = sm.AddState("Move");
            moveState.motion = move;
            sm.defaultState = moveState;

            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static T 데이터베이스로드또는생성<T>(string path) where T : ScriptableObject
        {
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);
            if (asset != null)
                return asset;

            T created = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(created, path);
            return created;
        }
    }
}
#endif
