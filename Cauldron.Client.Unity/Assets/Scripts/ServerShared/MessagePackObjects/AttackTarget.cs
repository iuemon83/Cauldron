#nullable enable

using MessagePack;
using System.Linq;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class AttackTarget
    {
        public PlayerId[] PlayerIdList { get; }
        public CardId[] CardIdList { get; }

        public AttackTarget(PlayerId[] playerIdList, CardId[] cardIdList)
        {
            this.PlayerIdList = playerIdList;
            this.CardIdList = cardIdList;
        }

        public bool Any => this.PlayerIdList.Any() || this.CardIdList.Any();
    }
}
