using UnityEngine;

namespace S2RD.Systems
{
    /// <summary>
    /// 골드 자원과 소환 비용을 관리하는 시스템입니다.
    /// </summary>
    public class GoldSystem : MonoBehaviour
    {
        [SerializeField] private int 시작골드 = 200;
        [SerializeField] private int 소환비용 = 10;

        private int _현재골드;

        public int 현재골드 => _현재골드;
        public int 현재소환비용 => 소환비용;

        public event System.Action<int> 골드변경이벤트;

        private void Awake()
        {
            _현재골드 = 시작골드;
        }

        public bool 소환비용지불()
        {
            if (_현재골드 < 소환비용)
                return false;

            _현재골드 -= 소환비용;
            골드변경이벤트?.Invoke(_현재골드);
            return true;
        }

        public void 골드추가(int amount)
        {
            if (amount <= 0)
                return;

            _현재골드 += amount;
            골드변경이벤트?.Invoke(_현재골드);
        }
    }
}
