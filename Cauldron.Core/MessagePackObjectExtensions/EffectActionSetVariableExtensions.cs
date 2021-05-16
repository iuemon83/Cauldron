using Cauldron.Core.Entities.Effect;
using Cauldron.Shared.MessagePackObjects.Value;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects
{
    public static class EffectActionSetVariableExtensions
    {
        public static async ValueTask<(bool, EffectEventArgs)> Execute(this EffectActionSetVariable effectActionSetVariable, Card effectOwnerCard, EffectEventArgs args)
        {
            if (effectActionSetVariable.NumValue != null)
            {
                var value = await effectActionSetVariable.NumValue.Calculate(effectOwnerCard, args);
                args.GameMaster.SetVariable(effectOwnerCard.Id, effectActionSetVariable.Name, value);
            }

            return (true, args);
        }
    }
}
