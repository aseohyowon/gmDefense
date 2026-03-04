// EnemySpawner.cs
// 적 생성을 담당하는 스포너 클래스.
// WaveManager의 지시를 받아 적 프리팹을 경로 시작점에 생성한다.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GMDefense.Enemy
{
    /// <summary>
    /// 웨이브 내 스폰 정보를 담는 데이터 구조체.
    /// </summary>
    [System.Serializable]
    public struct 스폰데이터
    {
        [Tooltip("생성할 적 프리팹")]
        public GameObject 적프리팹;

        [Tooltip("이 적 생성 수")]
        public int 생성수;

        [Tooltip("각 적 생성 간격 (초)")]
        public float 생성간격;
    }

    /// <summary>
    /// 적 스포너 컴포넌트.
    /// 적을 경로의 시작 지점에서 생성하고 경로를 부여한다.
    /// </summary>
    public class EnemySpawner : MonoBehaviour
    {
        [Header("경로 설정")]
        [Tooltip("적이 이동할 웨이포인트 목록 (시작 → 종료 순서)")]
        [SerializeField] private Transform[] 경로포인트;

        // 현재 스폰 중인지 여부
        private bool _스폰중 = false;

        // 생성된 적 목록 (살아있는 적 추적용)
        private List<EnemyBase> _생성된적목록 = new List<EnemyBase>();

        // 모든 적 사망 이벤트
        public System.Action 웨이브클리어이벤트;

        private static EnemySpawner _인스턴스;
        public static EnemySpawner 인스턴스
        {
            get
            {
                if (_인스턴스 == null)
                    _인스턴스 = FindObjectOfType<EnemySpawner>();
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

        /// <summary>
        /// 지정된 스폰 데이터 목록에 따라 적을 순서대로 생성한다.
        /// </summary>
        /// <param name="스폰목록">스폰할 적 정보 목록</param>
        public void 웨이브스폰시작(List<스폰데이터> 스폰목록)
        {
            if (_스폰중)
            {
                Debug.LogWarning("[스포너] 이미 스폰 중입니다.");
                return;
            }
            StartCoroutine(스폰코루틴(스폰목록));
        }

        /// <summary>
        /// 스폰 데이터에 따라 순차적으로 적을 생성하는 코루틴.
        /// </summary>
        private IEnumerator 스폰코루틴(List<스폰데이터> 스폰목록)
        {
            _스폰중 = true;
            _생성된적목록.Clear();

            foreach (var 스폰 in 스폰목록)
            {
                if (스폰.적프리팹 == null) continue;

                for (int i = 0; i < 스폰.생성수; i++)
                {
                    적생성(스폰.적프리팹);
                    yield return new WaitForSeconds(스폰.생성간격);
                }
            }

            _스폰중 = false;
            Debug.Log("[스포너] 웨이브 스폰 완료. 남은 적 수: " + _생성된적목록.Count);
        }

        /// <summary>
        /// 적을 경로 시작점에 생성하고 이벤트를 등록한다.
        /// </summary>
        private void 적생성(GameObject 프리팹)
        {
            if (경로포인트 == null || 경로포인트.Length == 0)
            {
                Debug.LogError("[스포너] 경로 포인트가 설정되지 않았습니다.");
                return;
            }

            Vector3 시작위치 = 경로포인트[0].position;
            var 오브젝트 = Instantiate(프리팹, 시작위치, Quaternion.identity);
            var 적 = 오브젝트.GetComponent<EnemyBase>();

            if (적 == null)
            {
                Debug.LogError($"[스포너] 프리팹 '{프리팹.name}'에 EnemyBase 컴포넌트가 없습니다.");
                Destroy(오브젝트);
                return;
            }

            적.초기화(경로포인트);
            _생성된적목록.Add(적);

            // 사망/도달 이벤트 구독
            적.사망이벤트 += (골드) => 적제거처리(적, 골드);
            적.성문도달이벤트 += (피해) => 적성문도달처리(적, 피해);
        }

        /// <summary>
        /// 적 처치 후 목록에서 제거하고 클리어 여부를 확인한다.
        /// </summary>
        private void 적제거처리(EnemyBase 적, int 골드)
        {
            _생성된적목록.Remove(적);
            웨이브완료확인();
        }

        /// <summary>
        /// 적이 성문에 도달했을 때 플레이어에게 피해를 주고 목록에서 제거한다.
        /// </summary>
        private void 적성문도달처리(EnemyBase 적, int 피해량)
        {
            _생성된적목록.Remove(적);
            // 플레이어 HP 감소
            GMDefense.Core.GameManager.인스턴스?.플레이어피해받기(피해량);
            웨이브완료확인();
        }

        /// <summary>
        /// 모든 적이 제거되었는지 확인하고 클리어 이벤트를 발생시킨다.
        /// </summary>
        private void 웨이브완료확인()
        {
            if (!_스폰중 && _생성된적목록.Count == 0)
            {
                Debug.Log("[스포너] 모든 적 제거 완료. 웨이브 클리어!");
                웨이브클리어이벤트?.Invoke();
            }
        }

        /// <summary>
        /// 현재 살아있는 적 수를 반환한다.
        /// </summary>
        public int 살아있는적수 => _생성된적목록.Count;
    }
}
