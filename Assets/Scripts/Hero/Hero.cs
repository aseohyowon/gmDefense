// Hero.cs
// 게임 내 영웅 오브젝트를 제어하는 컴포넌트.
// 영웅의 자동 공격, 타겟 탐색, 투사체 발사 등 전투 로직을 담당한다.

using System.Collections;
using UnityEngine;
using GMDefense.Enemy;
using GMDefense.Systems;

namespace GMDefense.Heroes
{
    /// <summary>
    /// 영웅 컴포넌트.
    /// 영웅 데이터를 기반으로 자동 전투를 수행한다.
    /// </summary>
    public class Hero : MonoBehaviour
    {
        [Header("영웅 데이터")]
        [Tooltip("이 영웅에 적용할 ScriptableObject 데이터")]
        [SerializeField] private HeroData _영웅데이터;

        [Header("전투 설정")]
        [Tooltip("타겟 탐색 레이어 마스크")]
        [SerializeField] private LayerMask 적레이어마스크;

        [Tooltip("투사체 프리팹")]
        [SerializeField] private GameObject 투사체프리팹;

        // 현재 타겟 적
        private Transform _현재타겟;
        // 다음 공격 가능 시간
        private float _다음공격시간;
        // 현재 슬롯 인덱스
        private int _슬롯인덱스 = -1;

        // 현재 공격력 (시너지 배율 적용 포함)
        private float _현재공격력;
        // 현재 공격속도 (시너지 배율 적용 포함)
        private float _현재공격속도;

        /// <summary>영웅 데이터 프로퍼티 (읽기 전용)</summary>
        public HeroData 영웅데이터 => _영웅데이터;

        /// <summary>현재 슬롯 인덱스 프로퍼티</summary>
        public int 슬롯인덱스
        {
            get => _슬롯인덱스;
            set => _슬롯인덱스 = value;
        }

        private void Start()
        {
            스탯갱신();
        }

        private void Update()
        {
            타겟탐색();
            자동공격시도();
        }

        /// <summary>
        /// 영웅 데이터를 초기화한다.
        /// </summary>
        /// <param name="데이터">적용할 영웅 ScriptableObject 데이터</param>
        public void 초기화(HeroData 데이터)
        {
            _영웅데이터 = 데이터;
            스탯갱신();

            // 스프라이트 적용
            if (데이터.영웅스프라이트 != null)
            {
                var 렌더러 = GetComponent<SpriteRenderer>();
                if (렌더러 != null)
                    렌더러.sprite = 데이터.영웅스프라이트;
            }

            Debug.Log($"[영웅] '{데이터.영웅이름}' 초기화 완료 (등급: {데이터.영웅등급})");
        }

        /// <summary>
        /// 시너지 배율을 반영하여 스탯을 갱신한다.
        /// </summary>
        public void 스탯갱신()
        {
            if (_영웅데이터 == null) return;

            float 공격력배율 = TraitSystem.인스턴스 != null
                ? TraitSystem.인스턴스.전체공격력배율가져오기()
                : 1f;

            float 공격속도배율 = TraitSystem.인스턴스 != null
                ? TraitSystem.인스턴스.전체공격속도배율가져오기()
                : 1f;

            _현재공격력 = _영웅데이터.최종공격력 * 공격력배율;
            _현재공격속도 = _영웅데이터.공격속도 * 공격속도배율;
        }

        /// <summary>
        /// 사거리 내에서 가장 가까운 적을 타겟으로 선정한다.
        /// </summary>
        private void 타겟탐색()
        {
            if (_영웅데이터 == null) return;

            Collider2D[] 탐지된적들 = Physics2D.OverlapCircleAll(
                transform.position,
                _영웅데이터.사거리,
                적레이어마스크
            );

            float 최소거리 = float.MaxValue;
            Transform 가장가까운적 = null;

            foreach (var 충돌체 in 탐지된적들)
            {
                float 거리 = Vector2.Distance(transform.position, 충돌체.transform.position);
                if (거리 < 최소거리)
                {
                    최소거리 = 거리;
                    가장가까운적 = 충돌체.transform;
                }
            }

            _현재타겟 = 가장가까운적;
        }

        /// <summary>
        /// 공격 쿨다운이 완료되면 타겟에게 공격한다.
        /// </summary>
        private void 자동공격시도()
        {
            if (_현재타겟 == null) return;
            if (Time.time < _다음공격시간) return;
            if (_현재공격속도 <= 0) return;

            공격실행(_현재타겟);
            _다음공격시간 = Time.time + (1f / _현재공격속도);
        }

        /// <summary>
        /// 지정된 타겟에게 공격을 수행한다.
        /// </summary>
        private void 공격실행(Transform 타겟)
        {
            if (투사체프리팹 != null)
            {
                // 투사체 발사 방식
                var 투사체 = Instantiate(투사체프리팹, transform.position, Quaternion.identity);
                var 투사체컴포넌트 = 투사체.GetComponent<Projectile>();
                if (투사체컴포넌트 != null)
                    투사체컴포넌트.초기화(타겟, _현재공격력);
            }
            else
            {
                // 직접 데미지 방식 (투사체 없을 때)
                var 적컴포넌트 = 타겟.GetComponent<EnemyBase>();
                if (적컴포넌트 != null)
                    적컴포넌트.데미지받기(_현재공격력);
            }
        }

        /// <summary>
        /// 사거리를 에디터에서 시각적으로 표시한다.
        /// </summary>
        private void OnDrawGizmosSelected()
        {
            if (_영웅데이터 == null) return;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, _영웅데이터.사거리);
        }
    }

    /// <summary>
    /// 영웅이 발사하는 투사체 컴포넌트.
    /// </summary>
    public class Projectile : MonoBehaviour
    {
        [Tooltip("투사체 이동 속도")]
        [SerializeField] private float 이동속도 = 8f;

        private Transform _타겟;
        private float _데미지;
        private bool _초기화완료 = false;

        /// <summary>
        /// 투사체를 초기화한다.
        /// </summary>
        public void 초기화(Transform 타겟, float 데미지)
        {
            _타겟 = 타겟;
            _데미지 = 데미지;
            _초기화완료 = true;
        }

        private void Update()
        {
            if (!_초기화완료 || _타겟 == null)
            {
                Destroy(gameObject);
                return;
            }

            // 타겟 방향으로 이동
            transform.position = Vector2.MoveTowards(
                transform.position,
                _타겟.position,
                이동속도 * Time.deltaTime
            );

            // 타겟에 도달 시 데미지 적용
            if (Vector2.Distance(transform.position, _타겟.position) < 0.1f)
            {
                var 적 = _타겟.GetComponent<EnemyBase>();
                if (적 != null)
                    적.데미지받기(_데미지);

                Destroy(gameObject);
            }
        }
    }
}
