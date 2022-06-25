﻿using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class OpenNewRoomRequest
    {
        public RuleBook RuleBook { get; }
        public string OwnerName { get; }
        public string Message { get; }
        public CardDefId[] DeckCardIdList { get; }

        public OpenNewRoomRequest(RuleBook ruleBook, string ownerName, string message, CardDefId[] deckCardIdList)
        {
            this.RuleBook = ruleBook;
            this.OwnerName = ownerName;
            this.Message = message;
            this.DeckCardIdList = deckCardIdList;
        }
    }
}
