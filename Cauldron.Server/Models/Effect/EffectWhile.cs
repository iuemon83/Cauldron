namespace Cauldron.Server.Models.Effect
{
    /// <summary>
    /// カード効果が有効となる期間
    /// </summary>
    public record EffectWhile(
        EffectTiming Timing,
        int Skip,
        int Take
        )
    {
        private int count = 0;

        public bool IsMatch(Card effectOwnerCard, EffectEventArgs eventArgs)
        {
            if (this.Timing.IsMatch(effectOwnerCard, eventArgs))
            {
                this.count++;
            }

            return (this.Skip == 0 || this.count > this.Skip)
                && this.count <= this.Take + this.Skip;
        }
    }
}
