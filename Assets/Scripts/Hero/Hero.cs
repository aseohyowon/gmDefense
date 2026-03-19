using System.Collections.Generic;
using UnityEngine;
using S2RD.Enemy;
using System;
using S2RD.Core;
using System.Linq;

namespace S2RD.Hero
{
    /// <summary>
    /// 영웅 유닛의 자동 공격 로직을 담당합니다.
    /// </summary>
    public class Hero : MonoBehaviour
    {
        private const float 고정공격속도초 = 1f;
        private const float 고정공격범위 = 3f;

        // ──── 영웅 애니메이션 타일 (HeroArcher_clean.png, 32열×32행, 32×32 px) ────
        // Idle: tile_rows 2-8, 3프레임 (col 간격 4)
        private static readonly string[][] 아처아이들타일 =
        {
            new[] { // Frame 0 (사용자 제공)
                "HeroArcher_clean_66",
                "HeroArcher_clean_98",  "HeroArcher_clean_99",
                "HeroArcher_clean_130", "HeroArcher_clean_131", "HeroArcher_clean_132",
                "HeroArcher_clean_162", "HeroArcher_clean_163", "HeroArcher_clean_164",
                "HeroArcher_clean_194", "HeroArcher_clean_195", "HeroArcher_clean_196",
                "HeroArcher_clean_226", "HeroArcher_clean_227",
                "HeroArcher_clean_258", "HeroArcher_clean_259" },
            new[] { // Frame 1 (col +4)
                "HeroArcher_clean_70",
                "HeroArcher_clean_102", "HeroArcher_clean_103",
                "HeroArcher_clean_134", "HeroArcher_clean_135", "HeroArcher_clean_136",
                "HeroArcher_clean_166", "HeroArcher_clean_167", "HeroArcher_clean_168",
                "HeroArcher_clean_198", "HeroArcher_clean_199", "HeroArcher_clean_200",
                "HeroArcher_clean_230", "HeroArcher_clean_231",
                "HeroArcher_clean_262", "HeroArcher_clean_263" },
            new[] { // Frame 2 (col +8)
                "HeroArcher_clean_74",
                "HeroArcher_clean_106", "HeroArcher_clean_107",
                "HeroArcher_clean_138", "HeroArcher_clean_139", "HeroArcher_clean_140",
                "HeroArcher_clean_170", "HeroArcher_clean_171", "HeroArcher_clean_172",
                "HeroArcher_clean_202", "HeroArcher_clean_203", "HeroArcher_clean_204",
                "HeroArcher_clean_234", "HeroArcher_clean_235",
                "HeroArcher_clean_266", "HeroArcher_clean_267" },
        };

        // Walk: tile_rows 22-28 (scan band 4-12), 3프레임 (col 간격 4)
        private static readonly string[][] 아처걷기타일 =
        {
            new[] { // Frame 0
                "HeroArcher_clean_706",
                "HeroArcher_clean_738", "HeroArcher_clean_739",
                "HeroArcher_clean_770", "HeroArcher_clean_771", "HeroArcher_clean_772",
                "HeroArcher_clean_802", "HeroArcher_clean_803", "HeroArcher_clean_804",
                "HeroArcher_clean_834", "HeroArcher_clean_835", "HeroArcher_clean_836",
                "HeroArcher_clean_866", "HeroArcher_clean_867",
                "HeroArcher_clean_898", "HeroArcher_clean_899" },
            new[] { // Frame 1 (col+4)
                "HeroArcher_clean_710",
                "HeroArcher_clean_742", "HeroArcher_clean_743",
                "HeroArcher_clean_774", "HeroArcher_clean_775", "HeroArcher_clean_776",
                "HeroArcher_clean_806", "HeroArcher_clean_807", "HeroArcher_clean_808",
                "HeroArcher_clean_838", "HeroArcher_clean_839", "HeroArcher_clean_840",
                "HeroArcher_clean_870", "HeroArcher_clean_871",
                "HeroArcher_clean_902", "HeroArcher_clean_903" },
            new[] { // Frame 2 (col+8)
                "HeroArcher_clean_714",
                "HeroArcher_clean_746", "HeroArcher_clean_747",
                "HeroArcher_clean_778", "HeroArcher_clean_779", "HeroArcher_clean_780",
                "HeroArcher_clean_810", "HeroArcher_clean_811", "HeroArcher_clean_812",
                "HeroArcher_clean_842", "HeroArcher_clean_843", "HeroArcher_clean_844",
                "HeroArcher_clean_874", "HeroArcher_clean_875",
                "HeroArcher_clean_906", "HeroArcher_clean_907" },
        };

