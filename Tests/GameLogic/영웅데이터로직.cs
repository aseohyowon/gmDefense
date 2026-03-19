// 영웅데이터로직.cs
// HeroData.cs의 순수 게임 로직을 Unity 없이 테스트하기 위한 클래스.
// ScriptableObject 없이 영웅 데이터와 합성 가능 여부 로직을 구현한다.

namespace GMDefense.Tests.GameLogic
{
    /// <summary>
    /// 영웅 등급 열거형. HeroData.cs의 HeroGrade와 동일.
    /// </summary>
    public enum 영웅등급
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
    /// 영웅 종류 열거형. HeroData.cs의 HeroType과 동일.
    /// </summary>
    public enum 영웅종류
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
    /// Unity 의존성 없는 영웅 데이터 클래스.
    /// HeroData.cs와 동일한 비즈니스 로직을 갖는다.
    /// </summary>
    public class 영웅데이터로직
    {
        /// <summary>영웅 이름</summary>
        public string 영웅이름 { get; set; } = "";

        /// <summary>영웅 종류</summary>
        public 영웅종류 종류 { get; set; }

        /// <summary>영웅 등급 (1~7단계)</summary>
        public 영웅등급 등급 { get; set; } = 영웅등급.등급1;

        /// <summary>기본 공격력</summary>
        public float 공격력 { get; set; } = 10f;

        /// <summary>공격 속도 (초당 공격 횟수)</summary>
        public float 공격속도 { get; set; } = 1f;

        /// <summary>공격 사거리</summary>
        public float 사거리 { get; set; } = 3f;

        /// <summary>
        /// 등급에 따른 실제 공격력.
        /// HeroData.cs의 최종공격력 프로퍼티와 동일한 계산식.
        /// </summary>
        public float 최종공격력 => 공격력 * (int)등급;

        /// <summary>
        /// 두 영웅이 합성 가능한지 확인한다.
        /// 조건: 같은 종류 + 같은 등급
        /// HeroData.cs의 합성가능() 메서드와 동일한 로직.
        /// </summary>
        /// <param name="대상">비교할 대상 영웅 데이터</param>
        /// <returns>합성 가능 여부</returns>
        public bool 합성가능(영웅데이터로직? 대상)
        {
            if (대상 == null) return false;
            return 종류 == 대상.종류 && 등급 == 대상.등급;
        }

        /// <summary>
        /// 최대 등급(7등급) 여부를 반환한다.
        /// </summary>
        public bool 최대등급여부 => 등급 == 영웅등급.등급7;
    }
}
