using Cauldron.Core.Entities.Effect;
using Cauldron.Shared.MessagePackObjects.Value;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectActionDamageExtensions
    {
        public static async ValueTask<(bool, EffectEventArgs)> Execute(this EffectActionDamage effectActionDamage, Card effectOwnerCard, EffectEventArgs args)
        {
            var choiceResult = await args.GameMaster.ChoiceCards(effectOwnerCard, effectActionDamage.Choice, args);

            var done = false;

            var damageValue = await effectActionDamage.Value.Calculate(effectOwnerCard, args);

            foreach (var playerId in choiceResult.PlayerIdList)
            {
                var damageContext = new DamageContext(
                    effectOwnerCard,
                    Value: damageValue,
                    GuardPlayer: args.GameMaster.PlayersById[playerId]
                    );

                await args.GameMaster.HitPlayer(damageContext);

                done = true;
            }

            foreach (var card in choiceResult.CardList)
            {
                var damageContext = new DamageContext(
                    effectOwnerCard,
                    Value: damageValue,
                    GuardCard: card
                    );
                await args.GameMaster.HitCreature(damageContext);

                done = true;
            }

            return (done, args);
        }
    }
}
