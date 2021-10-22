using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    /// <summary>
    /// カード効果（プレイ時）を発動するための条件
    /// </summary>
    [MessagePackObject(true)]
    public class EffectConditionByPlaying : EffectCondition
    {
        public EffectConditionByPlaying(
            EffectWhen When = null,
            EffectWhile While = null,
            EffectIf If = null
            )
            : base(ZonePrettyName.None, When, While, If) { }
    }
}
