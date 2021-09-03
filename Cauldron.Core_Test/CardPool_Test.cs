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
                    new CardDef()
                    {
                        Name = "carddef"
                    },
                    new CardDef()
                    {
                        Name = "carddef2"
                    }
                })
            });

            var actual = cardpool.IsValid();

            Assert.True(actual);
        }

        [Fact]
        public void IsValid_valid2()
        {
            var cardpool = new CardPool(new[]
            {
                new CardSet("cardset", new[]
                {
                    new CardDef()
                    {
                        Name = "carddef"
                    },
                    new CardDef()
                    {
                        Name = "carddef2"
                    }
                }),
                new CardSet("cardset2", new[]
                {
                    new CardDef()
                    {
                        Name = "carddef"
                    },
                    new CardDef()
                    {
                        Name = "carddef2"
                    }
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
                    new CardDef()
                    {
                        Name = "carddef"
                    },
                    new CardDef()
                    {
                        Name = "carddef"
                    }
                }),
                new CardSet("cardset2", new[]
                {
                    new CardDef()
                    {
                        Name = "carddef"
                    },
                    new CardDef()
                    {
                        Name = "carddef2"
                    }
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
                    new CardDef()
                    {
                        Name = "carddef"
                    },
                    new CardDef()
                    {
                        Name = "carddef2"
                    }
                }),
                new CardSet("cardset", new[]
                {
                    new CardDef()
                    {
                        Name = "carddef"
                    },
                    new CardDef()
                    {
                        Name = "carddef2"
                    }
                })
            });

            var actual = cardpool.IsValid();

            Assert.False(actual);
        }
    }
}
