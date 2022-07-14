#nullable enable

using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    /// <summary>
    /// カード効果を発動するための条件
    /// </summary>
    [MessagePackObject(true)]
    public class EffectCondition
    {
        public ZonePrettyName Zone { get; }

        public EffectWhen? When { get; }

        public EffectWhile? While { get; }

        public EffectIf? If { get; }

        public EffectCondition(
            ZonePrettyName Zone = ZonePrettyName.Any,
            EffectWhen? When = null,
            EffectWhile? While = null,
            EffectIf? If = null
            )
        {
            this.Zone = Zone;
            this.When = When;
            this.While = While;
            this.If = If;
        }
    }
}
