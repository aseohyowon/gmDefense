// 시너지로직.cs
// TraitSystem.cs의 시너지 계산 로직을 Unity 없이 테스트하기 위한 클래스.
// 영웅 특성 조합에 따라 시너지 발동 여부와 배율을 계산한다.

namespace GMDefense.Tests.GameLogic
{
    /// <summary>
    /// 영웅 특성 종류 열거형. HeroData.cs의 TraitType과 동일.
    /// </summary>
    public enum 특성종류
    {
        없음,
        빛의수호자,
        어둠의사냥꾼,
        자연의수호,
        불꽃전사,
        얼음마법사,
        번개술사,
        강철기사
    }

    /// <summary>
    /// 시너지 단계 데이터. TraitSystem.cs의 시너지데이터 구조체와 동일.
    /// </summary>
    public class 시너지단계데이터
    {
        /// <summary>시너지 발동에 필요한 최소 영웅 수</summary>
        public int 필요영웅수 { get; set; }
        /// <summary>공격력 배율 보너스 (1.0 = 변화없음)</summary>
        public float 공격력배율 { get; set; } = 1f;
        /// <summary>공격속도 배율 보너스 (1.0 = 변화없음)</summary>
        public float 공격속도배율 { get; set; } = 1f;
    }

    /// <summary>
    /// 특성별 시너지 설정.
    /// </summary>
    public class 특성시너지설정
    {
        /// <summary>특성 종류</summary>
        public 특성종류 특성 { get; set; }
        /// <summary>시너지 단계 목록 (필요 영웅수 오름차순)</summary>
        public List<시너지단계데이터> 단계목록 { get; set; } = new();
    }

    /// <summary>
    /// Unity 의존성 없는 시너지 로직 계산 클래스.
    /// TraitSystem.cs의 시너지 계산 로직을 추출한 것.
    /// </summary>
    public class 시너지로직
    {
        // 특성별 시너지 설정 목록
        private readonly List<특성시너지설정> _설정목록;

        // 현재 배치된 영웅들의 특성 집계 결과
        private readonly Dictionary<특성종류, int> _활성시너지 = new();

        /// <summary>
        /// 시너지 로직을 초기화한다.
        /// </summary>
        /// <param name="설정목록">특성별 시너지 설정 목록</param>
        public 시너지로직(List<특성시너지설정>? 설정목록 = null)
        {
            _설정목록 = 설정목록 ?? new List<특성시너지설정>();
        }

        /// <summary>
        /// 영웅 특성 목록들을 받아 시너지를 재계산한다.
        /// TraitSystem.cs의 시너지재계산() 메서드와 동일한 로직.
        /// </summary>
        /// <param name="영웅특성목록들">각 영웅의 특성 배열 목록</param>
        public void 시너지재계산(IEnumerable<특성종류[]> 영웅특성목록들)
        {
            _활성시너지.Clear();

            foreach (var 특성목록 in 영웅특성목록들)
            {
                foreach (var 특성 in 특성목록)
                {
                    if (특성 == 특성종류.없음) continue;

                    if (!_활성시너지.ContainsKey(특성))
                        _활성시너지[특성] = 0;

                    _활성시너지[특성]++;
                }
            }
        }

        /// <summary>
        /// 특정 특성의 현재 영웅 수를 반환한다.
        /// </summary>
        public int 특성영웅수가져오기(특성종류 특성)
            => _활성시너지.TryGetValue(특성, out int 수) ? 수 : 0;

        /// <summary>
        /// 현재 활성화된 시너지에 따른 전체 공격력 배율을 반환한다.
        /// TraitSystem.cs의 전체공격력배율가져오기()와 동일한 로직.
        /// </summary>
        public float 전체공격력배율가져오기()
        {
            float 배율 = 1f;

            foreach (var 쌍 in _활성시너지)
            {
                var 설정 = _설정목록.Find(s => s.특성 == 쌍.Key);
                if (설정 == null) continue;

                foreach (var 단계 in 설정.단계목록)
                {
                    if (쌍.Value >= 단계.필요영웅수)
                        배율 *= 단계.공격력배율;
                }
            }

            return 배율;
        }

        /// <summary>
        /// 현재 활성화된 시너지에 따른 전체 공격속도 배율을 반환한다.
        /// TraitSystem.cs의 전체공격속도배율가져오기()와 동일한 로직.
        /// </summary>
        public float 전체공격속도배율가져오기()
        {
            float 배율 = 1f;

            foreach (var 쌍 in _활성시너지)
            {
                var 설정 = _설정목록.Find(s => s.특성 == 쌍.Key);
                if (설정 == null) continue;

                foreach (var 단계 in 설정.단계목록)
                {
                    if (쌍.Value >= 단계.필요영웅수)
                        배율 *= 단계.공격속도배율;
                }
            }

            return 배율;
        }

        /// <summary>
        /// 현재 활성화된 모든 시너지 목록을 반환한다.
        /// </summary>
        public IReadOnlyDictionary<특성종류, int> 활성시너지목록 => _활성시너지;
    }
}
