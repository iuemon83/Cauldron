using System.Linq;

namespace Cauldron.Server.Models.Effect
{
    public class EffectActionAddCard
    {
        public Choice Choice { get; set; }
        public ZoneType ZoneToAddCard { get; set; }

        public void Execute(Card ownerCard, EffectEventArgs args) => this.Execute(args.GameMaster, ownerCard, args.Source);

        public void Execute(GameMaster gameMaster, Card ownerCard, Card eventSource)
        {
            var newCardDefs = gameMaster.ChoiceCards(ownerCard, this.Choice, eventSource).CardDefList;

            var owner = gameMaster.PlayersById[ownerCard.OwnerId];
            var newCards = newCardDefs.Select(cd => gameMaster.GenerateNewCard(cd.Id, owner.Id));

            switch (this.ZoneToAddCard)
            {
                case ZoneType.YouHand:
                    foreach (var newCard in newCards)
                    {
                        gameMaster.AddHand(owner, newCard);
                    }
                    break;

                case ZoneType.YouField:
                    foreach (var newCard in newCards)
                    {
                        gameMaster.PlayDirect(ownerCard.OwnerId, newCard.Id);
                    }
                    break;

                default:
                    return;
            }
        }
    }
}
