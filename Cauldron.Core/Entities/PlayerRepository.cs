using Cauldron.Shared.MessagePackObjects;
using System.Collections.Concurrent;

namespace Cauldron.Core.Entities
{
    public class PlayerRepository
    {
        private readonly ConcurrentDictionary<PlayerId, Player> PlayersById = new();

        public IReadOnlyList<Player> AllPlayers => this.PlayersById.Values.ToArray();

        public IReadOnlyList<Player> SortedPlayers => this.PlayersById.Values.OrderBy(p => p.PlayOrder).ToArray();

        public IReadOnlyList<Player> Alives => this.PlayersById.Values.Where(p => p.CurrentHp > 0).ToArray();

        public (bool exists, Player value) TryGet(PlayerId id)
        {
            return this.PlayersById.TryGetValue(id, out var value)
                ? (true, value)
                : (false, default!);
        }

        public IEnumerable<Player> TryList(IReadOnlyList<PlayerId> idList)
        {
            return idList
                .Select(playerId => this.TryGet(playerId))
                .Where(x => x.exists)
                .Select(x => x.value);
        }

        public Player CreateNew(PlayerDef playerDef, RuleBook ruleBook, Card[] deckCards, int order)
        {
            var player = new Player(playerDef.Id, playerDef.Name, ruleBook, deckCards, order);
            this.PlayersById.TryAdd(player.Id, player);

            return player;
        }

        public Player[] Opponents(PlayerId id) => this.AllPlayers.Where(p => p.Id != id).ToArray();
    }
}
