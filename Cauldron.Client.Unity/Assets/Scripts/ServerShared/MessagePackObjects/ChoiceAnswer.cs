using MessagePack;
using System;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class ChoiceAnswer
    {
        public PlayerId[] PlayerIdList { get; }
        public CardId[] CardIdList { get; }
        public CardDefId[] CardDefIdList { get; }

        public int Count() => (this.PlayerIdList?.Length ?? 0)
            + (this.CardIdList?.Length ?? 0)
            + (this.CardDefIdList?.Length ?? 0);

        public ChoiceAnswer(PlayerId[] PlayerIdList, CardId[] CardIdList, CardDefId[] CardDefIdList)
        {
            this.PlayerIdList = PlayerIdList ?? Array.Empty<PlayerId>();
            this.CardIdList = CardIdList ?? Array.Empty<CardId>();
            this.CardDefIdList = CardDefIdList ?? Array.Empty<CardDefId>();
        }
    }
}
