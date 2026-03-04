// GridSystem.cs
// 영웅 배치용 그리드 슬롯 시스템.
// 영웅 슬롯의 생성, 빈 슬롯 탐색, 영웅 배치/제거를 관리한다.

using System.Collections.Generic;
using UnityEngine;

namespace GMDefense.Heroes
{
    /// <summary>
    /// 개별 영웅 슬롯 정보를 나타내는 클래스.
    /// </summary>
    public class HeroSlot
    {
        /// <summary>슬롯 고유 인덱스</summary>
        public int 인덱스 { get; private set; }
        /// <summary>슬롯 월드 위치</summary>
        public Vector3 위치 { get; private set; }
        /// <summary>슬롯에 배치된 영웅 (없으면 null)</summary>
        public Hero 영웅 { get; set; }
        /// <summary>슬롯이 비어 있는지 여부</summary>
        public bool 비어있음 => 영웅 == null;

        public HeroSlot(int 인덱스, Vector3 위치)
        {
            this.인덱스 = 인덱스;
            this.위치 = 위치;
        }
    }

    /// <summary>
    /// 영웅 배치 그리드 시스템.
    /// 지정된 행/열 크기로 슬롯을 생성하고, 영웅 배치/이동을 담당한다.
    /// </summary>
    public class GridSystem : MonoBehaviour
    {
        [Header("그리드 설정")]
        [Tooltip("가로 열 수")]
        [SerializeField] private int 열수 = 4;

        [Tooltip("세로 행 수")]
        [SerializeField] private int 행수 = 3;

        [Tooltip("슬롯 간 간격")]
        [SerializeField] private float 슬롯간격 = 1.5f;

        [Tooltip("슬롯 시각화 프리팹 (선택 사항)")]
        [SerializeField] private GameObject 슬롯프리팹;

        // 전체 슬롯 목록
        private List<HeroSlot> _슬롯목록 = new List<HeroSlot>();

        private static GridSystem _인스턴스;
        public static GridSystem 인스턴스
        {
            get
            {
                if (_인스턴스 == null)
                    _인스턴스 = FindObjectOfType<GridSystem>();
                return _인스턴스;
            }
        }

        private void Awake()
        {
            if (_인스턴스 == null)
                _인스턴스 = this;
            else if (_인스턴스 != this)
                Destroy(gameObject);

            그리드생성();
        }

        /// <summary>
        /// 설정된 행/열 수에 맞게 슬롯을 생성하고 배치한다.
        /// </summary>
        private void 그리드생성()
        {
            _슬롯목록.Clear();

            // 그리드 중앙 정렬을 위한 오프셋 계산
            float 가로오프셋 = -(열수 - 1) * 슬롯간격 * 0.5f;
            float 세로오프셋 = -(행수 - 1) * 슬롯간격 * 0.5f;

            int 인덱스 = 0;
            for (int 행 = 0; 행 < 행수; 행++)
            {
                for (int 열 = 0; 열 < 열수; 열++)
                {
                    Vector3 슬롯위치 = transform.position + new Vector3(
                        가로오프셋 + 열 * 슬롯간격,
                        세로오프셋 + 행 * 슬롯간격,
                        0f
                    );

                    var 슬롯 = new HeroSlot(인덱스, 슬롯위치);
                    _슬롯목록.Add(슬롯);

                    // 슬롯 시각화 오브젝트 생성
                    if (슬롯프리팹 != null)
                    {
                        var 슬롯오브젝트 = Instantiate(슬롯프리팹, 슬롯위치, Quaternion.identity, transform);
                        슬롯오브젝트.name = $"슬롯_{인덱스}";
                    }

                    인덱스++;
                }
            }

            Debug.Log($"[그리드시스템] {열수}x{행수} 그리드 생성 완료. 총 슬롯 수: {_슬롯목록.Count}");
        }

        /// <summary>
        /// 비어 있는 첫 번째 슬롯을 반환한다.
        /// </summary>
        /// <returns>빈 슬롯, 없으면 null</returns>
        public HeroSlot 빈슬롯가져오기()
        {
            return _슬롯목록.Find(s => s.비어있음);
        }

        /// <summary>
        /// 지정된 인덱스의 슬롯을 반환한다.
        /// </summary>
        public HeroSlot 슬롯가져오기(int 인덱스)
        {
            if (인덱스 < 0 || 인덱스 >= _슬롯목록.Count) return null;
            return _슬롯목록[인덱스];
        }

        /// <summary>
        /// 영웅을 지정된 슬롯에 배치한다.
        /// </summary>
        /// <param name="영웅">배치할 영웅</param>
        /// <param name="슬롯">배치 대상 슬롯</param>
        /// <returns>배치 성공 여부</returns>
        public bool 영웅배치(Hero 영웅, HeroSlot 슬롯)
        {
            if (슬롯 == null || 영웅 == null) return false;
            if (!슬롯.비어있음) return false;

            슬롯.영웅 = 영웅;
            영웅.슬롯인덱스 = 슬롯.인덱스;
            영웅.transform.position = 슬롯.위치;

            Debug.Log($"[그리드시스템] 영웅 '{영웅.영웅데이터?.영웅이름}' → 슬롯 {슬롯.인덱스} 배치");
            return true;
        }

        /// <summary>
        /// 슬롯에서 영웅을 제거한다.
        /// </summary>
        public void 영웅제거(int 슬롯인덱스)
        {
            var 슬롯 = 슬롯가져오기(슬롯인덱스);
            if (슬롯 != null)
                슬롯.영웅 = null;
        }

        /// <summary>
        /// 현재 배치된 모든 영웅 목록을 반환한다.
        /// </summary>
        public List<Hero> 배치된영웅목록가져오기()
        {
            var 결과 = new List<Hero>();
            foreach (var 슬롯 in _슬롯목록)
            {
                if (슬롯.영웅 != null)
                    결과.Add(슬롯.영웅);
            }
            return 결과;
        }

        /// <summary>
        /// 월드 위치에서 가장 가까운 슬롯을 찾아 반환한다.
        /// </summary>
        public HeroSlot 가장가까운슬롯가져오기(Vector3 위치)
        {
            HeroSlot 결과 = null;
            float 최소거리 = float.MaxValue;

            foreach (var 슬롯 in _슬롯목록)
            {
                float 거리 = Vector3.Distance(위치, 슬롯.위치);
                if (거리 < 최소거리)
                {
                    최소거리 = 거리;
                    결과 = 슬롯;
                }
            }

            return 결과;
        }

        /// <summary>
        /// 전체 슬롯 수를 반환한다.
        /// </summary>
        public int 전체슬롯수 => _슬롯목록.Count;
    }
}
