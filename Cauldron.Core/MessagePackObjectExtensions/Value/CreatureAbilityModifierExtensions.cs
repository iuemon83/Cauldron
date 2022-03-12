namespace Cauldron.Shared.MessagePackObjects.Value
{
    public static class CreatureAbilityModifierExtensions
    {
        public static CreatureAbility[] Modify(this CreatureAbilityModifier _this, IReadOnlyList<CreatureAbility> value)
        {
            return _this.Operator switch
            {
                CreatureAbilityModifier.OperatorValue.Add => value.Concat(new[] { _this.Value }).ToArray(),
                CreatureAbilityModifier.OperatorValue.Remove => value.Where(a => a != _this.Value).ToArray(),
                CreatureAbilityModifier.OperatorValue.Clear => Array.Empty<CreatureAbility>(),
                _ => throw new InvalidOperationException()
            };
        }
    }
}
