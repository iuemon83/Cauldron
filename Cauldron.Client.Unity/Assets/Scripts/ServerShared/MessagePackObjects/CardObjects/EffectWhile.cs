using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    /// <summary>
    /// カード効果が有効となる期間
    /// </summary>
    [MessagePackObject(true)]
    public class EffectWhile
    {
        public EffectTiming Timing { get; }
        public int Skip { get; }
        public int Take { get; }

        public EffectWhile(EffectTiming Timing, int Skip, int Take)
        {
            this.Timing = Timing;
            this.Skip = Skip;
            this.Take = Take;
        }
    }
}
