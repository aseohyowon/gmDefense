// 웨이브로직테스트.cs
// 웨이브로직 클래스에 대한 단위 테스트.
// WaveManager.cs의 보스 웨이브 주기, 난이도 스케일링 계산을 검증한다.

using NUnit.Framework;
using GMDefense.Tests.GameLogic;

namespace GMDefense.Tests
{
    /// <summary>
    /// 웨이브 로직 단위 테스트 클래스.
    /// </summary>
    [TestFixture]
    public class 웨이브로직테스트
    {
        private 웨이브로직 _웨이브로직 = null!;

        [SetUp]
        public void 테스트_초기화()
        {
            // 기본 설정: 5웨이브마다 보스 등장
            _웨이브로직 = new 웨이브로직(보스웨이브주기: 5);
        }

        // ────────────────────────────────────────────────
        // 보스 웨이브 주기 테스트
        // ────────────────────────────────────────────────

        [TestCase(5,  true,  Description = "5웨이브는 보스 웨이브다")]
        [TestCase(10, true,  Description = "10웨이브는 보스 웨이브다")]
        [TestCase(15, true,  Description = "15웨이브는 보스 웨이브다")]
        [TestCase(1,  false, Description = "1웨이브는 일반 웨이브다")]
        [TestCase(3,  false, Description = "3웨이브는 일반 웨이브다")]
        [TestCase(7,  false, Description = "7웨이브는 일반 웨이브다")]
        public void 보스웨이브여부_웨이브번호별_판단(int 웨이브번호, bool 기대값)
        {
            Assert.That(_웨이브로직.보스웨이브여부(웨이브번호), Is.EqualTo(기대값));
        }

        [Test]
        [Description("보스 주기를 3으로 설정하면 3, 6, 9웨이브가 보스다")]
        public void 보스웨이브여부_주기3_3배수가보스()
        {
            var 주기3로직 = new 웨이브로직(보스웨이브주기: 3);

            Assert.That(주기3로직.보스웨이브여부(3), Is.True);
            Assert.That(주기3로직.보스웨이브여부(6), Is.True);
            Assert.That(주기3로직.보스웨이브여부(9), Is.True);
            Assert.That(주기3로직.보스웨이브여부(2), Is.False);
            Assert.That(주기3로직.보스웨이브여부(4), Is.False);
        }

        // ────────────────────────────────────────────────
        // 적 생성 수 스케일링 테스트
        // ────────────────────────────────────────────────

        [Test]
        [Description("1웨이브 적 생성 수는 5이다 (3 + 1×2)")]
        public void 일반적생성수_웨이브1_5마리()
        {
            Assert.That(_웨이브로직.일반적생성수계산(1), Is.EqualTo(5));
        }

        [Test]
        [Description("5웨이브 적 생성 수는 13이다 (3 + 5×2)")]
        public void 일반적생성수_웨이브5_13마리()
        {
            Assert.That(_웨이브로직.일반적생성수계산(5), Is.EqualTo(13));
        }

        [Test]
        [Description("웨이브가 높아질수록 적 생성 수가 증가한다")]
        public void 일반적생성수_연속웨이브_단조증가()
        {
            for (int 웨이브 = 1; 웨이브 < 20; 웨이브++)
            {
                Assert.That(
                    _웨이브로직.일반적생성수계산(웨이브 + 1),
                    Is.GreaterThan(_웨이브로직.일반적생성수계산(웨이브)),
                    $"웨이브 {웨이브 + 1}의 적 수가 웨이브 {웨이브}보다 많아야 한다"
                );
            }
        }

        // ────────────────────────────────────────────────
        // 생성 간격 스케일링 테스트
        // ────────────────────────────────────────────────

        [Test]
        [Description("1웨이브 생성 간격은 1.45초다 (1.5 - 1×0.05)")]
        public void 생성간격_웨이브1_1초45()
        {
            Assert.That(_웨이브로직.생성간격계산(1), Is.EqualTo(1.45f).Within(0.001f));
        }

        [Test]
        [Description("생성 간격은 0.3초 미만으로 내려가지 않는다 (최소값 보장)")]
        public void 생성간격_높은웨이브_최소값보장()
        {
            // 공식상 0.3 이하가 되는 웨이브: 1.5 - n*0.05 < 0.3 → n > 24
            Assert.That(_웨이브로직.생성간격계산(25), Is.EqualTo(0.3f).Within(0.001f));
            Assert.That(_웨이브로직.생성간격계산(50), Is.EqualTo(0.3f).Within(0.001f));
            Assert.That(_웨이브로직.생성간격계산(100), Is.EqualTo(0.3f).Within(0.001f));
        }

        [Test]
        [Description("초반 웨이브에서는 간격이 0.3초보다 길다")]
        public void 생성간격_초반웨이브_0초3이상()
        {
            for (int 웨이브 = 1; 웨이브 <= 10; 웨이브++)
            {
                Assert.That(_웨이브로직.생성간격계산(웨이브), Is.GreaterThanOrEqualTo(0.3f),
                    $"웨이브 {웨이브}의 생성 간격은 최소 0.3초여야 한다");
            }
        }

        // ────────────────────────────────────────────────
        // 클리어 보상 골드 계산 테스트
        // ────────────────────────────────────────────────

        [Test]
        [Description("1웨이브 클리어 보상은 60골드다 (50 + 1×10)")]
        public void 클리어보상_웨이브1_60골드()
        {
            Assert.That(_웨이브로직.클리어보상골드계산(1), Is.EqualTo(60));
        }

        [Test]
        [Description("5웨이브 클리어 보상은 100골드다 (50 + 5×10)")]
        public void 클리어보상_웨이브5_100골드()
        {
            Assert.That(_웨이브로직.클리어보상골드계산(5), Is.EqualTo(100));
        }

        [Test]
        [Description("웨이브가 높아질수록 보상 골드가 증가한다")]
        public void 클리어보상_연속웨이브_단조증가()
        {
            for (int 웨이브 = 1; 웨이브 < 20; 웨이브++)
            {
                Assert.That(
                    _웨이브로직.클리어보상골드계산(웨이브 + 1),
                    Is.GreaterThan(_웨이브로직.클리어보상골드계산(웨이브)),
                    $"웨이브 {웨이브 + 1} 보상이 웨이브 {웨이브}보다 많아야 한다"
                );
            }
        }
    }
}
