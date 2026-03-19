// 골드시스템로직.cs
// GoldSystem.cs의 순수 게임 로직을 Unity 없이 테스트하기 위한 클래스.
// MonoBehaviour를 상속받지 않고 동일한 골드 관리 로직을 구현한다.

namespace GMDefense.Tests.GameLogic
{
    /// <summary>
    /// Unity 의존성 없는 골드 시스템 로직 클래스.
    /// GoldSystem.cs와 동일한 비즈니스 로직을 갖는다.
    /// </summary>
    public class 골드시스템로직
    {
        // 현재 골드 잔액
        private int _현재골드;
        // 현재 소환 비용
        private int _현재소환비용;
        // 총 소환 횟수
        private int _소환횟수;

        // 기본 소환 비용
        public readonly int 기본소환비용;
        // 소환 횟수당 비용 증가량
        public readonly int 소환비용증가량;

        // 골드 변경 이벤트 (변경 후 잔액 전달)
        public Action<int>? 골드변경이벤트;

        /// <summary>
        /// 골드 시스템을 초기화한다.
        /// </summary>
        /// <param name="초기골드">게임 시작 시 골드</param>
        /// <param name="기본소환비용">첫 소환 비용</param>
        /// <param name="소환비용증가량">소환 횟수당 비용 증가량</param>
        public 골드시스템로직(int 초기골드 = 200, int 기본소환비용 = 100, int 소환비용증가량 = 10)
        {
            _현재골드 = 초기골드;
            this.기본소환비용 = 기본소환비용;
            this.소환비용증가량 = 소환비용증가량;
            _현재소환비용 = 기본소환비용;
            _소환횟수 = 0;
        }

        /// <summary>현재 골드 잔액 (읽기 전용)</summary>
        public int 현재골드 => _현재골드;

        /// <summary>현재 소환 비용 (읽기 전용)</summary>
        public int 현재소환비용 => _현재소환비용;

        /// <summary>총 소환 횟수 (읽기 전용)</summary>
        public int 소환횟수 => _소환횟수;

        /// <summary>
        /// 골드를 추가한다.
        /// </summary>
        /// <param name="양">추가할 양 (양수만 유효)</param>
        public void 골드추가(int 양)
        {
            if (양 <= 0) return;
            _현재골드 += 양;
            골드변경이벤트?.Invoke(_현재골드);
        }

        /// <summary>
        /// 골드를 소비한다.
        /// </summary>
        /// <param name="양">소비할 양 (양수만 유효)</param>
        /// <returns>소비 성공 여부 (잔액 부족 시 false)</returns>
        public bool 골드소비(int 양)
        {
            if (양 <= 0) return false;
            if (_현재골드 < 양) return false;

            _현재골드 -= 양;
            골드변경이벤트?.Invoke(_현재골드);
            return true;
        }

        /// <summary>
        /// 소환 비용을 지불하고 소환 횟수를 증가시킨다.
        /// 지불 성공 시 다음 소환 비용이 증가한다.
        /// </summary>
        /// <returns>지불 성공 여부</returns>
        public bool 소환비용지불()
        {
            if (!골드소비(_현재소환비용)) return false;

            _소환횟수++;
            _현재소환비용 = 기본소환비용 + _소환횟수 * 소환비용증가량;
            return true;
        }

        /// <summary>
        /// 현재 골드가 지정된 금액 이상인지 확인한다.
        /// </summary>
        public bool 골드충분(int 필요골드) => _현재골드 >= 필요골드;
    }
}
