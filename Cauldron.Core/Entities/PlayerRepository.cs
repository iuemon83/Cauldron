using Cauldron.Shared.MessagePackObjects;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Cauldron.Core.Entities
{
    public class PlayerRepository
    {
        private readonly ConcurrentDictionary<PlayerId, Player> PlayersById = new();

        public IReadOnlyList<Player> AllPlayers => this.PlayersById.Values.ToArray();

        public IReadOnlyList<Player> Alives => this.PlayersById.Values.Where(p => p.CurrentHp > 0).ToArray();

        public (bool exists, Player value) TryGet(PlayerId id)
        {
            return this.PlayersById.TryGetValue(id, out var value)
                ? (true, value)
                : (false, default);
        }

        public Player CreateNew(PlayerDef playerDef, RuleBook ruleBook, Card[] deckCards)
        {
            var player = new Player(playerDef.Id, playerDef.Name, ruleBook, deckCards);
            this.PlayersById.TryAdd(player.Id, player);

            return player;
        }

        public Player[] Opponents(PlayerId id) => this.AllPlayers.Where(p => p.Id != id).ToArray();
    }
}
