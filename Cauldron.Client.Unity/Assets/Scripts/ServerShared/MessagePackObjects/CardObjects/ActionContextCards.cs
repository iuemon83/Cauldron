﻿using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class ActionContextCards
    {
        public ActionContextCardsOfAddEffect ActionContextCardsOfAddEffect { get; }
        public ActionContextCardsOfDestroyCard ActionContextCardsOfDestroyCard { get; }
        public ActionContextCardsOfMoveCard ActionContextCardsOfMoveCard { get; }
        public ActionContextCardsOfExcludeCard ActionContextCardsOfExcludeCard { get; }

        public ActionContextCards(
            ActionContextCardsOfAddEffect ActionContextCardsOfAddEffect = null,
            ActionContextCardsOfDestroyCard ActionContextCardsOfDestroyCard = null,
            ActionContextCardsOfMoveCard ActionContextCardsOfMoveCard = null,
            ActionContextCardsOfExcludeCard ActionContextCardsOfExcludeCard = null
            )
        {
            this.ActionContextCardsOfAddEffect = ActionContextCardsOfAddEffect;
            this.ActionContextCardsOfDestroyCard = ActionContextCardsOfDestroyCard;
            this.ActionContextCardsOfMoveCard = ActionContextCardsOfMoveCard;
            this.ActionContextCardsOfExcludeCard = ActionContextCardsOfExcludeCard;
        }
    }
}
