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
        public void IsValid_�قȂ�J�[�h�Z�b�g���ɓ����̃J�[�h()
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
        public void IsValid_��()
        {
            var cardpool = new CardPool(System.Array.Empty<CardSet>());

            var actual = cardpool.IsValid();

            Assert.True(actual);
        }

        [Fact]
        public void IsValid_��̃J�[�h�Z�b�g���ɓ����̃J�[�h()
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
        public void IsValid_�����̃J�[�h�Z�b�g()
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
