using Cauldron.Server.Models;
using System.Collections.Generic;
using Xunit;

namespace Cauldron.Server_Test
{
    public class CardDefId_Test
    {
        [Fact]
        public void CardDefId_test()
        {
            var p1 = CardDefId.NewId();
            var p2 = CardDefId.Parse(p1.ToString());

            Assert.Equal(p1, p2);
            Assert.True(p1 == p2);
            Assert.False(p1 != p2);

            var p3 = CardDefId.NewId();

            Assert.NotEqual(p1, p3);
            Assert.False(p1 == p3);
            Assert.True(p1 != p3);

            var hash = new HashSet<CardDefId>
            {
                p1
            };
            Assert.Contains(p1, hash);
            Assert.Contains(p2, hash);
            Assert.DoesNotContain(p3, hash);
        }
    }
}
