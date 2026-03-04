// HeroData.cs
// 영웅의 기본 데이터를 ScriptableObject로 정의하는 클래스.
// 영웅 종류, 레벨, 공격력, 공격속도, 사거리, 특성 등의 데이터를 담는다.

using UnityEngine;

namespace GMDefense.Heroes
{
    /// <summary>
    /// 영웅 특성 종류 열거형.
    /// </summary>
    public enum TraitType
    {
        없음,
        빛의수호자,     // 공격력 증가 시너지
        어둠의사냥꾼,   // 공격속도 증가 시너지
        자연의수호,     // 회복 시너지
        불꽃전사,       // 광역 데미지 시너지
        얼음마법사,     // 둔화 시너지
        번개술사,       // 연쇄 공격 시너지
        강철기사        // 방어력 증가 시너지
    }

    /// <summary>
    /// 영웅 등급(성장 단계) 열거형. 총 7단계.
    /// </summary>
    public enum HeroGrade
    {
        등급1 = 1,
        등급2 = 2,
        등급3 = 3,
        등급4 = 4,
        등급5 = 5,
        등급6 = 6,
        등급7 = 7
    }

    /// <summary>
    /// 영웅 종류 열거형.
    /// </summary>
    public enum HeroType
    {
        전사,
        궁수,
        마법사,
        성기사,
        암살자,
        드루이드,
        용병
    }

    /// <summary>
    /// 영웅 데이터 ScriptableObject.
    /// 영웅의 기본 스탯과 메타데이터를 정의한다.
    /// </summary>
    [CreateAssetMenu(fileName = "새영웅데이터", menuName = "GMDefense/영웅 데이터")]
    public class HeroData : ScriptableObject
    {
        [Header("기본 정보")]
        [Tooltip("영웅 이름")]
        public string 영웅이름;

        [Tooltip("영웅 종류")]
        public HeroType 영웅종류;

        [Tooltip("영웅 등급 (1~7단계)")]
        public HeroGrade 영웅등급 = HeroGrade.등급1;

        [Header("전투 스탯")]
        [Tooltip("기본 공격력")]
        public float 공격력 = 10f;

        [Tooltip("공격 속도 (초당 공격 횟수)")]
        public float 공격속도 = 1f;

        [Tooltip("공격 사거리")]
        public float 사거리 = 3f;

        [Header("특성")]
        [Tooltip("영웅이 보유한 특성 목록")]
        public TraitType[] 특성목록;

        [Header("소환 정보")]
        [Tooltip("소환 시 필요한 골드")]
        public int 소환비용 = 100;

        [Header("비주얼")]
        [Tooltip("영웅 스프라이트")]
        public Sprite 영웅스프라이트;

        /// <summary>
        /// 등급에 따른 실제 공격력을 계산하여 반환한다.
        /// </summary>
        public float 최종공격력 => 공격력 * (int)영웅등급;

        /// <summary>
        /// 두 영웅 데이터가 합성 가능한지 확인한다.
        /// 같은 종류, 같은 등급이어야 합성 가능.
        /// </summary>
        public bool 합성가능(HeroData 대상)
        {
            if (대상 == null) return false;
            return 영웅종류 == 대상.영웅종류 && 영웅등급 == 대상.영웅등급;
        }
    }
}
