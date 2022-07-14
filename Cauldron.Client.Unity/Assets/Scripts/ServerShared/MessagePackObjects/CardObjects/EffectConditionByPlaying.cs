#nullable enable

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
            EffectIf? If = null
            )
            : base(ZonePrettyName.Any, default, default, If) { }
    }
}
