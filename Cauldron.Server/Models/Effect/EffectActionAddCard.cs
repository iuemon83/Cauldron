using System.Linq;

namespace Cauldron.Server.Models.Effect
{
    public class EffectActionAddCard : IEffectAction
    {
        public Choice Choice { get; set; }
        public ZonePrettyName ZoneToAddCard { get; set; }

        public (bool, EffectEventArgs) Execute(Card ownerCard, EffectEventArgs args)
        {
            var newCardDefs = args.GameMaster.ChoiceCards(ownerCard, this.Choice, args).CardDefList;

            var owner = args.GameMaster.PlayersById[ownerCard.OwnerId];
            var newCards = newCardDefs.Select(cd => args.GameMaster.GenerateNewCard(cd.Id, owner.Id));

            var done = false;
            switch (this.ZoneToAddCard)
            {
                case ZonePrettyName.YouHand:
                    foreach (var newCard in newCards)
                    {
                        args.GameMaster.AddHand(owner, newCard);

                        done = true;
                    }
                    break;

                case ZonePrettyName.YouField:
                    foreach (var newCard in newCards)
                    {
                        args.GameMaster.PlayDirect(ownerCard.OwnerId, newCard.Id);

                        done = true;
                    }
                    break;

                default:
                    break;
            }

            return (done, args);
        }
    }
}
