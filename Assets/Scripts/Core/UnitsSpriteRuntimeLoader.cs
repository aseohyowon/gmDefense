using System.Collections.Generic;
using UnityEngine;

namespace S2RD.Core
{
    public static class UnitsSpriteRuntimeLoader
    {
        private const string ResourcePath = "units_sprite_sheet";

        // 알려주신 캐릭터별 시작 인덱스 (머리 상단 왼쪽 기준)
        // 각 캐릭터는 보통 4프레임씩 사용하므로 시작번호부터 4개씩 잡았습니다.
        private static readonly int[] ArcherIndices = { 82, 83, 84, 85 };
        private static readonly int[] ArcherIdleIndices = { 82, 83, 84 };
        private static readonly int[] ArcherMoveIndices = { 82, 83, 84, 85 };
        private static readonly int[] ArcherAttackIndices = { 86, 87, 88, 89 };
        private static readonly int[] KnightIndices = { 238, 239, 240, 241 };
        private static readonly int[] RogueIndices = { 550, 551, 552, 553 };
        private static readonly int[] BardIndices = { 706, 707, 708, 709 };
        private static readonly int[] MonkIndices = { 862, 863, 864, 865 };

        // 몬스터 인덱스
        private static readonly int[] SkeletonIndices = { 256, 257, 258, 259 };
        private static readonly int[] ZombieIndices = { 412, 413, 414, 415 };
        private static readonly int[] BirdIndices = { 568, 569, 570, 571 };
        private static readonly int[] CowIndices = { 685, 686, 687, 688 };
        private static readonly int[] BossDragonIndices = { 919, 920, 921, 922 };

        private static Sprite[] _loadedSprites;
        private static readonly Dictionary<int, Sprite> _spriteBySheetIndex = new Dictionary<int, Sprite>();
        private static bool _initialized;

        private static void EnsureInitialized()
        {
            if (_initialized) return;
            
            _loadedSprites = Resources.LoadAll<Sprite>(ResourcePath);
            _spriteBySheetIndex.Clear();
            
            if (_loadedSprites == null || _loadedSprites.Length == 0)
            {
                Debug.LogError($"[Loader] '{ResourcePath}' 로드 실패! Resources 폴더와 파일명을 확인하세요.");
            }
            else
            {
                Debug.Log($"[Loader] 총 {_loadedSprites.Length}개의 조각을 성공적으로 로드했습니다.");

                for (int i = 0; i < _loadedSprites.Length; i++)
                {
                    Sprite sprite = _loadedSprites[i];
                    if (sprite == null)
                        continue;

                    int parsedIndex = 스프라이트인덱스가져오기(sprite);
                    if (parsedIndex < 0)
                        continue;

                    if (!_spriteBySheetIndex.ContainsKey(parsedIndex))
                        _spriteBySheetIndex.Add(parsedIndex, sprite);
                }

                Debug.Log($"[Loader] 인덱스 매핑 완료: {_spriteBySheetIndex.Count}개");
            }
            _initialized = true;
        }

        public static Sprite 랜덤영웅프레임가져오기(string type = "")
        {
            EnsureInitialized();
            List<int> allHeroes = new List<int>();

            string key = string.IsNullOrWhiteSpace(type) ? string.Empty : type.ToLowerInvariant();
            if (key.Contains("archer"))
                allHeroes.AddRange(ArcherIndices);
            else if (key.Contains("warrior") || key.Contains("knight"))
                allHeroes.AddRange(KnightIndices);
            else if (key.Contains("rogue") || key.Contains("assassin"))
                allHeroes.AddRange(RogueIndices);
            else if (key.Contains("bard"))
                allHeroes.AddRange(BardIndices);
            else if (key.Contains("monk"))
                allHeroes.AddRange(MonkIndices);

            if (allHeroes.Count == 0)
            {
                allHeroes.AddRange(ArcherIndices);
                allHeroes.AddRange(KnightIndices);
                allHeroes.AddRange(RogueIndices);
                allHeroes.AddRange(BardIndices);
                allHeroes.AddRange(MonkIndices);
            }

            int randomIndex = allHeroes[Random.Range(0, allHeroes.Count)];
            return GetSpriteSafe(randomIndex);
        }

