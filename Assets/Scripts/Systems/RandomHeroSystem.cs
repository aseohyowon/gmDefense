using UnityEngine;
using S2RD.Hero;

namespace S2RD.Systems
{
    /// <summary>
    /// 랜덤 영웅 소환 확률을 계산하는 시스템입니다.
    /// </summary>
    public class RandomHeroSystem : MonoBehaviour
    {
        public HeroData 랜덤영웅데이터생성()
        {
            HeroGrade grade = 등급추첨();
            HeroType type = (HeroType)Random.Range(0, 4);
            return HeroData.생성(type, grade);
        }

        private HeroGrade 등급추첨()
        {
            float roll = Random.Range(0f, 100f);
            if (roll < 60f)
                return HeroGrade.Common;
            if (roll < 90f)
                return HeroGrade.Rare;
            return HeroGrade.Epic;
        }
    }
}
