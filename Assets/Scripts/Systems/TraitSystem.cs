// TraitSystem.cs
// 영웅의 특성(Trait) 시스템을 관리하는 클래스.
// 영웅들의 특성 조합에 따라 시너지 효과를 계산하고 적용한다.

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using GMDefense.Heroes;

namespace GMDefense.Systems
{
    /// <summary>
    /// 특성 시너지 데이터 구조체.
    /// </summary>
    [Serializable]
    public struct 시너지데이터
    {
        [Tooltip("시너지 발동에 필요한 최소 영웅 수")]
        public int 필요영웅수;

        [Tooltip("시너지 효과 설명")]
        public string 시너지설명;

        [Tooltip("공격력 배율 보너스 (1.0 = 변화없음)")]
        public float 공격력배율;

        [Tooltip("공격속도 배율 보너스 (1.0 = 변화없음)")]
        public float 공격속도배율;
    }

    /// <summary>
    /// 특성 시스템 매니저.
    /// 배치된 영웅들의 특성을 분석하여 시너지를 계산한다.
    /// </summary>
    public class TraitSystem : MonoBehaviour
    {
        // 특성별 시너지 설정 (인스펙터에서 설정 가능)
        [Header("특성 시너지 설정")]
        [SerializeField] private List<TraitSynergyConfig> 시너지설정목록 = new List<TraitSynergyConfig>();

        // 현재 활성화된 시너지 목록
        private Dictionary<TraitType, int> 활성시너지 = new Dictionary<TraitType, int>();

        // 시너지 변경 이벤트
        public event Action<Dictionary<TraitType, int>> 시너지변경이벤트;

        private static TraitSystem _인스턴스;
        public static TraitSystem 인스턴스
        {
            get
            {
                if (_인스턴스 == null)
                    _인스턴스 = FindObjectOfType<TraitSystem>();
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
        /// 배치된 영웅 목록을 받아 시너지를 재계산한다.
        /// </summary>
        /// <param name="배치영웅목록">현재 배치된 영웅 리스트</param>
        public void 시너지재계산(List<Hero> 배치영웅목록)
        {
            활성시너지.Clear();

            foreach (var 영웅 in 배치영웅목록)
            {
                if (영웅 == null || 영웅.영웅데이터 == null) continue;

                foreach (var 특성 in 영웅.영웅데이터.특성목록)
                {
                    if (특성 == TraitType.없음) continue;

                    if (!활성시너지.ContainsKey(특성))
                        활성시너지[특성] = 0;

                    활성시너지[특성]++;
                }
            }

            시너지변경이벤트?.Invoke(활성시너지);
            Debug.Log($"[특성시스템] 시너지 재계산 완료. 활성 특성 수: {활성시너지.Count}");
        }

        /// <summary>
        /// 특정 특성의 현재 시너지 레벨을 반환한다.
        /// </summary>
        public int 시너지레벨가져오기(TraitType 특성)
        {
            return 활성시너지.TryGetValue(특성, out int 수) ? 수 : 0;
        }

        /// <summary>
        /// 현재 활성화된 시너지에 따른 전체 공격력 배율을 반환한다.
        /// </summary>
        public float 전체공격력배율가져오기()
        {
            float 배율 = 1f;

            foreach (var 쌍 in 활성시너지)
            {
                var 설정 = 시너지설정목록.Find(s => s.특성종류 == 쌍.Key);
                if (설정 == null) continue;

                foreach (var 시너지 in 설정.시너지단계)
                {
                    if (쌍.Value >= 시너지.필요영웅수)
                        배율 *= 시너지.공격력배율;
                }
            }

            return 배율;
        }

        /// <summary>
        /// 현재 활성화된 시너지에 따른 전체 공격속도 배율을 반환한다.
        /// </summary>
        public float 전체공격속도배율가져오기()
        {
            float 배율 = 1f;

            foreach (var 쌍 in 활성시너지)
            {
                var 설정 = 시너지설정목록.Find(s => s.특성종류 == 쌍.Key);
                if (설정 == null) continue;

                foreach (var 시너지 in 설정.시너지단계)
                {
                    if (쌍.Value >= 시너지.필요영웅수)
                        배율 *= 시너지.공격속도배율;
                }
            }

            return 배율;
        }

        /// <summary>
        /// 현재 시너지 상태를 문자열로 반환 (UI 표시용).
        /// </summary>
        public string 시너지상태문자열가져오기()
        {
            if (활성시너지.Count == 0) return "활성 시너지 없음";

            var 결과 = new System.Text.StringBuilder();
            foreach (var 쌍 in 활성시너지)
            {
                결과.AppendLine($"{쌍.Key}: {쌍.Value}개");
            }
            return 결과.ToString();
        }
    }

    /// <summary>
    /// 특성별 시너지 단계 설정 (ScriptableObject 대신 직렬화 가능한 클래스로 구성).
    /// </summary>
    [Serializable]
    public class TraitSynergyConfig
    {
        [Tooltip("특성 종류")]
        public TraitType 특성종류;

        [Tooltip("시너지 단계 목록 (필요 영웅수 순으로 정렬)")]
        public List<시너지데이터> 시너지단계 = new List<시너지데이터>();
    }
}
