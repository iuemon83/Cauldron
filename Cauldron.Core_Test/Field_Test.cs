using Cauldron.Core.Entities;
using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.MessagePackObjects.Value;
using System.Threading.Tasks;
using Xunit;

namespace Cauldron.Core_Test
{

    /// <summary>
    /// Fieldのテスト
    /// </summary>
    public class Field_Test
    {
        [Fact]
        public void コンストラクタ()
        {
            var field = new Field(TestUtil.TestRuleBook(StartMaxNumFields: 1, MaxNumFields: 2));

            Assert.Equal(1, field.CurrentLimit);
            Assert.Equal(2, field.Limit);
            Assert.Equal(0, field.Count);
            Assert.False(field.Full);
        }

        [Fact]
        public void Add_場所自動()
        {
            var field = new Field(TestUtil.TestRuleBook(StartMaxNumFields: 1, MaxNumFields: 2));

            Assert.Equal(0, field.Count);

            var actual = field.Add(Card.Empty);

            Assert.Equal(1, field.Count);
            Assert.Equal(1, field.AllCards.Count);
            Assert.Equal(0, actual);
        }

        [Fact]
        public void Add_場所指定()
        {
            static void test(int testIndex, int expectedCount, int expectedReturn)
            {
                var field = new Field(TestUtil.TestRuleBook(StartMaxNumFields: 3, MaxNumFields: 3));

                Assert.Equal(0, field.Count);

                var actual = field.Add(Card.Empty, testIndex);

                Assert.Equal(expectedCount, field.Count);
                Assert.Equal(expectedCount, field.AllCards.Count);
                Assert.Equal(expectedReturn, actual);
            }

            test(1, 1, 1);

            // 上限越え
            test(4, 0, -1);

            // < 0
            test(-1, 0, -1);
        }

        [Fact]
        public void Remove()
        {
            var field = new Field(TestUtil.TestRuleBook(StartMaxNumFields: 1, MaxNumFields: 2));

            var card = Card.Empty;

            field.Add(card);

            field.Remove(card);

            Assert.Equal(0, field.Count);
            Assert.Equal(0, field.AllCards.Count);
        }

        [Fact]
        public void 数を増やす()
        {
            static void test(int testLimit, int expectedCurrentLimit, int expectedReturn)
            {
                var field = new Field(TestUtil.TestRuleBook(StartMaxNumFields: 1, MaxNumFields: 3));
                var actual = field.UpdateLimit(testLimit);

                Assert.Equal(expectedCurrentLimit, field.CurrentLimit);
                Assert.Equal(3, field.Limit);
                Assert.Equal(expectedReturn, actual);

            }

            // 上限-1
            test(2, 2, 1);

            // 上限
            test(3, 3, 2);

            // 上限+1
            test(4, 3, 2);
        }

        [Fact]
        public void 数を減らす()
        {
            static void test(int testLimit, int expectedCurrentLimit, int expectedReturn)
            {
                var field = new Field(TestUtil.TestRuleBook(StartMaxNumFields: 2, MaxNumFields: 3));
                var actual = field.UpdateLimit(testLimit);

                Assert.Equal(expectedCurrentLimit, field.CurrentLimit);
                Assert.Equal(3, field.Limit);
                Assert.Equal(expectedReturn, actual);
            }

            // > 0
            test(1, 1, -1);

            // = 0
            test(0, 0, -2);

            // < 0
            test(-1, 0, -2);
        }

        [Fact]
        public void 数を減らす_カードあり()
        {
            var testLimit = 0;
            var expectedCurrentLimit = 1;
            var expectedReturn = -1;

            var field = new Field(TestUtil.TestRuleBook(StartMaxNumFields: 2, MaxNumFields: 3));
            field.Add(Card.Empty, 1);
            var actual = field.UpdateLimit(testLimit);

            // 1枚カードがあるので、1残る
            Assert.Equal(expectedCurrentLimit, field.CurrentLimit);
            Assert.Equal(3, field.Limit);
            Assert.Equal(expectedReturn, actual);

            // 数が減ってるので置けない
            var actualAddResult = field.Add(Card.Empty);
            Assert.Equal(-1, actualAddResult);
        }
    }
}
