// DragDropHandler.cs
// 모바일 터치 및 마우스 입력을 처리하는 드래그 앤 드롭 시스템.
// 영웅을 드래그하여 다른 슬롯으로 이동하거나 합성을 트리거한다.

using UnityEngine;
using UnityEngine.EventSystems;
using GMDefense.Heroes;

namespace GMDefense.Systems
{
    /// <summary>
    /// 영웅 드래그 앤 드롭 핸들러.
    /// 영웅 게임오브젝트에 부착하여 이동 및 합성을 처리한다.
    /// </summary>
    public class DragDropHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("드래그 설정")]
        [Tooltip("드래그 중 표시할 Z 오프셋 (다른 오브젝트 위에 표시)")]
        [SerializeField] private float 드래그Z오프셋 = -1f;

        [Tooltip("드래그 중 투명도 (0 ~ 1)")]
        [SerializeField] private float 드래그투명도 = 0.7f;

        // 드래그 시작 전 원래 위치
        private Vector3 _원래위치;
        // 드래그 시작 전 원래 슬롯
        private HeroSlot _원래슬롯;
        // 현재 드래그 중인 영웅 컴포넌트
        private Hero _영웅;
        // 스프라이트 렌더러
        private SpriteRenderer _스프라이트렌더러;
        // 카메라 캐시
        private Camera _메인카메라;

        private void Awake()
        {
            _영웅 = GetComponent<Hero>();
            _스프라이트렌더러 = GetComponent<SpriteRenderer>();
            _메인카메라 = Camera.main;
        }

        /// <summary>
        /// 드래그 시작 시 호출. 원래 위치와 슬롯을 저장한다.
        /// </summary>
        public void OnBeginDrag(PointerEventData eventData)
        {
            _원래위치 = transform.position;
            _원래슬롯 = GridSystem.인스턴스?.슬롯가져오기(_영웅?.슬롯인덱스 ?? -1);

            // 드래그 중 시각 피드백
            if (_스프라이트렌더러 != null)
            {
                var 색 = _스프라이트렌더러.color;
                색.a = 드래그투명도;
                _스프라이트렌더러.color = 색;
            }

            // 그리드에서 임시 제거 (다른 슬롯에 배치 가능하도록)
            if (_원래슬롯 != null)
                _원래슬롯.영웅 = null;

            Debug.Log($"[드래그드롭] 드래그 시작: {_영웅?.영웅데이터?.영웅이름}");
        }

        /// <summary>
        /// 드래그 중 매 프레임 호출. 영웅을 포인터 위치로 이동시킨다.
        /// </summary>
        public void OnDrag(PointerEventData eventData)
        {
            if (_메인카메라 == null) return;

            Vector3 스크린위치 = new Vector3(
                eventData.position.x,
                eventData.position.y,
                _메인카메라.WorldToScreenPoint(transform.position).z
            );

            Vector3 월드위치 = _메인카메라.ScreenToWorldPoint(스크린위치);
            월드위치.z = 드래그Z오프셋;
            transform.position = 월드위치;
        }

        /// <summary>
        /// 드래그 종료 시 호출. 가장 가까운 슬롯을 찾아 배치 또는 합성을 시도한다.
        /// </summary>
        public void OnEndDrag(PointerEventData eventData)
        {
            // 투명도 복원
            if (_스프라이트렌더러 != null)
            {
                var 색 = _스프라이트렌더러.color;
                색.a = 1f;
                _스프라이트렌더러.color = 색;
            }

            if (GridSystem.인스턴스 == null)
            {
                원래위치로복귀();
                return;
            }

            // 가장 가까운 슬롯 탐색
            var 대상슬롯 = GridSystem.인스턴스.가장가까운슬롯가져오기(transform.position);

            if (대상슬롯 == null)
            {
                원래위치로복귀();
                return;
            }

            // 대상 슬롯에 영웅이 이미 있는 경우 → 합성 시도
            if (!대상슬롯.비어있음)
            {
                var 대상영웅 = 대상슬롯.영웅;

                if (MergeManager.인스턴스 != null && MergeManager.인스턴스.합성시도(_영웅, 대상영웅))
                {
                    // 합성 성공 - 두 영웅은 MergeManager에서 처리됨
                    Debug.Log("[드래그드롭] 합성 성공");
                    return;
                }
                else
                {
                    // 합성 실패 → 원래 자리로 복귀
                    원래위치로복귀();
                    return;
                }
            }

            // 빈 슬롯 → 이동
            if (GridSystem.인스턴스.영웅배치(_영웅, 대상슬롯))
            {
                Debug.Log($"[드래그드롭] 슬롯 {대상슬롯.인덱스}으로 이동");
            }
            else
            {
                원래위치로복귀();
            }
        }

        /// <summary>
        /// 영웅을 드래그 전 원래 슬롯 위치로 되돌린다.
        /// </summary>
        private void 원래위치로복귀()
        {
            transform.position = _원래위치;

            if (_원래슬롯 != null && _영웅 != null)
            {
                _원래슬롯.영웅 = _영웅;
                _영웅.슬롯인덱스 = _원래슬롯.인덱스;
            }

            Debug.Log("[드래그드롭] 원래 위치로 복귀");
        }
    }
}
