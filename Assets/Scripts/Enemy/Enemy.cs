// Enemy.cs
// 적 캐릭터의 기본 동작을 정의하는 클래스.
// 경로를 따라 이동하고, 체력 관리 및 사망 처리를 담당한다.

using System;
using UnityEngine;

namespace GMDefense.Enemy
{
    /// <summary>
    /// 적 기본 추상 클래스.
    /// 모든 적 타입이 상속받아야 하는 공통 기능을 정의한다.
    /// </summary>
    public class EnemyBase : MonoBehaviour
    {
        [Header("적 기본 스탯")]
        [Tooltip("최대 체력")]
        [SerializeField] protected float 최대체력 = 100f;

        [Tooltip("이동 속도")]
        [SerializeField] protected float 이동속도 = 2f;

        [Tooltip("성문 도달 시 감소할 플레이어 HP")]
        [SerializeField] protected int 성문피해량 = 1;

        [Tooltip("처치 시 획득 골드")]
        [SerializeField] protected int 획득골드 = 10;

        // 현재 체력
        protected float _현재체력;
        // 이동 경로 포인트 목록
        protected Transform[] _경로포인트;
        // 현재 향하는 경로 인덱스
        protected int _현재경로인덱스 = 0;
        // 이미 처치되었는지 여부 (중복 처리 방지)
        private bool _처치됨 = false;

        // 적 사망 이벤트 (처치 골드 전달)
        public event Action<int> 사망이벤트;
        // 성문 도달 이벤트 (피해량 전달)
        public event Action<int> 성문도달이벤트;

        protected virtual void Awake()
        {
            _현재체력 = 최대체력;
        }

        protected virtual void Update()
        {
            경로이동();
        }

        /// <summary>
        /// 적을 초기화하고 이동 경로를 설정한다.
        /// </summary>
        /// <param name="경로">이동할 웨이포인트 배열</param>
        public virtual void 초기화(Transform[] 경로)
        {
            _경로포인트 = 경로;
            _현재경로인덱스 = 0;
            _현재체력 = 최대체력;
            _처치됨 = false;
        }

        /// <summary>
        /// 경로를 따라 이동한다.
        /// </summary>
        protected virtual void 경로이동()
        {
            if (_경로포인트 == null || _경로포인트.Length == 0) return;
            if (_현재경로인덱스 >= _경로포인트.Length)
            {
                성문도달처리();
                return;
            }

            Transform 목표 = _경로포인트[_현재경로인덱스];
            if (목표 == null) return;

            transform.position = Vector2.MoveTowards(
                transform.position,
                목표.position,
                이동속도 * Time.deltaTime
            );

            // 목표 포인트에 충분히 가까워지면 다음 포인트로
            if (Vector2.Distance(transform.position, 목표.position) < 0.05f)
                _현재경로인덱스++;
        }

        /// <summary>
        /// 데미지를 받는다.
        /// </summary>
        /// <param name="데미지량">입힐 데미지 수치</param>
        public virtual void 데미지받기(float 데미지량)
        {
            if (_처치됨) return;

            _현재체력 -= 데미지량;
            _현재체력 = Mathf.Max(0f, _현재체력);

            if (_현재체력 <= 0f)
                사망처리();
        }

        /// <summary>
        /// 적이 사망했을 때의 처리.
        /// </summary>
        protected virtual void 사망처리()
        {
            if (_처치됨) return;
            _처치됨 = true;

            사망이벤트?.Invoke(획득골드);
            Debug.Log($"[적] '{gameObject.name}' 사망. 획득 골드: {획득골드}");

            Destroy(gameObject);
        }

        /// <summary>
        /// 적이 성문에 도달했을 때의 처리.
        /// </summary>
        protected virtual void 성문도달처리()
        {
            if (_처치됨) return;
            _처치됨 = true;

            성문도달이벤트?.Invoke(성문피해량);
            Debug.Log($"[적] '{gameObject.name}' 성문 도달. 피해량: {성문피해량}");

            Destroy(gameObject);
        }

        /// <summary>
        /// 현재 체력 비율을 반환 (0 ~ 1).
        /// </summary>
        public float 체력비율 => 최대체력 > 0 ? _현재체력 / 최대체력 : 0f;
    }

    /// <summary>
    /// 일반 적 클래스. EnemyBase를 상속받는 기본 구현체.
    /// </summary>
    public class NormalEnemy : EnemyBase
    {
        // 일반 적 특이 동작은 없음. EnemyBase 기본 동작 사용.
    }

    /// <summary>
    /// 보스 적 클래스. 강화된 스탯과 특수 패턴을 가진다.
    /// </summary>
    public class BossEnemy : EnemyBase
    {
        [Header("보스 고유 설정")]
        [Tooltip("보스 분노 임계치 (체력이 이 비율 이하로 떨어지면 가속)")]
        [SerializeField] private float 분노임계치 = 0.3f;

        [Tooltip("분노 상태의 이동속도 배율")]
        [SerializeField] private float 분노이동속도배율 = 1.5f;

        private bool _분노상태 = false;
        private float _기본이동속도;

        protected override void Awake()
        {
            base.Awake();
            _기본이동속도 = 이동속도;
        }

        public override void 데미지받기(float 데미지량)
        {
            base.데미지받기(데미지량);

            // 체력이 임계치 이하로 떨어지면 분노 상태 진입
            if (!_분노상태 && 체력비율 <= 분노임계치)
            {
                _분노상태 = true;
                이동속도 = _기본이동속도 * 분노이동속도배율;
                Debug.Log($"[보스] '{gameObject.name}' 분노 상태 진입! 이동속도 {이동속도:F1}");
            }
        }
    }
}
