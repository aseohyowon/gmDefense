// GoldSystem.cs
// 골드 관리 시스템 클래스.
// 골드 획득, 소비, 잔고 확인, 소환 비용 증가 로직을 담당한다.

using UnityEngine;

namespace GMDefense.Core
{
    /// <summary>
    /// 골드 시스템 매니저.
    /// 골드 잔고를 관리하고 골드 변경 이벤트를 발행한다.
    /// </summary>
    public class GoldSystem : MonoBehaviour
    {
        [Header("골드 초기 설정")]
        [Tooltip("게임 시작 시 초기 골드")]
        [SerializeField] private int 초기골드 = 200;

        [Tooltip("영웅 기본 소환 비용")]
        [SerializeField] private int 기본소환비용 = 100;

        [Tooltip("소환 횟수당 비용 증가량")]
        [SerializeField] private int 소환비용증가량 = 10;

        // 현재 골드
        private int _현재골드;
        // 현재 소환 비용 (호출 횟수에 따라 증가)
        private int _현재소환비용;
        // 총 소환 횟수
        private int _소환횟수 = 0;

        // 골드 변경 이벤트 (변경 후 골드 전달)
        public System.Action<int> 골드변경이벤트;

        private static GoldSystem _인스턴스;
        public static GoldSystem 인스턴스
        {
            get
            {
                if (_인스턴스 == null)
                    _인스턴스 = FindObjectOfType<GoldSystem>();
                return _인스턴스;
            }
        }

        private void Awake()
        {
            if (_인스턴스 == null)
                _인스턴스 = this;
            else if (_인스턴스 != this)
                Destroy(gameObject);

            _현재골드 = 초기골드;
            _현재소환비용 = 기본소환비용;
        }

        /// <summary>
        /// 골드를 추가한다.
        /// </summary>
        /// <param name="양">추가할 골드 양 (양수)</param>
        public void 골드추가(int 양)
        {
            if (양 <= 0) return;

            _현재골드 += 양;
            골드변경이벤트?.Invoke(_현재골드);
            Debug.Log($"[골드시스템] +{양} 골드. 현재: {_현재골드}");
        }

        /// <summary>
        /// 골드를 소비한다.
        /// </summary>
        /// <param name="양">소비할 골드 양 (양수)</param>
        /// <returns>소비 성공 여부</returns>
        public bool 골드소비(int 양)
        {
            if (양 <= 0) return false;

            if (_현재골드 < 양)
            {
                Debug.Log($"[골드시스템] 골드 부족! 필요: {양}, 보유: {_현재골드}");
                return false;
            }

            _현재골드 -= 양;
            골드변경이벤트?.Invoke(_현재골드);
            Debug.Log($"[골드시스템] -{양} 골드. 현재: {_현재골드}");
            return true;
        }

        /// <summary>
        /// 영웅 소환 비용을 지불하고 소환 횟수를 증가시킨다.
        /// </summary>
        /// <returns>소환 성공 여부</returns>
        public bool 소환비용지불()
        {
            if (!골드소비(_현재소환비용)) return false;

            _소환횟수++;
            _현재소환비용 = 기본소환비용 + _소환횟수 * 소환비용증가량;

            Debug.Log($"[골드시스템] 소환 완료. 다음 소환 비용: {_현재소환비용}");
            return true;
        }

        /// <summary>현재 골드 (읽기 전용)</summary>
        public int 현재골드 => _현재골드;

        /// <summary>현재 소환 비용 (읽기 전용)</summary>
        public int 현재소환비용 => _현재소환비용;

        /// <summary>골드가 충분한지 확인한다.</summary>
        public bool 골드충분(int 필요골드) => _현재골드 >= 필요골드;
    }
}
