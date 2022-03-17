using Cauldron.Core.Entities.Effect;
using Cauldron.Shared.MessagePackObjects;

namespace Cauldron.Core.MessagePackObjectExtensions
{
    public class EffectActionReserveEffectExecuter : IEffectActionExecuter
    {
        private readonly EffectActionReserveEffect _this;

        public EffectActionReserveEffectExecuter(EffectActionReserveEffect _this)
        {
            this._this = _this;
        }

        public ValueTask<(bool, EffectEventArgs)> Execute(Card effectOwnerCard, EffectEventArgs args)
        {
            args.GameMaster.ReserveEffect(effectOwnerCard, _this.EffectsToReserve);

            return new ValueTask<(bool, EffectEventArgs)>((true, args));
        }
    }
}
