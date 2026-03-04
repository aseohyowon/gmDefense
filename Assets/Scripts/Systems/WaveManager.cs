// WaveManager.cs
// 웨이브 시스템을 관리하는 클래스.
// 웨이브 진행, 타이머, 보스 스폰 여부, 난이도 스케일링을 담당한다.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GMDefense.Enemy;

namespace GMDefense.Systems
{
    /// <summary>
    /// 웨이브 설정 데이터 구조체. 각 웨이브마다 스폰할 적 정보를 담는다.
    /// </summary>
    [System.Serializable]
    public struct 웨이브설정
    {
        [Tooltip("이 웨이브에 스폰할 적 목록")]
        public List<스폰데이터> 스폰목록;

        [Tooltip("웨이브 보상 골드")]
        public int 보상골드;
    }

    /// <summary>
    /// 웨이브 매니저.
    /// 일정 시간마다 웨이브를 시작하고, 5웨이브마다 보스를 스폰한다.
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        [Header("웨이브 설정")]
        [Tooltip("웨이브 시작 전 대기 시간 (초)")]
        [SerializeField] private float 웨이브대기시간 = 10f;

        [Tooltip("미리 설정된 웨이브 목록 (설정이 없으면 자동 생성)")]
        [SerializeField] private List<웨이브설정> 웨이브설정목록 = new List<웨이브설정>();

        [Tooltip("보스 웨이브 주기 (N웨이브마다 보스 등장)")]
        [SerializeField] private int 보스웨이브주기 = 5;

        [Tooltip("일반 적 프리팹")]
        [SerializeField] private GameObject 일반적프리팹;

        [Tooltip("보스 적 프리팹")]
        [SerializeField] private GameObject 보스적프리팹;

        // 현재 웨이브 번호 (1부터 시작)
        private int _현재웨이브 = 0;
        // 게임이 진행 중인지 여부
        private bool _진행중 = false;
        // 다음 웨이브까지 남은 시간
        private float _남은대기시간;

        // 웨이브 변경 이벤트 (현재 웨이브 번호 전달)
        public System.Action<int> 웨이브변경이벤트;
        // 보스 웨이브 이벤트
        public System.Action 보스웨이브이벤트;

        private static WaveManager _인스턴스;
        public static WaveManager 인스턴스
        {
            get
            {
                if (_인스턴스 == null)
                    _인스턴스 = FindObjectOfType<WaveManager>();
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
            // EnemySpawner 클리어 이벤트 구독
            if (EnemySpawner.인스턴스 != null)
                EnemySpawner.인스턴스.웨이브클리어이벤트 += 웨이브클리어처리;
        }

        private void OnDestroy()
        {
            if (EnemySpawner.인스턴스 != null)
                EnemySpawner.인스턴스.웨이브클리어이벤트 -= 웨이브클리어처리;
        }

        /// <summary>
        /// 웨이브 시스템을 시작한다.
        /// </summary>
        public void 게임시작()
        {
            _진행중 = true;
            _현재웨이브 = 0;
            _남은대기시간 = 웨이브대기시간;
            StartCoroutine(웨이브타이머코루틴());
            Debug.Log("[웨이브매니저] 게임 시작!");
        }

        /// <summary>
        /// 다음 웨이브 시작까지 카운트다운하는 코루틴.
        /// </summary>
        private IEnumerator 웨이브타이머코루틴()
        {
            while (_진행중)
            {
                _남은대기시간 = 웨이브대기시간;

                while (_남은대기시간 > 0)
                {
                    yield return new WaitForSeconds(1f);
                    _남은대기시간--;
                }

                다음웨이브시작();

                // 다음 루프는 게임 진행 중이고 현재 웨이브 적이 모두 처치된 후 진행됨
                yield return new WaitUntil(() => !_진행중 || (EnemySpawner.인스턴스 != null && EnemySpawner.인스턴스.살아있는적수 == 0));
            }
        }

        /// <summary>
        /// 다음 웨이브를 시작한다.
        /// </summary>
        private void 다음웨이브시작()
        {
            _현재웨이브++;
            bool 보스웨이브 = _현재웨이브 % 보스웨이브주기 == 0;

            웨이브변경이벤트?.Invoke(_현재웨이브);

            if (보스웨이브)
                보스웨이브이벤트?.Invoke();

            Debug.Log($"[웨이브매니저] 웨이브 {_현재웨이브} 시작" + (보스웨이브 ? " [보스 웨이브!]" : ""));

            // 스폰 데이터 생성
            List<스폰데이터> 스폰목록 = 웨이브스폰데이터생성(보스웨이브);
            EnemySpawner.인스턴스?.웨이브스폰시작(스폰목록);
        }

        /// <summary>
        /// 웨이브 번호와 보스 여부에 따라 스폰 데이터를 생성한다.
        /// 미리 설정된 웨이브가 있으면 해당 설정을 사용하고, 없으면 자동 생성한다.
        /// </summary>
        private List<스폰데이터> 웨이브스폰데이터생성(bool 보스웨이브)
        {
            // 미리 설정된 웨이브 데이터 사용
            int 인덱스 = _현재웨이브 - 1;
            if (웨이브설정목록.Count > 인덱스)
                return 웨이브설정목록[인덱스].스폰목록;

            // 자동 생성: 웨이브 번호에 따라 난이도 스케일링
            var 결과 = new List<스폰데이터>();

            if (!보스웨이브 && 일반적프리팹 != null)
            {
                결과.Add(new 스폰데이터
                {
                    적프리팹 = 일반적프리팹,
                    생성수 = 3 + _현재웨이브 * 2,
                    생성간격 = Mathf.Max(0.3f, 1.5f - _현재웨이브 * 0.05f)
                });
            }

            if (보스웨이브 && 보스적프리팹 != null)
            {
                결과.Add(new 스폰데이터
                {
                    적프리팹 = 보스적프리팹,
                    생성수 = 1,
                    생성간격 = 2f
                });
                // 보스와 함께 일반 적도 추가
                if (일반적프리팹 != null)
                {
                    결과.Add(new 스폰데이터
                    {
                        적프리팹 = 일반적프리팹,
                        생성수 = _현재웨이브,
                        생성간격 = 0.5f
                    });
                }
            }

            return 결과;
        }

        /// <summary>
        /// 웨이브가 클리어되었을 때 호출되는 콜백.
        /// </summary>
        private void 웨이브클리어처리()
        {
            int 보상 = 웨이브설정목록.Count > _현재웨이브 - 1
                ? 웨이브설정목록[_현재웨이브 - 1].보상골드
                : 50 + _현재웨이브 * 10;

            Core.GoldSystem.인스턴스?.골드추가(보상);
            Debug.Log($"[웨이브매니저] 웨이브 {_현재웨이브} 클리어! 보상 골드: {보상}");
        }

        /// <summary>현재 웨이브 번호 (읽기 전용)</summary>
        public int 현재웨이브 => _현재웨이브;

        /// <summary>다음 웨이브까지 남은 시간 (읽기 전용)</summary>
        public float 남은대기시간 => _남은대기시간;
    }
}