        // 아처서있는합성슬라이스 = 아이들 frame 0 (단일 스프라이트 합성용)
        private static readonly string[] 아처서있는합성슬라이스 = 아처아이들타일[0];

        private HeroData _data;
        private float _다음공격가능시간;
        private SpriteRenderer _renderer;
        private PixelSpriteAnimator _animator;
        private Animator _animatorController;
        private bool _드래그중;

        public HeroData 데이터 => _data;
        public int 레벨 => _data.Level;
        public float 공격력 => _data.AttackPower;
        public float 공격속도 => _data.AttackInterval;
        public HeroGrade 등급 => _data.Grade;

        public event Action<Hero, Hero> 드래그해제이벤트;

        public void 초기화(HeroData data)
        {
            _data = data;
            _data.AttackInterval = 고정공격속도초;
            _data.AttackRange = 고정공격범위;

            _renderer = GetComponent<SpriteRenderer>();
            if (_renderer == null)
                _renderer = gameObject.AddComponent<SpriteRenderer>();

            _animatorController = GetComponent<Animator>();
            bool hasAnimatorVisual = _animatorController != null && _animatorController.runtimeAnimatorController != null;

            if (!hasAnimatorVisual)
            {
                // ── 슬라이스 애니메이션 합성 ──
                Sprite[] idleFrames = SlicedCharacterComposer.ComposeAnimationFrames(
                    "HeroArcher_clean", 32f, "Hero_idle", 아처아이들타일);
                Sprite[] walkFrames = SlicedCharacterComposer.ComposeAnimationFrames(
                    "HeroArcher_clean", 32f, "Hero_walk", 아처걷기타일);

                if (idleFrames != null && idleFrames.Length > 0)
                {
                    _renderer.sprite = idleFrames[0];
                    _animator = GetComponent<PixelSpriteAnimator>() ?? gameObject.AddComponent<PixelSpriteAnimator>();
                    // 공격 프레임 = walk 프레임을 핑퐁(forward→reverse)으로 재생 → 자연스러운 스윙
                    _animator.프레임설정(idleFrames, walkFrames ?? idleFrames, walkFrames ?? idleFrames);
                    Debug.Log($"[Hero Anim] idle={idleFrames.Length}frames, walk={(walkFrames?.Length ?? 0)}frames");
                }
                else
                {
                    // 폴백: 정적 합성 스프라이트
                    Sprite cleanHero = 대체영웅스프라이트가져오기();
                    if (cleanHero != null) _renderer.sprite = cleanHero;

                    if (_renderer.sprite == null)
                    {
                        Sprite[] frames = 영웅프레임생성(_data.Grade, _data.Type);
                        _renderer.sprite = frames != null && frames.Length > 0 ? frames[0] : 기본사각스프라이트생성();
                        _animator = GetComponent<PixelSpriteAnimator>() ?? gameObject.AddComponent<PixelSpriteAnimator>();
                        _animator.프레임설정(frames, frames, frames);
                    }
                }
            }

            _renderer.sortingOrder = 20;
            _renderer.color = Color.white;
            _renderer.drawMode = SpriteDrawMode.Simple;
            적용크기보정();

            if (_renderer.sprite != null)
                Debug.Log($"[Hero Sprite Assigned] name={_renderer.sprite.name}");
            else
                Debug.LogWarning("[Hero Sprite Assigned] sprite is null after assignment.");

            BoxCollider2D collider = GetComponent<BoxCollider2D>();
            if (collider == null)
                collider = gameObject.AddComponent<BoxCollider2D>();

            collider.size = 계산콜라이더크기();
        }

        public void 자동공격업데이트(List<Enemy.Enemy> enemies)
        {
            if (enemies == null || enemies.Count == 0)
                return;

            if (Time.time < _다음공격가능시간)
                return;

            Enemy.Enemy target = 가장가까운적찾기(enemies);
            if (target == null)
                return;

            target.피해적용(_data.AttackPower);
            if (_animatorController != null)
                _animatorController.SetTrigger("Attack");
            else if (_animator != null)
                _animator.공격애니메이션재생();
            StartCoroutine(공격시각효과(target.transform.position));
            _다음공격가능시간 = Time.time + 고정공격속도초;
        }

