using UnityEngine;

namespace S2RD.Hero
{
    /// <summary>
    /// 영웅 등급입니다.
    /// </summary>
    public enum HeroGrade
    {
        Common,
        Rare,
        Epic,
        Legendary
    }

    /// <summary>
    /// 영웅 직업 타입입니다.
    /// </summary>
    public enum HeroType
    {
        Archer,
        Knight,
        Mage,
        Lancer
    }

    /// <summary>
    /// 영웅의 전투 데이터를 담는 구조체입니다.
    /// </summary>
    [System.Serializable]
    public struct HeroData
    {
        public int Level;
        public HeroType Type;
        public HeroGrade Grade;
        public float AttackPower;
        public float AttackRange;
        public float AttackInterval;

        public static HeroData 생성(HeroType type, HeroGrade grade)
        {
            return 생성(type, 1, grade);
        }

        public static HeroData 생성(HeroType type, int level)
        {
            HeroGrade grade = 레벨에따른등급(level);
            return 생성(type, level, grade);
        }

        private static HeroData 생성(HeroType type, int level, HeroGrade grade)
        {
            float attack = 10f * Mathf.Pow(2f, Mathf.Max(0, level - 1));

            return new HeroData
            {
                Level = Mathf.Max(1, level),
                Type = type,
                Grade = grade,
                AttackPower = attack,
                AttackRange = 3f,
                AttackInterval = 1f
            };
        }

        private static HeroGrade 레벨에따른등급(int level)
        {
            if (level >= 4)
                return HeroGrade.Legendary;
            if (level == 3)
                return HeroGrade.Epic;
            if (level == 2)
                return HeroGrade.Rare;
            return HeroGrade.Common;
        }
    }
}