        public static Sprite[] 영웅애니메이션프레임가져오기(string type, string state)
        {
            EnsureInitialized();

            string typeKey = string.IsNullOrWhiteSpace(type) ? string.Empty : type.ToLowerInvariant();
            string stateKey = string.IsNullOrWhiteSpace(state) ? "idle" : state.ToLowerInvariant();

            int[] indices = ArcherIdleIndices;
            if (typeKey.Contains("archer"))
            {
                if (stateKey.Contains("attack"))
                    indices = ArcherAttackIndices;
                else if (stateKey.Contains("move") || stateKey.Contains("walk"))
                    indices = ArcherMoveIndices;
                else
                    indices = ArcherIdleIndices;
            }

            return 인덱스배열로프레임가져오기(indices);
        }

        public static Sprite[] 궁수프레임가져오기(bool attack)
        {
            EnsureInitialized();
            return 인덱스배열로프레임가져오기(attack ? ArcherAttackIndices : ArcherIdleIndices);
        }

        public static Sprite 랜덤적프레임가져오기(string type = "")
        {
            EnsureInitialized();
            List<int> allEnemies = new List<int>();

            string key = string.IsNullOrWhiteSpace(type) ? string.Empty : type.ToLowerInvariant();
            if (key.Contains("boss"))
                allEnemies.AddRange(BossDragonIndices);
            else if (key.Contains("skeleton"))
                allEnemies.AddRange(SkeletonIndices);
            else if (key.Contains("zombie"))
                allEnemies.AddRange(ZombieIndices);
            else if (key.Contains("bird"))
                allEnemies.AddRange(BirdIndices);
            else if (key.Contains("cow") || key.Contains("orc") || key.Contains("goblin") || key.Contains("slime"))
                allEnemies.AddRange(CowIndices);

            if (allEnemies.Count == 0)
            {
                allEnemies.AddRange(SkeletonIndices);
                allEnemies.AddRange(ZombieIndices);
                allEnemies.AddRange(BirdIndices);
                allEnemies.AddRange(CowIndices);
            }
            
            int randomIndex = allEnemies[Random.Range(0, allEnemies.Count)];
            return GetSpriteSafe(randomIndex);
        }

        public static Sprite 보스프레임가져오기()
        {
            EnsureInitialized();
            int randomIndex = BossDragonIndices[Random.Range(0, BossDragonIndices.Length)];
            return GetSpriteSafe(randomIndex);
        }

        public static bool 스프라이트유효성검사(Sprite sprite, out string reason)
        {
            if (sprite == null)
            {
                reason = "sprite is null";
                return false;
            }

            reason = string.Empty;
            return true;
        }

        public static bool 유효캐릭터프레임여부(Sprite sprite)
        {
            return sprite != null;
        }

        public static int 스프라이트인덱스가져오기(Sprite sprite)
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

        private static Sprite GetSpriteSafe(int index)
        {
            if (_loadedSprites == null || _loadedSprites.Length == 0)
            {
                Debug.LogWarning($"[Loader] 스프라이트 로드 실패 상태에서 요청됨: {index}");
                return null;
            }

            if (!_spriteBySheetIndex.TryGetValue(index, out Sprite s) || s == null)
            {
                Debug.LogWarning($"[Loader] 시트 인덱스 매핑 실패: {index}");
                return null;
            }
            
            Debug.Log($"[Loader] 스프라이트 할당 성공: {s.name} (Index: {index})");
            return s;
        }

        private static Sprite[] 인덱스배열로프레임가져오기(int[] indices)
        {
            if (indices == null || indices.Length == 0)
                return System.Array.Empty<Sprite>();

            List<Sprite> frames = new List<Sprite>(indices.Length);
            for (int i = 0; i < indices.Length; i++)
            {
                Sprite sprite = GetSpriteSafe(indices[i]);
                if (sprite != null)
                    frames.Add(sprite);
            }

            return frames.ToArray();
        }
    }
}