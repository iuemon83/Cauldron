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

            var (success, defaultZone) = zonePrettyNames[0].TryGetZone(owner.Id, opponent.Id, owner.Id);
            if (!success)
            {
                return (false, effectEventArgs);
            }

            var choiceResult = await effectEventArgs.GameMaster
                .ChoiceCards(effectOwnerCard, effectActionAddCard.Choice, effectEventArgs);

            var cardDefAndZones = choiceResult.CardList
                .Select(c => (
                    effectEventArgs.GameMaster.TryGet(c.CardDefId),
                    zonePrettyNames[0].TryGetZone(owner.Id, opponent.Id, c.OwnerId)))
                .Where(x => x.Item1.Exists && x.Item2.Success)
                .Select(x => (x.Item1.CardDef, x.Item2.Zone))
                .Concat(choiceResult.CardDefList.Select(cd => (CardDef: cd, Zone: defaultZone)));

            foreach (var (cardDef, zone) in cardDefAndZones)
            {
                foreach (var cd in Enumerable.Repeat(cardDef, effectActionAddCard.NumOfAddCards))
                {
                    await effectEventArgs.GameMaster.GenerateNewCard(cd.Id, zone, effectActionAddCard.InsertCardPosition);
                }
            }

            return (true, effectEventArgs);
        }
    }
}
