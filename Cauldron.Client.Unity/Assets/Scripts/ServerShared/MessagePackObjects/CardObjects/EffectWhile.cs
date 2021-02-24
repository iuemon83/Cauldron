using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    /// <summary>
    /// カード効果が有効となる期間
    /// </summary>
    [MessagePackObject(true)]
    public class EffectWhile
    {
        [Key(1)]
        public EffectTiming Timing { get; set; }
        [Key(2)]
        public int Skip { get; set; }
        [Key(3)]
        public int Take { get; set; }

        public EffectWhile(EffectTiming Timing, int Skip, int Take)
        {
            this.Timing = Timing;
            this.Skip = Skip;
            this.Take = Take;
        }
    }
}
