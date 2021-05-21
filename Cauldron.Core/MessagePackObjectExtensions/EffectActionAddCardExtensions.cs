using Cauldron.Core.Entities.Effect;
using Cauldron.Shared.MessagePackObjects.Value;
using System.Linq;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectActionAddCardExtensions
    {
        public static async ValueTask<(bool, EffectEventArgs)> Execute(this EffectActionAddCard effectActionAddCard, Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            var choiceResult = await effectEventArgs.GameMaster.ChoiceCards(effectOwnerCard, effectActionAddCard.Choice, effectEventArgs);
            var newCardDefs = choiceResult.CardDefList;

            var (exists, owner) = effectEventArgs.GameMaster.playerRepository.TryGet(effectOwnerCard.OwnerId);
            if (!exists)
            {
                return (false, effectEventArgs);
            }

            var opponent = effectEventArgs.GameMaster.GetOpponent(effectOwnerCard.OwnerId);

            var zonePrettyNames = await effectActionAddCard.ZoneToAddCard.Calculate(effectOwnerCard, effectEventArgs);

            if (!zonePrettyNames.Any())
            {
                return (false, effectEventArgs);
            }

            var (success, zone) = zonePrettyNames[0].TryGetZone(owner.Id, opponent.Id);
            if (!success)
            {
                return (false, effectEventArgs);
            }

            var newCards = newCardDefs.Select(cd => effectEventArgs.GameMaster.GenerateNewCard(cd.Id, zone)).ToArray();

            return (true, effectEventArgs);
        }
    }
}
