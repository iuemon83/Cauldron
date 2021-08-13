using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    /// <summary>
    /// カード効果で、カードを除外する処理
    /// </summary>
    [MessagePackObject(true)]
    public class EffectActionExcludeCard
    {
        public Choice Choice { get; }

        public string Name { get; }

        public EffectActionExcludeCard(Choice Choice, string Name = null)
        {
            this.Choice = Choice;
            this.Name = Name;
        }
    }
}
