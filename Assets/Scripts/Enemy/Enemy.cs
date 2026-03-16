using UnityEngine;
using S2RD.Core;
using System.Linq;

namespace S2RD.Enemy
{
    /// <summary>
    /// 일반 몬스터의 이동/피격 처리를 담당합니다.
    /// </summary>
    public class Enemy : MonoBehaviour
    {
        // ──── 애니메이션 타일 인덱스 (44열×24행, 32×32 px) ────
        // Idle Stand: tile_rows 2-5, 6프레임 (col 간격 6~7)
        private static readonly string[][] 몬스터아이들타일 =
        {
            new[] { "monster_clean_91",  "monster_clean_92",
                    "monster_clean_135", "monster_clean_136", "monster_clean_137",
                    "monster_clean_178", "monster_clean_179", "monster_clean_180", "monster_clean_181",
                    "monster_clean_222", "monster_clean_223", "monster_clean_224", "monster_clean_225" },
            new[] { "monster_clean_97",  "monster_clean_98",
                    "monster_clean_141", "monster_clean_142", "monster_clean_143",
                    "monster_clean_184", "monster_clean_185", "monster_clean_186", "monster_clean_187",
                    "monster_clean_228", "monster_clean_229", "monster_clean_230", "monster_clean_231" },
            new[] { "monster_clean_103", "monster_clean_104",
                    "monster_clean_147", "monster_clean_148", "monster_clean_149",
                    "monster_clean_190", "monster_clean_191", "monster_clean_192", "monster_clean_193",
                    "monster_clean_234", "monster_clean_235", "monster_clean_236", "monster_clean_237" },
        };

        // Walk: tile_rows 9-12, 6프레임 (col 간격 6~8)
        private static readonly string[][] 몬스터걷기타일 =
        {
            new[] { "monster_clean_399", "monster_clean_400",
                    "monster_clean_443", "monster_clean_444", "monster_clean_445",
                    "monster_clean_486", "monster_clean_487", "monster_clean_488", "monster_clean_489",
                    "monster_clean_530", "monster_clean_531", "monster_clean_532", "monster_clean_533" },
            new[] { "monster_clean_405", "monster_clean_406",
                    "monster_clean_449", "monster_clean_450", "monster_clean_451",
                    "monster_clean_492", "monster_clean_493", "monster_clean_494", "monster_clean_495",
                    "monster_clean_536", "monster_clean_537", "monster_clean_538", "monster_clean_539" },
            new[] { "monster_clean_411", "monster_clean_412",
                    "monster_clean_455", "monster_clean_456", "monster_clean_457",
                    "monster_clean_498", "monster_clean_499", "monster_clean_500", "monster_clean_501",
                    "monster_clean_542", "monster_clean_543", "monster_clean_544", "monster_clean_545" },
            new[] { "monster_clean_422", "monster_clean_423",
                    "monster_clean_466", "monster_clean_467", "monster_clean_468",
                    "monster_clean_509", "monster_clean_510", "monster_clean_511", "monster_clean_512",
                    "monster_clean_553", "monster_clean_554", "monster_clean_555", "monster_clean_556" },
            new[] { "monster_clean_428", "monster_clean_429",
                    "monster_clean_472", "monster_clean_473", "monster_clean_474",
                    "monster_clean_515", "monster_clean_516", "monster_clean_517", "monster_clean_518",
                    "monster_clean_559", "monster_clean_560", "monster_clean_561", "monster_clean_562" },
            new[] { "monster_clean_434", "monster_clean_435",
                    "monster_clean_478", "monster_clean_479", "monster_clean_480",
                    "monster_clean_521", "monster_clean_522", "monster_clean_523", "monster_clean_524",
                    "monster_clean_565", "monster_clean_566", "monster_clean_567", "monster_clean_568" },
        };

        protected float 최대체력;
        protected float 현재체력;
        protected float 이동속도;
        protected int 공격력;
        protected int 골드드랍값;

        private Vector3[] _경로포인트;
        private int _현재경로인덱스;
        private SpriteRenderer _renderer;
        private PixelSpriteAnimator _animator;

        public int 골드드랍 => 골드드랍값;
        public bool 사망여부 => 현재체력 <= 0f;
        public bool 경로종료도달여부 => _경로포인트 != null && _경로포인트.Length > 0 && _현재경로인덱스 >= _경로포인트.Length;

