using UnityEngine;

namespace S2RD.Managers
{
    /// <summary>
    /// 골드 자원을 관리하는 시스템입니다.
    /// 소환 비용 지불, 골드 획득, 현재 골드 조회를 담당합니다.
    /// </summary>
    public class GoldSystem : MonoBehaviour
    {
        [Header("골드 설정")]
        [Tooltip("게임 시작 시 지급되는 기본 골드")]
        [SerializeField] private int 시작골드 = 200;

        [Tooltip("소환 기본 비용")]
        [SerializeField] private int 기본소환비용 = 100;

        [Tooltip("소환 1회 후 증가 비용")]
        [SerializeField] private int 소환증가비용 = 10;

        private int _현재골드;
        private int _현재소환비용;
        private int _소환횟수;

        public event System.Action<int> 골드변경이벤트;

        public int 현재골드 => _현재골드;
        public int 현재소환비용 => _현재소환비용;

        private void Awake()
        {
            _현재골드 = 시작골드;
            _현재소환비용 = 기본소환비용;
            _소환횟수 = 0;
        }

        public bool 소환비용지불()
        {
            if (_현재골드 < _현재소환비용)
                return false;

            _현재골드 -= _현재소환비용;
            _소환횟수++;
            _현재소환비용 = 기본소환비용 + (_소환횟수 * 소환증가비용);
            골드변경이벤트?.Invoke(_현재골드);
            return true;
        }

        public void 골드획득(int amount)
        {
            if (amount <= 0)
                return;

            _현재골드 += amount;
            골드변경이벤트?.Invoke(_현재골드);
        }

        public bool 골드충분(int cost)
        {
            return _현재골드 >= cost;
        }
    }
}
