using Cauldron.Core.Entities.Effect;
using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.MessagePackObjects.Value;

namespace Cauldron.Core.MessagePackObjectExtensions
{
    public class EffectActionSetVariableExecuter : IEffectActionExecuter
    {
        private readonly EffectActionSetVariable _this;

        public EffectActionSetVariableExecuter(EffectActionSetVariable _this)
        {
            this._this = _this;
        }

        public async ValueTask<(bool, EffectEventArgs)> Execute(Card effectOwnerCard, CardEffectId effectId, EffectEventArgs args)
        {
            if (_this.NumValue != null)
            {
                var value = await _this.NumValue.Calculate(effectOwnerCard, args);
                args.GameMaster.SetVariable(effectOwnerCard.Id, _this.Name, value);
            }

            return (true, args);
        }
    }
}
