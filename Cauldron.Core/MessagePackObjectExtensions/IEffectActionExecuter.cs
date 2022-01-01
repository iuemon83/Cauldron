using Cauldron.Core.Entities.Effect;
using Cauldron.Shared.MessagePackObjects;
using System.Threading.Tasks;

namespace Cauldron.Core.MessagePackObjectExtensions
{
    internal interface IEffectActionExecuter
    {
        ValueTask<(bool, EffectEventArgs)> Execute(Card effectOwnerCard, EffectEventArgs effectEventArgs);
    }
}
