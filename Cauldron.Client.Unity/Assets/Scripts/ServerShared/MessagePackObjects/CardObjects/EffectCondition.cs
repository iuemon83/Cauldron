using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    /// <summary>
    /// カード効果を発動するための条件
    /// </summary>
    [MessagePackObject(true)]
    public class EffectCondition
    {
        public ZonePrettyName ZonePrettyName { get; }

        public EffectWhen When { get; }

        public EffectWhile While { get; }

        public EffectIf If { get; }

        public EffectCondition(
            ZonePrettyName ZonePrettyName,
            EffectWhen When,
            EffectWhile While = null,
            EffectIf If = null
            )
        {
            this.ZonePrettyName = ZonePrettyName;
            this.When = When;
            this.While = While;
            this.If = If;
        }
    }
}
