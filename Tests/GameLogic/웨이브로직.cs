// 웨이브로직.cs
// WaveManager.cs의 웨이브 난이도 스케일링 로직을 Unity 없이 테스트하기 위한 클래스.
// 웨이브 번호에 따른 적 생성 수, 생성 간격, 보스 여부를 계산한다.

namespace GMDefense.Tests.GameLogic
{
    /// <summary>
    /// Unity 의존성 없는 웨이브 로직 계산 클래스.
    /// WaveManager.cs의 웨이브스폰데이터생성 로직을 검증 가능하게 추출한 것.
    /// </summary>
    public class 웨이브로직
    {
        /// <summary>보스 웨이브 주기 (N웨이브마다 보스 등장)</summary>
        public readonly int 보스웨이브주기;

        /// <summary>
        /// 웨이브 로직을 초기화한다.
        /// </summary>
        /// <param name="보스웨이브주기">몇 웨이브마다 보스가 등장하는지</param>
        public 웨이브로직(int 보스웨이브주기 = 5)
        {
            this.보스웨이브주기 = 보스웨이브주기;
        }

        /// <summary>
        /// 지정된 웨이브가 보스 웨이브인지 판단한다.
        /// WaveManager.cs의 _현재웨이브 % 보스웨이브주기 == 0 조건과 동일.
        /// </summary>
        /// <param name="웨이브번호">판단할 웨이브 번호 (1부터 시작)</param>
        /// <returns>보스 웨이브 여부</returns>
        public bool 보스웨이브여부(int 웨이브번호) => 웨이브번호 % 보스웨이브주기 == 0;

        /// <summary>
        /// 웨이브 번호에 따른 일반 적 생성 수를 계산한다.
        /// WaveManager.cs의 자동 생성 공식: 3 + 웨이브번호 * 2
        /// </summary>
        /// <param name="웨이브번호">현재 웨이브 번호 (1부터 시작)</param>
        /// <returns>생성할 일반 적 수</returns>
        public int 일반적생성수계산(int 웨이브번호) => 3 + 웨이브번호 * 2;

        /// <summary>
        /// 웨이브 번호에 따른 적 생성 간격(초)을 계산한다.
        /// WaveManager.cs의 자동 생성 공식: Max(0.3, 1.5 - 웨이브번호 * 0.05)
        /// </summary>
        /// <param name="웨이브번호">현재 웨이브 번호 (1부터 시작)</param>
        /// <returns>적 생성 간격 (초)</returns>
        public float 생성간격계산(int 웨이브번호)
        {
            float 간격 = 1.5f - 웨이브번호 * 0.05f;
            return MathF.Max(0.3f, 간격);
        }

        /// <summary>
        /// 웨이브 클리어 보상 골드를 계산한다.
        /// WaveManager.cs의 자동 보상 공식: 50 + 웨이브번호 * 10
        /// </summary>
        /// <param name="웨이브번호">클리어한 웨이브 번호</param>
        /// <returns>보상 골드 양</returns>
        public int 클리어보상골드계산(int 웨이브번호) => 50 + 웨이브번호 * 10;
    }
}
