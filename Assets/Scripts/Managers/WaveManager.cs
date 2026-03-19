using UnityEngine;

namespace S2RD.Managers
{
    /// <summary>
    /// 자동 웨이브 증가와 웨이브 파생 수치를 관리합니다.
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        [SerializeField] private float 웨이브증가간격초 = 10f;

        private int _현재웨이브 = 1;
        private int _처치수;
        private float _다음웨이브시간;

        public int 현재웨이브 => _현재웨이브;
        public int 처치수 => _처치수;

        public event System.Action<int, int> 웨이브정보변경이벤트;
        public event System.Action<int> 웨이브변경이벤트;

        private void Awake()
        {
            _다음웨이브시간 = Time.time + 웨이브증가간격초;
            웨이브정보변경이벤트?.Invoke(_현재웨이브, _처치수);
        }

        private void Update()
        {
            if (Time.time < _다음웨이브시간)
                return;

            _현재웨이브++;
            _다음웨이브시간 = Time.time + 웨이브증가간격초;
            웨이브정보변경이벤트?.Invoke(_현재웨이브, _처치수);
            웨이브변경이벤트?.Invoke(_현재웨이브);
        }

        public void 처치카운트증가()
        {
            _처치수++;
            웨이브정보변경이벤트?.Invoke(_현재웨이브, _처치수);
        }

        public int 현재웨이브스폰수()
        {
            // 요구사항 고정값: Wave1=5, Wave2=8, Wave3=12
            if (_현재웨이브 == 1)
                return 5;
            if (_현재웨이브 == 2)
                return 8;
            if (_현재웨이브 == 3)
                return 12;

            // 이후 웨이브는 점진적으로 증가
            return 12 + ((_현재웨이브 - 3) * 4);
        }

        public float 몬스터체력배율()
        {
            // 웨이브 상승에 따라 체력 증가
            return 1f + ((_현재웨이브 - 1) * 0.25f);
        }

        public float 몬스터이동속도배율()
        {
            // 웨이브 상승에 따라 이동속도 증가
            return 1f + ((_현재웨이브 - 1) * 0.08f);
        }

        public bool 보스웨이브여부()
        {
            // 5, 10, 15 ... 웨이브마다 보스가 등장합니다.
            return _현재웨이브 % 5 == 0;
        }
    }
}
