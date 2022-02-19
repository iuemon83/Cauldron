#nullable enable

using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    /// <summary>
    /// カード効果を発動するための条件
    /// </summary>
    [MessagePackObject(true)]
    public class EffectConditionWrap
    {
        public EffectConditionByPlaying? ByPlay { get; }

        public EffectConditionAsNotPlay? ByNotPlay { get; }

        public EffectConditionAsReserve? Reserve { get; }

        public EffectConditionWrap(
            EffectConditionByPlaying? ByPlay = default,
            EffectConditionAsNotPlay? ByNotPlay = default,
            EffectConditionAsReserve? Reserve = default
            )
        {
            this.ByPlay = ByPlay;
            this.ByNotPlay = ByNotPlay;
            this.Reserve = Reserve;
        }
    }
}
