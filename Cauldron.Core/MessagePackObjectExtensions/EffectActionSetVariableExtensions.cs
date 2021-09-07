using Cauldron.Core.Entities.Effect;
using Cauldron.Shared.MessagePackObjects.Value;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectActionSetVariableExtensions
    {
        public static async ValueTask<(bool, EffectEventArgs)> Execute(this EffectActionSetVariable _this,
            Card effectOwnerCard, EffectEventArgs args)
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
