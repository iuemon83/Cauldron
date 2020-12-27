using Cauldron.Server.Models;
using System.Collections.Generic;
using Xunit;

namespace Cauldron.Server_Test
{
    public class CardId_Test
    {
        [Fact]
        public void CardId_test()
        {
            var p1 = CardId.NewId();
            var p2 = CardId.Parse(p1.ToString());

            Assert.Equal(p1, p2);
            Assert.True(p1 == p2);
            Assert.False(p1 != p2);

            var p3 = CardId.NewId();

            Assert.NotEqual(p1, p3);
            Assert.False(p1 == p3);
            Assert.True(p1 != p3);

            var hash = new HashSet<CardId>
            {
                p1
            };
            Assert.Contains(p1, hash);
            Assert.Contains(p2, hash);
            Assert.DoesNotContain(p3, hash);
        }
    }
}
