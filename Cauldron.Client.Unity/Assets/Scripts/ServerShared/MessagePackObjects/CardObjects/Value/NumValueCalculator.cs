#nullable enable

using MessagePack;

namespace Cauldron.Shared.MessagePackObjects.Value
{
    [MessagePackObject(true)]
    public class NumValueCalculator
    {
        /// <summary>
        /// イベントに関係する値
        /// </summary>
        public enum EventContextValue
        {
            None,

            /// <summary>
            /// ダメージの値。ダメージ前イベントで有効。
            /// </summary>
            DamageValue
        }

        public EventContextValue EventContext { get; }
        public NumValueRandom? Random { get; }
        public NumValueCalculatorForPlayer? ForPlayer { get; }
        public NumValueCalculatorForCard? ForCard { get; }
        public NumValueCalculatorForCounter? ForCounter { get; }

        public NumValueCalculator(
            EventContextValue EventContext = EventContextValue.None,
            NumValueRandom? Random = null,
            NumValueCalculatorForPlayer? ForPlayer = null,
            NumValueCalculatorForCard? ForCard = null,
            NumValueCalculatorForCounter? ForCounter = null
            )
        {
            this.EventContext = EventContext;
            this.Random = Random;
            this.ForPlayer = ForPlayer;
            this.ForCard = ForCard;
            this.ForCounter = ForCounter;
        }
    }
}
