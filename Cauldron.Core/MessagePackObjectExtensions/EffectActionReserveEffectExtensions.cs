using Cauldron.Core.Entities.Effect;
using Cauldron.Shared.MessagePackObjects;
using System.Threading.Tasks;

namespace Cauldron.Core.MessagePackObjectExtensions
{
    public class EffectActionReserveEffectExecuter : IEffectActionExecuter
    {
        private readonly EffectActionReserveEffect _this;

        public EffectActionReserveEffectExecuter(EffectActionReserveEffect _this)
        {
            this._this = _this;
        }

        public async ValueTask<(bool, EffectEventArgs)> Execute(Card effectOwnerCard, EffectEventArgs args)
        {
            args.GameMaster.ReserveEffect(effectOwnerCard, _this.EffectsToReserve);

            return (true, args);
        }
    }
}
