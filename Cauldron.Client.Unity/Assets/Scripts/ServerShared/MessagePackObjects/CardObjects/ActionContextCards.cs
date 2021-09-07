using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class ActionContextCards
    {
        public ActionContextCardsOfAddCard AddCard { get; }
        public ActionContextCardsOfAddEffect AddEffect { get; }
        public ActionContextCardsOfDamage Damage { get; }
        public ActionContextCardsOfDestroyCard DestroyCard { get; }
        public ActionContextCardsOfDrawCard DrawCard { get; }
        public ActionContextCardsOfExcludeCard ExcludeCard { get; }
        public ActionContextCardsOfModifyCard ModifyCard { get; }
        public ActionContextCardsOfModifyCounter ModifyCounter { get; }
        public ActionContextCardsOfMoveCard MoveCard { get; }

        public ActionContextCards(
            ActionContextCardsOfAddCard AddCard = null,
            ActionContextCardsOfAddEffect AddEffect = null,
            ActionContextCardsOfDamage Damage = null,
            ActionContextCardsOfDestroyCard DestroyCard = null,
            ActionContextCardsOfDrawCard DrawCard = null,
            ActionContextCardsOfExcludeCard ExcludeCard = null,
            ActionContextCardsOfModifyCard ModifyCard = null,
            ActionContextCardsOfModifyCounter ModifyCounter = null,
            ActionContextCardsOfMoveCard MoveCard = null
            )
        {
            this.AddCard = AddCard;
            this.AddEffect = AddEffect;
            this.Damage = Damage;
            this.DestroyCard = DestroyCard;
            this.DrawCard = DrawCard;
            this.ExcludeCard = ExcludeCard;
            this.ModifyCard = ModifyCard;
            this.ModifyCounter = ModifyCounter;
            this.MoveCard = MoveCard;
        }
    }
}
