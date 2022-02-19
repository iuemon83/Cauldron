#nullable enable

using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    /// <summary>
    /// カード効果（プレイ無し）を発動するための条件
    /// </summary>
    [MessagePackObject(true)]
    public class EffectConditionAsNotPlay : EffectCondition
    {
        public EffectConditionAsNotPlay(
            ZonePrettyName Zone,
            EffectWhen? When = null,
            EffectWhile? While = null,
            EffectIf? If = null
            )
            : base(Zone, When, While, If) { }
    }
}
