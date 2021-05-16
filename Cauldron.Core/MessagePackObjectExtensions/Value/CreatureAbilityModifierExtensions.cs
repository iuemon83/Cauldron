using System;
using System.Collections.Generic;
using System.Linq;

namespace Cauldron.Shared.MessagePackObjects.Value
{
    public static class CreatureAbilityModifierExtensions
    {
        public static CreatureAbility[] Modify(this CreatureAbilityModifier abilityMOdifier, IReadOnlyList<CreatureAbility> value)
        {
            return abilityMOdifier.Operator switch
            {
                CreatureAbilityModifier.OperatorValue.Add => value.Concat(new[] { abilityMOdifier.Value }).ToArray(),
                CreatureAbilityModifier.OperatorValue.Remove => value.Where(a => a != abilityMOdifier.Value).ToArray(),
                CreatureAbilityModifier.OperatorValue.Clear => Array.Empty<CreatureAbility>(),
                _ => throw new InvalidOperationException()
            };
        }
    }
}
