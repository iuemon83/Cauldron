using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using Xunit;

namespace Cauldron.Core_Test
{

    /// <summary>
    /// PositionConditionExtentionsのテスト
    /// </summary>
    public class PositionConditionExtentions_Test
    {
        [Fact]
        public void 同じ場()
        {
            static void AssertSameField(int i1, int i2, int x, int y, bool expected)
            {
                var p1 = PlayerId.NewId();

                var actual = PositionConditionExtentions.IsMatch(
                    new Zone(p1, ZoneName.Field), i1,
                    new Zone(p1, ZoneName.Field), i2,
                    x, y);

                Assert.Equal(expected, actual);
            }

            // 0
            AssertSameField(
                1, 1,
                0, 0,
                true);

            // +1
            AssertSameField(
                1, 2,
                1, 0,
                true);

            // -1
            AssertSameField(
                1, 0,
                -1, 0,
                true);

            // 違う
            AssertSameField(
                2, 1,
                0, 0,
                false);

            // 違う
            AssertSameField(
                1, 1,
                -1, 0,
                false);

            // yが違う
            AssertSameField(
                1, 1,
                0, 1,
                false);
        }

        [Fact]
        public void 違う場()
        {
            static void AssertDiffField(int i1, int i2, int x, int y, bool expected)
            {
                var p1 = PlayerId.NewId();
                var p2 = PlayerId.NewId();

                var actual = PositionConditionExtentions.IsMatch(
                    new Zone(p1, ZoneName.Field), i1,
                    new Zone(p2, ZoneName.Field), i2,
                    x, y);

                Assert.Equal(expected, actual);
            }

            // 0
            AssertDiffField(
                1, 1,
                0, 1,
                true);

            // +1
            AssertDiffField(
                1, 2,
                1, 1,
                true);

            // -1
            AssertDiffField(
                1, 0,
                -1, 1,
                true);

            // 違う
            AssertDiffField(
                2, 1,
                0, 1,
                false);

            // 違う
            AssertDiffField(
                1, 1,
                -1, 1,
                false);

            // yが違う
            AssertDiffField(
                1, 1,
                0, 0,
                false);
        }
    }
}
