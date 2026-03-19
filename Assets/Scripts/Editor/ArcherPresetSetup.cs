#if UNITY_EDITOR
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace S2RD.EditorTools
{
    public static class ArcherPresetSetup
    {
        private const string SheetPath = "Assets/Resources/Units_sprite_sheet.png";
        private const string AnimFolder = "Assets/Animations/Units";
        private const string ControllerFolder = "Assets/AnimatorControllers/Units";
        private const string PrefabFolder = "Assets/Resources/Prefabs/Heroes";

        private static readonly int[] IdleIndices = { 82, 83, 84 };
        private static readonly int[] AttackIndices = { 86, 87, 88, 89 };

        [MenuItem("S2RD/궁수 프리셋 생성")]
        public static void 생성()
        {
            EnsureFolder("Assets", "Animations");
            EnsureFolder("Assets/Animations", "Units");
            EnsureFolder("Assets", "AnimatorControllers");
            EnsureFolder("Assets/AnimatorControllers", "Units");
            EnsureFolder("Assets", "Resources");
            EnsureFolder("Assets/Resources", "Prefabs");
            EnsureFolder("Assets/Resources/Prefabs", "Heroes");

            List<Sprite> all = AssetDatabase.LoadAllAssetsAtPath(SheetPath)
                .OfType<Sprite>()
                .OrderBy(ParseIndex)
                .ToList();

            if (all.Count == 0)
            {
                Debug.LogError($"스프라이트시트를 찾지 못했습니다: {SheetPath}");
                return;
            }

            List<Sprite> idle = PickByIndex(all, IdleIndices);
            List<Sprite> attack = PickByIndex(all, AttackIndices);
            if (idle.Count == 0)
            {
                Debug.LogError("궁수 Idle 프레임을 찾지 못했습니다. 인덱스를 확인하세요.");
                return;
            }

            if (attack.Count == 0)
                attack = idle;

            AnimationClip idleClip = CreateClip($"{AnimFolder}/HeroArcher_Idle.anim", idle, 8f, true);
            AnimationClip attackClip = CreateClip($"{AnimFolder}/HeroArcher_Attack.anim", attack, 10f, false);
            AnimatorController controller = CreateController($"{ControllerFolder}/HeroArcher.controller", idleClip, attackClip);
            CreatePrefab(controller, idle[0]);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("궁수 프리셋 생성 완료: HeroArcher_Animated.prefab");
        }

        private static void CreatePrefab(AnimatorController controller, Sprite defaultSprite)
        {
            string prefabPath = $"{PrefabFolder}/HeroArcher_Animated.prefab";
            GameObject go = new GameObject("HeroArcher_Animated", typeof(SpriteRenderer), typeof(Animator), typeof(S2RD.Hero.Hero));
            SpriteRenderer renderer = go.GetComponent<SpriteRenderer>();
            renderer.sprite = defaultSprite;
            renderer.sortingOrder = 20;

            Animator animator = go.GetComponent<Animator>();
            animator.runtimeAnimatorController = controller;

            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);
        }

        private static AnimatorController CreateController(string path, AnimationClip idle, AnimationClip attack)
        {
            AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (controller == null)
                controller = AnimatorController.CreateAnimatorControllerAtPath(path);

            AnimatorControllerLayer layer = controller.layers[0];
            AnimatorStateMachine sm = layer.stateMachine;
            sm.states = new ChildAnimatorState[0];
            sm.anyStateTransitions = new AnimatorStateTransition[0];

            AnimatorState idleState = sm.AddState("Idle");
            idleState.motion = idle;
            AnimatorState attackState = sm.AddState("Attack");
            attackState.motion = attack;

            sm.defaultState = idleState;

            AnimatorControllerParameter attackParam = controller.parameters.FirstOrDefault(p => p.name == "Attack");
            if (attackParam == null)
                controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);

            AnimatorStateTransition idleToAttack = idleState.AddTransition(attackState);
            idleToAttack.hasExitTime = false;
            idleToAttack.duration = 0f;
            idleToAttack.AddCondition(AnimatorConditionMode.If, 0f, "Attack");

            AnimatorStateTransition attackToIdle = attackState.AddTransition(idleState);
            attackToIdle.hasExitTime = true;
            attackToIdle.exitTime = 1f;
            attackToIdle.duration = 0f;

            EditorUtility.SetDirty(controller);
            return controller;
        }

        private static AnimationClip CreateClip(string path, List<Sprite> frames, float fps, bool loop)
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

            SerializedObject so = new SerializedObject(clip);
            SerializedProperty settings = so.FindProperty("m_AnimationClipSettings");
            if (settings != null)
            {
                settings.FindPropertyRelative("m_LoopTime").boolValue = loop;
                so.ApplyModifiedProperties();
            }

            EditorUtility.SetDirty(clip);
            return clip;
        }

        private static List<Sprite> PickByIndex(List<Sprite> all, int[] indices)
        {
            Dictionary<int, Sprite> map = new Dictionary<int, Sprite>();
            for (int i = 0; i < all.Count; i++)
            {
                Sprite s = all[i];
                int idx = ParseIndex(s);
                if (idx >= 0 && !map.ContainsKey(idx))
                    map.Add(idx, s);
            }

            List<Sprite> selected = new List<Sprite>();
            for (int i = 0; i < indices.Length; i++)
            {
                if (map.TryGetValue(indices[i], out Sprite sprite) && sprite != null)
                    selected.Add(sprite);
            }

            return selected;
        }

        private static int ParseIndex(Sprite sprite)
        {
            if (sprite == null || string.IsNullOrEmpty(sprite.name))
                return -1;

            int underscore = sprite.name.LastIndexOf('_');
            if (underscore < 0 || underscore >= sprite.name.Length - 1)
                return -1;

            return int.TryParse(sprite.name.Substring(underscore + 1), out int parsed)
                ? parsed
                : -1;
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