        public virtual void 초기화(float hp, float speed, int damage, int dropGold, Vector3[] pathPoints)
        {
            최대체력 = hp;
            현재체력 = hp;
            이동속도 = speed;
            공격력 = damage;
            골드드랍값 = dropGold;
            _경로포인트 = pathPoints;
            _현재경로인덱스 = 1;

            _renderer = GetComponent<SpriteRenderer>();
            if (_renderer == null)
                _renderer = gameObject.AddComponent<SpriteRenderer>();

            // ── 애니메이션 프레임 합성 (항상 실행) ──
            Sprite[] idleFrames = SlicedCharacterComposer.ComposeAnimationFrames(
                "monster_clean", 32f, "Monster_idle", 몬스터아이들타일);
            Sprite[] walkFrames = SlicedCharacterComposer.ComposeAnimationFrames(
                "monster_clean", 32f, "Monster_walk", 몬스터걷기타일);

            if (idleFrames != null && idleFrames.Length > 0)
            {
                _renderer.sprite = idleFrames[0];
                _animator = GetComponent<PixelSpriteAnimator>() ?? gameObject.AddComponent<PixelSpriteAnimator>();
                _animator.프레임설정(idleFrames, walkFrames ?? idleFrames, idleFrames);
                Debug.Log($"[Monster Anim] idle={idleFrames.Length}frames, walk={(walkFrames?.Length ?? 0)}frames");
            }
            else
            {
                // 폴백: 정적 스프라이트
                _renderer.sprite = 대체몬스터스프라이트가져오기(this is BossEnemy)
                    ?? 몬스터스프라이트생성();
            }

            _renderer.color = Color.white;
            _renderer.sortingOrder = 18;
            _renderer.drawMode = SpriteDrawMode.Simple;
            적용크기보정();

            if (_renderer.sprite != null)
                Debug.Log($"[Enemy Sprite Assigned] name={_renderer.sprite.name}, size={_renderer.sprite.rect.width}x{_renderer.sprite.rect.height}");
            else
                Debug.LogWarning("[Enemy Sprite Assigned] sprite is null after assignment.");
        }

        public virtual void 이동업데이트()
        {
            if (_경로포인트 == null || _경로포인트.Length == 0 || 사망여부)
                return;

            if (_현재경로인덱스 >= _경로포인트.Length)
            {
                if (_animator != null)
                    _animator.상태설정이동(false);
                return;
            }

            Vector3 target = _경로포인트[_현재경로인덱스];
            Vector3 toTarget = target - transform.position;

            // 수평 이동 방향에 따라 스프라이트 반전 (왼쪽 이동 시 flipX)
            if (_renderer != null && Mathf.Abs(toTarget.x) > 0.01f)
                _renderer.flipX = toTarget.x < 0f;

            float step = 이동속도 * Time.deltaTime;

            if (toTarget.sqrMagnitude <= step * step)
            {
                transform.position = target;
                _현재경로인덱스++;
                if (_animator != null)
                    _animator.상태설정이동(true);
                return;
            }

            transform.position += toTarget.normalized * step;
            if (_animator != null)
                _animator.상태설정이동(true);
        }

        public void 피해적용(float damage)
        {
            if (damage <= 0f || 사망여부)
                return;

            현재체력 = Mathf.Max(0f, 현재체력 - damage);
        }

        private static Sprite 몬스터스프라이트생성()
        {
            Sprite[] frames = PixelArtFactory.몬스터애니메이션프레임생성(new Color(0.88f, 0.22f, 0.25f, 1f), new Color(0.62f, 0.08f, 0.10f, 1f));
            return frames != null && frames.Length > 0 ? frames[0] : null;
        }

        private static Sprite 대체몬스터스프라이트가져오기(bool isBoss)
        {
            string path = isBoss ? "Prefabs/Enemies/enemy_BossDragon" : "Prefabs/Enemies/enemy_goblin";
            GameObject prefab = Resources.Load<GameObject>(path);
            if (prefab == null)
                return null;

            SpriteRenderer sr = prefab.GetComponent<SpriteRenderer>();
            return sr != null ? sr.sprite : null;
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

            const float targetWorldHeight = 1.1f;
            float scale = targetWorldHeight / spriteHeight;
            transform.localScale = new Vector3(scale, scale, 1f);
        }
    }
}
