using Cauldron.Shared.MessagePackObjects;
using System.Collections.Generic;
using Xunit;

namespace Cauldron.Core_Test
{
    public class GameId_Test
    {
        [Fact]
        public void GameId_test()
        {
            var p1 = GameId.NewId();
            var p2 = GameId.Parse(p1.ToString());

            Assert.Equal(p1, p2);
            Assert.True(p1 == p2);
            Assert.False(p1 != p2);

            var p3 = GameId.NewId();

            Assert.NotEqual(p1, p3);
            Assert.False(p1 == p3);
            Assert.True(p1 != p3);

            var hash = new HashSet<GameId>
            {
                p1
            };
            Assert.Contains(p1, hash);
            Assert.Contains(p2, hash);
            Assert.DoesNotContain(p3, hash);
        }
    }
}
