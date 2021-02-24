using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class ActionContextCards
    {
        public ActionContextCardsOfAddEffect ActionContextCardsOfAddEffect { get; set; }
        public ActionContextCardsOfDestroyCard ActionContextCardsOfDestroyCard { get; set; }
        public ActionContextCardsOfMoveCard ActionContextCardsOfMoveCard { get; set; }

        public ActionContextCards(
            ActionContextCardsOfAddEffect ActionContextCardsOfAddEffect = null,
            ActionContextCardsOfDestroyCard ActionContextCardsOfDestroyCard = null,
            ActionContextCardsOfMoveCard ActionContextCardsOfMoveCard = null
            )
        {
            this.ActionContextCardsOfAddEffect = ActionContextCardsOfAddEffect;
            this.ActionContextCardsOfDestroyCard = ActionContextCardsOfDestroyCard;
            this.ActionContextCardsOfMoveCard = ActionContextCardsOfMoveCard;
        }
    }
}
