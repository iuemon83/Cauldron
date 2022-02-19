using Cauldron.Core.Entities;
using Cauldron.Shared.MessagePackObjects;
using Xunit;

namespace Cauldron.Core_Test
{
    public class CardPool_Test
    {
        [Fact]
        public void IsValid_valid()
        {
            var cardpool = new CardPool(new[]
            {
                new CardSet("cardset", new[]
                {
                    TestUtil.CardDef("carddef"),
                    TestUtil.CardDef("carddef2"),
                })
            });

            var actual = cardpool.IsValid();

            Assert.True(actual);
        }

        [Fact]
        public void IsValid_異なるカードセット内に同名のカード()
        {
            var cardpool = new CardPool(new[]
            {
                new CardSet("cardset", new[]
                {
                    TestUtil.CardDef("carddef"),
                    TestUtil.CardDef("carddef2"),
                }),
                new CardSet("cardset2", new[]
                {
                    TestUtil.CardDef("carddef"),
                    TestUtil.CardDef("carddef2"),
                })
            });

            var actual = cardpool.IsValid();

            Assert.True(actual);
        }

        [Fact]
        public void IsValid_空()
        {
            var cardpool = new CardPool(System.Array.Empty<CardSet>());

            var actual = cardpool.IsValid();

            Assert.True(actual);
        }

        [Fact]
        public void IsValid_一つのカードセット内に同名のカード()
        {
            var cardpool = new CardPool(new[]
            {
                new CardSet("cardset", new[]
                {
                    TestUtil.CardDef("carddef"),
                    TestUtil.CardDef("carddef"),
                }),
                new CardSet("cardset2", new[]
                {
                    TestUtil.CardDef("carddef"),
                    TestUtil.CardDef("carddef2"),
                })
            });

            var actual = cardpool.IsValid();

            Assert.False(actual);
        }

        [Fact]
        public void IsValid_同名のカードセット()
        {
            var cardpool = new CardPool(new[]
            {
                new CardSet("cardset", new[]
                {
                    TestUtil.CardDef("carddef"),
                    TestUtil.CardDef("carddef2"),
                }),
                new CardSet("cardset", new[]
                {
                    TestUtil.CardDef("carddef"),
                    TestUtil.CardDef("carddef2"),
                })
            });

            var actual = cardpool.IsValid();

            Assert.False(actual);
        }
    }
}
