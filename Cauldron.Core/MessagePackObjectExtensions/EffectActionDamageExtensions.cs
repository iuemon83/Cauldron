using Cauldron.Core.Entities.Effect;
using Cauldron.Shared.MessagePackObjects.Value;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectActionDamageExtensions
    {
        public static async ValueTask<(bool, EffectEventArgs)> Execute(this EffectActionDamage _this,
            Card effectOwnerCard, EffectEventArgs args)
        {
            var choiceResult = await args.GameMaster.Choice(effectOwnerCard, _this.Choice, args);

            var done = false;

            var damageValue = await _this.Value.Calculate(effectOwnerCard, args);

            foreach (var playerId in choiceResult.PlayerIdList)
            {
                var (exists, guardPlayer) = args.GameMaster.playerRepository.TryGet(playerId);
                if (!exists)
                {
                    continue;
                }

                var damageContext = new DamageContext(
                    effectOwnerCard,
                    Value: damageValue,
                    GuardPlayer: guardPlayer
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

            if (!string.IsNullOrEmpty(_this.Name))
            {
                var context = new ActionContext(Damage: new(choiceResult.CardList));
                args.GameMaster.SetActionContext(effectOwnerCard.Id, _this.Name, context);
            }

            return (done, args);
        }
    }
}