        private System.Collections.IEnumerator 공격시각효과(Vector3 targetPos)
        {
            if (_renderer == null) yield break;

            // 스케일 펰치 제거 → 위치 기반 굴진으로 교체 (픽셀 아트 깨짐 없음)
            Vector3 원래위치 = transform.position;
            Vector3 방향 = (targetPos - 원래위치);
            방향.y = 0f;
            Vector3 런지벡터 = 방향.normalized * 0.18f;   // 적쪽으로 0.18유닛 전진

            // 순간 하얀 플래시 (스케일 무변경, 색상만)
            _renderer.color = new Color(1f, 1f, 0.7f, 1f);

            // 가속 전진 (0.06초)
            float elapsed = 0f;
            while (elapsed < 0.06f)
            {
                transform.position = Vector3.Lerp(원래위치, 원래위치 + 런지벡터, elapsed / 0.06f);
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.position = 원래위치 + 런지벡터;
            _renderer.color = Color.white;

            // 감속 복귀 (0.1초)
            elapsed = 0f;
            while (elapsed < 0.1f)
            {
                transform.position = Vector3.Lerp(원래위치 + 런지벡터, 원래위치, elapsed / 0.1f);
                elapsed += Time.deltaTime;
                yield return null;
            }
            transform.position = 원래위치;
        }

        private Enemy.Enemy 가장가까운적찾기(List<Enemy.Enemy> enemies)
        {
            Enemy.Enemy nearest = null;
            float minDistance = float.MaxValue;

            for (int i = 0; i < enemies.Count; i++)
            {
                Enemy.Enemy enemy = enemies[i];
                if (enemy == null)
                    continue;

                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                if (distance <= _data.AttackRange && distance < minDistance)
                {
                    minDistance = distance;
                    nearest = enemy;
                }
            }

            return nearest;
        }

        private static Sprite 기본사각스프라이트생성()
        {
            Texture2D texture = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            texture.SetPixel(0, 0, Color.white);
            texture.filterMode = FilterMode.Point;
            texture.Apply();
            return Sprite.Create(texture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f), 1f);
        }

        private static Color 등급색상(HeroGrade grade)
        {
            return grade switch
            {
                HeroGrade.Rare => new Color(0.33f, 0.64f, 0.95f, 1f),
                HeroGrade.Epic => new Color(0.74f, 0.36f, 0.94f, 1f),
                HeroGrade.Legendary => new Color(0.98f, 0.79f, 0.21f, 1f),
                _ => new Color(0.88f, 0.88f, 0.88f, 1f)
            };
        }

        private static Sprite[] 영웅프레임생성(HeroGrade grade, HeroType type)
        {
            Color 주색 = grade switch
            {
                HeroGrade.Rare => new Color(0.26f, 0.55f, 0.93f, 1f),
                HeroGrade.Epic => new Color(0.66f, 0.32f, 0.90f, 1f),
                HeroGrade.Legendary => new Color(0.95f, 0.72f, 0.17f, 1f),
                _ => new Color(0.78f, 0.78f, 0.80f, 1f)
            };

            Color 보조색 = type switch
            {
                HeroType.Knight => new Color(0.30f, 0.35f, 0.40f, 1f),
                HeroType.Mage => new Color(0.12f, 0.50f, 0.82f, 1f),
                HeroType.Lancer => new Color(0.46f, 0.34f, 0.24f, 1f),
                _ => new Color(0.18f, 0.56f, 0.24f, 1f)
            };

            return PixelArtFactory.영웅애니메이션프레임생성(주색, 보조색);
        }

        private static Sprite 테스트영웅스프라이트가져오기()
        {
            Sprite[] sliced = 리소스슬라이스프레임가져오기("HeroArcher_clean", true);
            if (sliced.Length > 0)
                return sliced[0];
            Debug.LogError("[Hero] HeroArcher_clean 슬라이스 프레임을 찾지 못했습니다. S2RD > 클린 투명 PNG 생성을 먼저 실행하세요.");
            return null;
        }

        private void 테스트영웅애니메이션보장(Sprite fallbackSprite)
        {
            테스트영웅프레임세트초기화(out Sprite[] idleFrames, out Sprite[] attackFrames);

            Sprite[] idle = (idleFrames != null && idleFrames.Length > 0)
                ? idleFrames
                : new[] { fallbackSprite };

            Sprite[] attack = (attackFrames != null && attackFrames.Length > 0)
                ? attackFrames
                : idle;

            _animator = GetComponent<PixelSpriteAnimator>();
            if (_animator == null)
                _animator = gameObject.AddComponent<PixelSpriteAnimator>();

            _animator.프레임설정(idle, idle, attack);
        }

        private static void 테스트영웅프레임세트초기화(out Sprite[] idleFrames, out Sprite[] attackFrames)
        {
            Sprite[] frames = 리소스슬라이스프레임가져오기("HeroArcher_clean", true);
            if (frames.Length > 0)
            {
                int idleCount = Mathf.Clamp(frames.Length >= 6 ? 3 : Mathf.Max(1, frames.Length / 2), 1, frames.Length);
                int attackStart = Mathf.Min(idleCount, frames.Length - 1);
                int attackCount = Mathf.Max(1, frames.Length - attackStart);

                idleFrames = new Sprite[idleCount];
                for (int i = 0; i < idleCount; i++)
                    idleFrames[i] = frames[i];

                attackFrames = new Sprite[attackCount];
                for (int i = 0; i < attackCount; i++)
                    attackFrames[i] = frames[attackStart + i];
                return;
            }

            idleFrames = System.Array.Empty<Sprite>();
            attackFrames = System.Array.Empty<Sprite>();
        }

