// 영웅데이터테스트.cs
// 영웅데이터로직 클래스에 대한 단위 테스트.
// HeroData.cs의 합성 가능 여부 조건 및 최종 공격력 계산을 검증한다.

using NUnit.Framework;
using GMDefense.Tests.GameLogic;

namespace GMDefense.Tests
{
    /// <summary>
    /// 영웅 데이터 로직 단위 테스트 클래스.
    /// </summary>
    [TestFixture]
    public class 영웅데이터테스트
    {
        // ────────────────────────────────────────────────
        // 합성 가능 여부 테스트
        // ────────────────────────────────────────────────

        [Test]
        [Description("같은 종류 + 같은 등급 영웅은 합성 가능하다")]
        public void 합성가능_같은종류같은등급_성공()
        {
            var 영웅A = new 영웅데이터로직 { 영웅이름 = "전사A", 종류 = 영웅종류.전사, 등급 = 영웅등급.등급1 };
            var 영웅B = new 영웅데이터로직 { 영웅이름 = "전사B", 종류 = 영웅종류.전사, 등급 = 영웅등급.등급1 };

            Assert.That(영웅A.합성가능(영웅B), Is.True);
        }

        [Test]
        [Description("다른 종류 영웅은 등급이 같아도 합성 불가다")]
        public void 합성가능_다른종류같은등급_실패()
        {
            var 영웅A = new 영웅데이터로직 { 종류 = 영웅종류.전사, 등급 = 영웅등급.등급1 };
            var 영웅B = new 영웅데이터로직 { 종류 = 영웅종류.궁수, 등급 = 영웅등급.등급1 };

            Assert.That(영웅A.합성가능(영웅B), Is.False);
        }

        [Test]
        [Description("같은 종류 영웅이라도 등급이 다르면 합성 불가다")]
        public void 합성가능_같은종류다른등급_실패()
        {
            var 영웅A = new 영웅데이터로직 { 종류 = 영웅종류.마법사, 등급 = 영웅등급.등급1 };
            var 영웅B = new 영웅데이터로직 { 종류 = 영웅종류.마법사, 등급 = 영웅등급.등급2 };

            Assert.That(영웅A.합성가능(영웅B), Is.False);
        }

        [Test]
        [Description("대상이 null이면 합성 불가다")]
        public void 합성가능_대상null_실패()
        {
            var 영웅A = new 영웅데이터로직 { 종류 = 영웅종류.전사, 등급 = 영웅등급.등급1 };

            Assert.That(영웅A.합성가능(null), Is.False);
        }

        [Test]
        [Description("모든 7가지 영웅 종류에 대해 같은 종류끼리 합성 가능하다")]
        public void 합성가능_전체영웅종류_각각합성가능()
        {
            var 모든종류 = Enum.GetValues<영웅종류>();

            foreach (var 종류 in 모든종류)
            {
                var 영웅A = new 영웅데이터로직 { 종류 = 종류, 등급 = 영웅등급.등급1 };
                var 영웅B = new 영웅데이터로직 { 종류 = 종류, 등급 = 영웅등급.등급1 };

                Assert.That(영웅A.합성가능(영웅B), Is.True,
                    $"종류 '{종류}'끼리 합성 가능해야 한다");
            }
        }

        [Test]
        [Description("모든 7단계 등급에서 합성 가능 조건이 정상 동작한다")]
        public void 합성가능_전체등급_각등급내합성가능()
        {
            var 모든등급 = Enum.GetValues<영웅등급>();

            foreach (var 등급 in 모든등급)
            {
                var 영웅A = new 영웅데이터로직 { 종류 = 영웅종류.전사, 등급 = 등급 };
                var 영웅B = new 영웅데이터로직 { 종류 = 영웅종류.전사, 등급 = 등급 };

                Assert.That(영웅A.합성가능(영웅B), Is.True,
                    $"등급 '{등급}' 전사끼리 합성 가능해야 한다");
            }
        }

        // ────────────────────────────────────────────────
        // 최종 공격력 계산 테스트
        // ────────────────────────────────────────────────

        [Test]
        [Description("등급1 영웅의 최종 공격력은 기본 공격력 × 1 이다")]
        public void 최종공격력_등급1_기본공격력그대로()
        {
            var 영웅 = new 영웅데이터로직 { 공격력 = 10f, 등급 = 영웅등급.등급1 };

            Assert.That(영웅.최종공격력, Is.EqualTo(10f));
        }

        [Test]
        [Description("등급2 영웅의 최종 공격력은 기본 공격력 × 2 이다")]
        public void 최종공격력_등급2_두배()
        {
            var 영웅 = new 영웅데이터로직 { 공격력 = 10f, 등급 = 영웅등급.등급2 };

            Assert.That(영웅.최종공격력, Is.EqualTo(20f));
        }

        [Test]
        [Description("등급7 영웅의 최종 공격력은 기본 공격력 × 7 이다")]
        public void 최종공격력_등급7_일곱배()
        {
            var 영웅 = new 영웅데이터로직 { 공격력 = 10f, 등급 = 영웅등급.등급7 };

            Assert.That(영웅.최종공격력, Is.EqualTo(70f));
        }

        [TestCase(영웅등급.등급1, 10f, ExpectedResult = 10f)]
        [TestCase(영웅등급.등급2, 10f, ExpectedResult = 20f)]
        [TestCase(영웅등급.등급3, 10f, ExpectedResult = 30f)]
        [TestCase(영웅등급.등급4, 10f, ExpectedResult = 40f)]
        [TestCase(영웅등급.등급5, 10f, ExpectedResult = 50f)]
        [TestCase(영웅등급.등급6, 10f, ExpectedResult = 60f)]
        [TestCase(영웅등급.등급7, 10f, ExpectedResult = 70f)]
        [Description("등급별 최종 공격력 계산이 정확하다 (기본 공격력 10 기준)")]
        public float 최종공격력_등급별계산_정확(영웅등급 등급, float 기본공격력)
        {
            var 영웅 = new 영웅데이터로직 { 공격력 = 기본공격력, 등급 = 등급 };
            return 영웅.최종공격력;
        }

        // ────────────────────────────────────────────────
        // 최대 등급 여부 테스트
        // ────────────────────────────────────────────────

        [Test]
        [Description("등급7 영웅은 최대 등급이다")]
        public void 최대등급여부_등급7_참()
        {
            var 영웅 = new 영웅데이터로직 { 등급 = 영웅등급.등급7 };
            Assert.That(영웅.최대등급여부, Is.True);
        }

        [Test]
        [Description("등급6 이하 영웅은 최대 등급이 아니다")]
        public void 최대등급여부_등급6이하_거짓()
        {
            foreach (영웅등급 등급 in new[] { 영웅등급.등급1, 영웅등급.등급2, 영웅등급.등급3,
                                              영웅등급.등급4, 영웅등급.등급5, 영웅등급.등급6 })
            {
                var 영웅 = new 영웅데이터로직 { 등급 = 등급 };
                Assert.That(영웅.최대등급여부, Is.False, $"등급{(int)등급}은 최대 등급이 아니어야 한다");
            }
        }
    }
}
