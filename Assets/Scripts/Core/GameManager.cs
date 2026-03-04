// GameManager.cs
// 게임 전체의 상태와 흐름을 관리하는 핵심 매니저 클래스.
// 게임 시작, 일시정지, 종료, 플레이어 HP 관리를 담당한다.

using UnityEngine;
using GMDefense.Heroes;
using GMDefense.Systems;
using GMDefense.Enemy;

namespace GMDefense.Core
{
    /// <summary>
    /// 게임 상태 열거형.
    /// </summary>
    public enum 게임상태
    {
        준비중,     // 게임 시작 전
        진행중,     // 게임 진행 중
        일시정지,   // 일시정지
        게임오버,   // 게임 오버
        승리        // 플레이어 승리 (목표 달성)
    }

    /// <summary>
    /// 게임 매니저.
    /// 씬 내 싱글톤으로 동작하며, 게임 전반의 상태를 제어한다.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        [Header("게임 설정")]
        [Tooltip("플레이어 초기 HP")]
        [SerializeField] private int 초기플레이어HP = 20;

        [Header("영웅 소환 설정")]
        [Tooltip("소환 가능한 영웅 데이터 풀")]
        [SerializeField] private HeroData[] 소환가능영웅풀;

        [Tooltip("영웅 게임오브젝트 프리팹")]
        [SerializeField] private GameObject 영웅프리팹;

        // 현재 게임 상태
        private 게임상태 _현재상태 = 게임상태.준비중;
        // 현재 플레이어 HP
        private int _현재플레이어HP;

        // 게임 상태 변경 이벤트
        public System.Action<게임상태> 게임상태변경이벤트;
        // 플레이어 HP 변경 이벤트
        public System.Action<int, int> HP변경이벤트;  // (현재HP, 최대HP)

        private static GameManager _인스턴스;
        public static GameManager 인스턴스
        {
            get
            {
                if (_인스턴스 == null)
                    _인스턴스 = FindObjectOfType<GameManager>();
                return _인스턴스;
            }
        }

        private void Awake()
        {
            if (_인스턴스 == null)
                _인스턴스 = this;
            else if (_인스턴스 != this)
                Destroy(gameObject);
        }

        private void Start()
        {
            게임초기화();
        }

        /// <summary>
        /// 게임을 초기 상태로 설정한다.
        /// </summary>
        private void 게임초기화()
        {
            _현재플레이어HP = 초기플레이어HP;
            상태변경(게임상태.준비중);

            // EnemySpawner의 성문 도달 이벤트를 간접적으로 처리하기 위해
            // EnemyBase.성문도달이벤트는 각 적 생성 시 구독됨
            Debug.Log("[게임매니저] 게임 초기화 완료");
        }

        /// <summary>
        /// 게임을 시작한다. WaveManager에 게임 시작 신호를 보낸다.
        /// </summary>
        public void 게임시작()
        {
            if (_현재상태 != 게임상태.준비중 && _현재상태 != 게임상태.일시정지)
                return;

            상태변경(게임상태.진행중);
            WaveManager.인스턴스?.게임시작();
            Debug.Log("[게임매니저] 게임 시작!");
        }

        /// <summary>
        /// 게임을 일시정지한다.
        /// </summary>
        public void 일시정지()
        {
            if (_현재상태 != 게임상태.진행중) return;

            상태변경(게임상태.일시정지);
            Time.timeScale = 0f;
            Debug.Log("[게임매니저] 일시정지");
        }

        /// <summary>
        /// 일시정지를 해제한다.
        /// </summary>
        public void 일시정지해제()
        {
            if (_현재상태 != 게임상태.일시정지) return;

            상태변경(게임상태.진행중);
            Time.timeScale = 1f;
            Debug.Log("[게임매니저] 게임 재개");
        }

        /// <summary>
        /// 플레이어가 피해를 받는다.
        /// </summary>
        /// <param name="피해량">받을 피해량</param>
        public void 플레이어피해받기(int 피해량)
        {
            if (_현재상태 != 게임상태.진행중) return;

            _현재플레이어HP = Mathf.Max(0, _현재플레이어HP - 피해량);
            HP변경이벤트?.Invoke(_현재플레이어HP, 초기플레이어HP);

            Debug.Log($"[게임매니저] 플레이어 피해: -{피해량}. 남은 HP: {_현재플레이어HP}");

            if (_현재플레이어HP <= 0)
                게임오버처리();
        }

        /// <summary>
        /// 랜덤 영웅을 소환한다. 골드를 소비하고 빈 슬롯에 배치한다.
        /// </summary>
        public void 영웅소환()
        {
            if (_현재상태 != 게임상태.진행중)
            {
                Debug.Log("[게임매니저] 게임 진행 중이 아닙니다.");
                return;
            }

            if (소환가능영웅풀 == null || 소환가능영웅풀.Length == 0)
            {
                Debug.LogError("[게임매니저] 소환 가능한 영웅 데이터가 없습니다.");
                return;
            }

            // 빈 슬롯 확인
            var 빈슬롯 = GridSystem.인스턴스?.빈슬롯가져오기();
            if (빈슬롯 == null)
            {
                Debug.Log("[게임매니저] 빈 슬롯이 없습니다.");
                return;
            }

            // 골드 소비
            if (!GoldSystem.인스턴스.소환비용지불())
                return;

            // 랜덤 영웅 데이터 선택
            int 랜덤인덱스 = Random.Range(0, 소환가능영웅풀.Length);
            HeroData 선택데이터 = 소환가능영웅풀[랜덤인덱스];

            // 영웅 생성
            var 오브젝트 = Instantiate(영웅프리팹, 빈슬롯.위치, Quaternion.identity);
            var 새영웅 = 오브젝트.GetComponent<Hero>();
            새영웅?.초기화(선택데이터);

            // 슬롯에 배치
            GridSystem.인스턴스?.영웅배치(새영웅, 빈슬롯);

            // 시너지 재계산
            var 배치목록 = GridSystem.인스턴스?.배치된영웅목록가져오기();
            if (배치목록 != null)
                TraitSystem.인스턴스?.시너지재계산(배치목록);

            Debug.Log($"[게임매니저] '{선택데이터.영웅이름}' 소환 완료!");
        }

        /// <summary>
        /// 게임 오버를 처리한다.
        /// </summary>
        private void 게임오버처리()
        {
            상태변경(게임상태.게임오버);
            Time.timeScale = 0f;
            Debug.Log("[게임매니저] 게임 오버!");
        }

        /// <summary>
        /// 게임 상태를 변경하고 이벤트를 발행한다.
        /// </summary>
        private void 상태변경(게임상태 새상태)
        {
            _현재상태 = 새상태;
            게임상태변경이벤트?.Invoke(새상태);
        }

        /// <summary>현재 게임 상태 (읽기 전용)</summary>
        public 게임상태 현재상태 => _현재상태;

        /// <summary>현재 플레이어 HP (읽기 전용)</summary>
        public int 현재플레이어HP => _현재플레이어HP;

        /// <summary>최대 플레이어 HP (읽기 전용)</summary>
        public int 최대플레이어HP => 초기플레이어HP;
    }
}
