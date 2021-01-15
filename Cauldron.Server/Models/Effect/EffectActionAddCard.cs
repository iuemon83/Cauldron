using Cauldron.Server.Models.Effect.Value;
using System.Linq;

namespace Cauldron.Server.Models.Effect
{
    public record EffectActionAddCard(ZoneValue ZoneToAddCard, Choice Choice) : IEffectAction
    {
        public (bool, EffectEventArgs) Execute(Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            var newCardDefs = effectEventArgs.GameMaster.ChoiceCards(effectOwnerCard, this.Choice, effectEventArgs).CardDefList;

            var owner = effectEventArgs.GameMaster.PlayersById[effectOwnerCard.OwnerId];
            var opponent = effectEventArgs.GameMaster.GetOpponent(effectOwnerCard.OwnerId);
            var newCards = newCardDefs.Select(cd => effectEventArgs.GameMaster.GenerateNewCard(cd.Id, owner.Id));

            var zone = this.ZoneToAddCard.Calculate(effectOwnerCard, effectEventArgs);

            if (!zone.Any())
            {
                return (false, effectEventArgs);
            }

            var done = false;
            switch (zone[0])
            {
                case ZonePrettyName.YouHand:
                    foreach (var newCard in newCards)
                    {
                        effectEventArgs.GameMaster.AddHand(owner, newCard);

                        done = true;
                    }
                    break;

                case ZonePrettyName.OpponentHand:
                    foreach (var newCard in newCards)
                    {
                        effectEventArgs.GameMaster.AddHand(opponent, newCard);

                        done = true;
                    }
                    break;

                case ZonePrettyName.YouField:
                    foreach (var newCard in newCards)
                    {
                        effectEventArgs.GameMaster.PlayDirect(effectOwnerCard.OwnerId, newCard.Id);

                        done = true;
                    }
                    break;

                case ZonePrettyName.OpponentField:
                    foreach (var newCard in newCards)
                    {
                        effectEventArgs.GameMaster.PlayDirect(opponent.Id, newCard.Id);

                        done = true;
                    }
                    break;

                default:
                    break;
            }

            return (done, effectEventArgs);
        }
    }
}
