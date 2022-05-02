using Cauldron.Core.Entities.Effect;
using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;

namespace Cauldron.Core.MessagePackObjectExtensions
{
    internal interface IEffectActionExecuter
    {
        ValueTask<(bool, EffectEventArgs)> Execute(Card effectOwnerCard, CardEffectId effectId, EffectEventArgs effectEventArgs);
    }
}
