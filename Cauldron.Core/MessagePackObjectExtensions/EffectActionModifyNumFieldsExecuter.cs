using Cauldron.Core.Entities.Effect;
using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.MessagePackObjects.Value;

namespace Cauldron.Core.MessagePackObjectExtensions
{
    public class EffectActionModifyNumFieldsExecuter : IEffectActionExecuter
    {
        private readonly EffectActionModifyNumFields _this;

        public EffectActionModifyNumFieldsExecuter(EffectActionModifyNumFields _this)
        {
            this._this = _this;
        }

        public async ValueTask<(bool, EffectEventArgs)> Execute(Card effectOwnerCard, CardEffectId effectId, EffectEventArgs args)
        {
            var choiceResult = await args.GameMaster.Choice(effectOwnerCard, _this.ChoicePlayers, args);
            var targetPlayerIdList = choiceResult.PlayerIdList;

            var done = false;
            foreach (var id in targetPlayerIdList)
            {
                var (exists, player) = args.GameMaster.TryGet(id);
                if (!exists)
                {
                    continue;
                }

                var newNumFields = await _this.DiffNum.Modify(effectOwnerCard, args, player.Field.CurrentLimit);

                var diff = args.GameMaster.ModifyNumFields(id, newNumFields);
                if (diff != 0)
                {
                    done = true;
                }
            }

            return (done, args);
        }
    }
}
