using Cauldron.Core.Entities.Effect;
using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.MessagePackObjects.Value;

namespace Cauldron.Core.MessagePackObjectExtensions
{
    public class EffectActionHealExecuter : IEffectActionExecuter
    {
        private readonly EffectActionHeal _this;

        public EffectActionHealExecuter(EffectActionHeal _this)
        {
            this._this = _this;
        }

        public async ValueTask<(bool, EffectEventArgs)> Execute(Card effectOwnerCard, CardEffectId effectId,
            EffectEventArgs args)
        {
            var choiceResult = await args.GameMaster.Choice(effectOwnerCard, _this.Choice, args);

            var targetPlayers = args.GameMaster.playerRepository.TryList(choiceResult.PlayerIdList).ToArray();

            var done = false;
            foreach (var guardPlayer in targetPlayers)
            {
                var newArgs = args with
                {
                    ActionTargetPlayers = targetPlayers,
                    ActionTargetPlayer = guardPlayer
                };

                var healValue = await _this.Value.Calculate(effectOwnerCard, newArgs);

                var healContext = new HealContext(
                    effectOwnerCard,
                    Value: healValue,
                    TakePlayer: guardPlayer
                    );

                await args.GameMaster.HealPlayer(healContext, effectOwnerCard, effectId);

                done = true;
            }

            foreach (var card in choiceResult.CardList)
            {
                var newArgs = args with
                {
                    ActionTargetCards = choiceResult.CardList,
                    ActionTargetCard = card
                };
                var healValue = await _this.Value.Calculate(effectOwnerCard, newArgs);

                var healContext = new HealContext(
                    effectOwnerCard,
                    Value: healValue,
                    TakeCard: card
                    );
                await args.GameMaster.HealCreature(healContext, effectOwnerCard, effectId);

                done = true;
            }

            if (!string.IsNullOrEmpty(_this.Name))
            {
                //var context = new ActionContext(Damage: new(choiceResult.CardList));
                //args.GameMaster.SetActionContext(effectOwnerCard.Id, _this.Name, context);
            }

            return (done, args);
        }
    }
}
