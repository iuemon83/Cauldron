using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    /// <summary>
    /// カード効果（予約）を発動するための条件
    /// </summary>
    [MessagePackObject(true)]
    public class EffectConditionAsReserve : EffectCondition
    {
        public EffectConditionAsReserve(
            EffectWhen When,
            EffectWhile While = null,
            EffectIf If = null
            )
            : base(ZonePrettyName.Any, When, While, If) { }
    }
}
