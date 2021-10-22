using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    /// <summary>
    /// カード効果を発動するための条件
    /// </summary>
    [MessagePackObject(true)]
    public class EffectConditionWrap
    {
        public EffectConditionByPlaying ByPlay { get; }

        public EffectCondition ByNotPlay { get; }

        public EffectConditionWrap(
            EffectConditionByPlaying ByPlay = default,
            EffectCondition ByNotPlay = default
            )
        {
            this.ByPlay = ByPlay;
            this.ByNotPlay = ByNotPlay;
        }
    }
}
