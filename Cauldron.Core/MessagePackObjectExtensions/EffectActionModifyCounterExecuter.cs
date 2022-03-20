using Cauldron.Core.Entities.Effect;
using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.MessagePackObjects.Value;

namespace Cauldron.Core.MessagePackObjectExtensions
{
    public class EffectActionModifyCounterExecuter : IEffectActionExecuter
    {
        private readonly EffectActionModifyCounter _this;

        public EffectActionModifyCounterExecuter(EffectActionModifyCounter _this)
        {
            this._this = _this;
        }

        public async ValueTask<(bool, EffectEventArgs)> Execute(Card effectOwnerCard, EffectEventArgs args)
        {
            if (_this.NumCountersModifier == null)
            {
                return (true, args);
            }

            var choiceResult = await args.GameMaster.Choice(effectOwnerCard, _this.TargetsChoice, args);

            var totalNumBeforeCounters = 0;
            var totalNumAfterCounters = 0;

            var targetPlayers = args.GameMaster.playerRepository.TryList(choiceResult.PlayerIdList).ToArray();

            foreach (var player in targetPlayers)
            {
                var beforeNumCounters = 0;
                totalNumBeforeCounters += beforeNumCounters;

                var newArgs = args with
                {
                    ActionTargetPlayers = targetPlayers,
                    ActionTargetPlayer = player
                };

                var modifyNum = await _this.NumCountersModifier.Modify(effectOwnerCard, args, beforeNumCounters)
                    - beforeNumCounters;
                await args.GameMaster.ModifyCounter(player.Id, _this.CounterName, modifyNum, effectOwnerCard);

                totalNumAfterCounters += modifyNum;
            }

            foreach (var card in choiceResult.CardList)
            {
                var beforeNumCounters = card.GetCounter(_this.CounterName);
                totalNumBeforeCounters += beforeNumCounters;

                var newArgs = args with
                {
                    ActionTargetCards = choiceResult.CardList,
                    ActionTargetCard = card
                };

                var modifyNum = await _this.NumCountersModifier.Modify(effectOwnerCard, newArgs, beforeNumCounters)
                    - beforeNumCounters;
                await args.GameMaster.ModifyCounter(card, _this.CounterName, modifyNum, effectOwnerCard);

                totalNumAfterCounters += modifyNum;
            }

            if (!string.IsNullOrEmpty(_this.Name))
            {
                var context = new ActionContext(ModifyCounter: new(
                    choiceResult.CardList,
                    Math.Max(0, totalNumBeforeCounters),
                    Math.Max(0, totalNumAfterCounters)
                    ));
                args.GameMaster.SetActionContext(effectOwnerCard.Id, _this.Name, context);
            }

            return (true, args);
        }
    }
}