        private static Sprite[] 리소스슬라이스프레임가져오기(string resourceKey, bool topHalfOnly)
        {
            Sprite[] all = Resources.LoadAll<Sprite>(resourceKey);
            if (all == null || all.Length == 0)
                return System.Array.Empty<Sprite>();

            Sprite[] ordered = all
                .Where(s => s != null)
                .OrderBy(스프라이트인덱스파싱)
                .ToArray();

            if (!topHalfOnly)
                return ordered;

            int half = Mathf.Max(1, ordered.Length / 2);
            Sprite[] top = new Sprite[half];
            for (int i = 0; i < half; i++)
                top[i] = ordered[i];
            return top;
        }

        private static int 스프라이트인덱스파싱(Sprite sprite)
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

        private static Sprite 대체영웅스프라이트가져오기()
        {
            Sprite composed = SlicedCharacterComposer.ComposeFromNamedSprites(
                "HeroArcher_clean",
                "HeroArcher_composed_stand",
                32f,
                아처서있는합성슬라이스);
            if (composed != null)
                return composed;

            Sprite[] clean = Resources.LoadAll<Sprite>("HeroArcher_clean");
            if (clean != null && clean.Length > 0)
                return 첫가시프레임선택(clean.OrderBy(스프라이트인덱스파싱).ToArray());

            return null;
        }

        private static Sprite 전체텍스처스프라이트생성(Texture2D texture, string name)
        {
            if (texture == null)
                return null;

            Rect rect = new Rect(0f, 0f, texture.width, texture.height);
            Vector2 pivot = new Vector2(0.5f, 0.5f);
            Sprite sprite = Sprite.Create(texture, rect, pivot, 32f, 0, SpriteMeshType.FullRect);
            sprite.name = name;
            return sprite;
        }

        private static Sprite 첫가시프레임선택(Sprite[] sprites)
        {
            if (sprites == null || sprites.Length == 0)
                return null;

            for (int i = 0; i < sprites.Length; i++)
            {
                Sprite s = sprites[i];
                if (s == null)
                    continue;

                if (가시스프라이트여부(s))
                    return s;
            }

            return sprites[0];
        }

        private static bool 가시스프라이트여부(Sprite sprite)
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

        private void 적용크기보정()
        {
            if (_renderer == null || _renderer.sprite == null)
            {
                transform.localScale = new Vector3(1f, 1f, 1f);
                return;
            }

            float spriteHeight = _renderer.sprite.bounds.size.y;
            if (spriteHeight <= 0.0001f)
            {
                transform.localScale = new Vector3(1f, 1f, 1f);
                return;
            }

            // X=Y 동일 스케일 → 픽셀이 정사각형 유지 (1.4f 제거로 픽셀 깨짐 수정)
            const float targetWorldHeight = 1.4f;
            float uniformScale = targetWorldHeight / spriteHeight;
            transform.localScale = new Vector3(uniformScale, uniformScale, 1f);
        }

        private Vector2 계산콜라이더크기()
        {
            if (_renderer == null || _renderer.sprite == null)
                return new Vector2(0.8f, 0.8f);

            // BoxCollider2D.size는 로컬 공간 — 물리엔진이 localScale을 자동 적용하므로
            // 여기서 localScale을 곱하면 이중 축소되어 콜라이더가 너무 작아짐
            Vector2 baseSize = _renderer.sprite.bounds.size;
            return new Vector2(
                Mathf.Max(0.5f, baseSize.x * 0.7f),
                Mathf.Max(0.5f, baseSize.y * 0.8f));
        }

        public void 드래그시작()
        {
            _드래그중 = true;
        }

        public void 드래그이동(Vector3 worldPos)
        {
            if (!_드래그중)
                return;

            transform.position = new Vector3(worldPos.x, worldPos.y, 0f);
        }

        public void 드래그종료()
        {
            if (!_드래그중)
                return;

            _드래그중 = false;
            Hero target = 찾기겹친영웅();
            if (target != null)
                드래그해제이벤트?.Invoke(this, target);
        }

        private Hero 찾기겹친영웅()
        {
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 0.35f);
            for (int i = 0; i < hits.Length; i++)
            {
                Hero hero = hits[i].GetComponent<Hero>();
                if (hero != null && hero != this)
                    return hero;
            }

            return null;
        }
    }
}
