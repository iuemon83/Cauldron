using Cauldron.Core.Entities.Effect;
using System.Threading.Tasks;

namespace Cauldron.Shared.MessagePackObjects.Value
{
    public static class TextValueExtensions
    {
        public static async ValueTask<string> Calculate(this TextValue textValue, Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            return textValue.PureValue
                ?? await (textValue.TextValueCalculator?.Calculate(effectOwnerCard, effectEventArgs)
                    ?? ValueTask.FromResult(""));
        }
    }
}
