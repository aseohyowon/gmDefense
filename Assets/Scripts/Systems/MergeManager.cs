// MergeManager.cs
// 영웅 합성 시스템을 관리하는 클래스.
// 같은 종류 + 같은 레벨 영웅 두 개를 합성하여 상위 레벨 영웅을 생성한다.

using System.Collections.Generic;
using UnityEngine;
using GMDefense.Heroes;

namespace GMDefense.Systems
{
    /// <summary>
    /// 합성 매니저.
    /// 드래그 앤 드롭으로 영웅 합성을 처리하고 상위 영웅을 생성한다.
    /// </summary>
    public class MergeManager : MonoBehaviour
    {
        [Header("합성 설정")]
        [Tooltip("영웅 프리팹 (생성 시 사용)")]
        [SerializeField] private GameObject 영웅프리팹;

        [Tooltip("각 영웅 종류별 등급 데이터 목록. 인덱스 0 = 등급1, 6 = 등급7")]
        [SerializeField] private List<HeroDataGroup> 영웅데이터그룹목록 = new List<HeroDataGroup>();

        // 합성 성공 이벤트
        public System.Action<Hero, Hero, Hero> 합성성공이벤트;

        private static MergeManager _인스턴스;
        public static MergeManager 인스턴스
        {
            get
            {
                if (_인스턴스 == null)
                    _인스턴스 = FindObjectOfType<MergeManager>();
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
        /// 두 영웅의 합성을 시도한다.
        /// 합성 가능 조건: 같은 종류 + 같은 등급, 최대 등급(7) 미만.
        /// </summary>
        /// <param name="영웅A">합성 원본 영웅 A</param>
        /// <param name="영웅B">합성 원본 영웅 B</param>
        /// <returns>합성 성공 여부</returns>
        public bool 합성시도(Hero 영웅A, Hero 영웅B)
        {
            if (영웅A == null || 영웅B == null)
            {
                Debug.LogWarning("[합성매니저] 합성 대상 영웅이 null입니다.");
                return false;
            }

            var 데이터A = 영웅A.영웅데이터;
            var 데이터B = 영웅B.영웅데이터;

            if (데이터A == null || 데이터B == null)
            {
                Debug.LogWarning("[합성매니저] 영웅 데이터가 없습니다.");
                return false;
            }

            // 합성 가능 여부 확인
            if (!데이터A.합성가능(데이터B))
            {
                Debug.Log($"[합성매니저] 합성 불가: {데이터A.영웅이름}({데이터A.영웅등급}) + {데이터B.영웅이름}({데이터B.영웅등급})");
                return false;
            }

            // 최대 등급 확인 (7등급은 더 이상 합성 불가)
            if (데이터A.영웅등급 == HeroGrade.등급7)
            {
                Debug.Log("[합성매니저] 최대 등급 영웅은 합성할 수 없습니다.");
                return false;
            }

            // 상위 등급 데이터 탐색
            HeroData 상위데이터 = 상위등급데이터가져오기(데이터A);
            if (상위데이터 == null)
            {
                Debug.LogWarning($"[합성매니저] '{데이터A.영웅이름}'의 상위 등급 데이터를 찾을 수 없습니다.");
                return false;
            }

            // 슬롯 위치 저장
            int 슬롯A인덱스 = 영웅A.슬롯인덱스;
            int 슬롯B인덱스 = 영웅B.슬롯인덱스;

            // 기존 영웅 제거
            GridSystem.인스턴스?.영웅제거(슬롯A인덱스);
            GridSystem.인스턴스?.영웅제거(슬롯B인덱스);
            Destroy(영웅A.gameObject);
            Destroy(영웅B.gameObject);

            // 상위 영웅 생성 (영웅A의 슬롯 위치에 생성)
            var 새영웅 = 새영웅생성(상위데이터, 슬롯A인덱스);

            합성성공이벤트?.Invoke(영웅A, 영웅B, 새영웅);

            // 특성 시너지 재계산
            var 배치목록 = GridSystem.인스턴스?.배치된영웅목록가져오기();
            if (배치목록 != null)
                TraitSystem.인스턴스?.시너지재계산(배치목록);

            Debug.Log($"[합성매니저] 합성 성공! {데이터A.영웅이름} + {데이터B.영웅이름} → {상위데이터.영웅이름}(등급{상위데이터.영웅등급})");
            return true;
        }

        /// <summary>
        /// 현재 영웅 데이터의 상위 등급 데이터를 반환한다.
        /// 등급별데이터 목록은 0-based: [0]=등급1, [1]=등급2, ..., [6]=등급7.
        /// 현재 등급이 N이면 다음 등급 데이터는 인덱스 N (enum 값 = N이므로).
        /// </summary>
        private HeroData 상위등급데이터가져오기(HeroData 현재데이터)
        {
            var 그룹 = 영웅데이터그룹목록.Find(g => g.영웅종류 == 현재데이터.영웅종류);
            if (그룹 == null) return null;

            // HeroGrade 값: 등급1=1, 등급2=2, ..., 등급7=7
            // 목록 인덱스: 0=등급1, 1=등급2, ..., 6=등급7
            // 현재 등급이 N이면 다음 등급 인덱스 = N (= 현재 enum 값)
            int 다음등급인덱스 = (int)현재데이터.영웅등급;
            if (다음등급인덱스 >= 그룹.등급별데이터.Count) return null;

            return 그룹.등급별데이터[다음등급인덱스];
        }

        /// <summary>
        /// 상위 영웅을 지정된 슬롯에 생성한다.
        /// </summary>
        private Hero 새영웅생성(HeroData 데이터, int 슬롯인덱스)
        {
            if (영웅프리팹 == null)
            {
                Debug.LogError("[합성매니저] 영웅 프리팹이 설정되지 않았습니다.");
                return null;
            }

            var 슬롯 = GridSystem.인스턴스?.슬롯가져오기(슬롯인덱스);
            Vector3 생성위치 = 슬롯?.위치 ?? Vector3.zero;

            var 오브젝트 = Instantiate(영웅프리팹, 생성위치, Quaternion.identity);
            var 영웅 = 오브젝트.GetComponent<Hero>();
            영웅?.초기화(데이터);

            if (슬롯 != null && 영웅 != null)
                GridSystem.인스턴스?.영웅배치(영웅, 슬롯);

            return 영웅;
        }
    }

    /// <summary>
    /// 영웅 종류별 등급 데이터 그룹 (인스펙터 설정용).
    /// </summary>
    [System.Serializable]
    public class HeroDataGroup
    {
        [Tooltip("영웅 종류")]
        public HeroType 영웅종류;

        [Tooltip("등급별 HeroData 목록 (인덱스 0 = 등급1, 인덱스 6 = 등급7)")]
        public List<HeroData> 등급별데이터 = new List<HeroData>();
    }
}
