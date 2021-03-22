using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectTimingDamageBeforeEvent
    {
        public enum DamageType
        {
            /// <summary>
            /// いずれか
            /// </summary>
            Any,

            /// <summary>
            /// 戦闘ダメージ
            /// </summary>
            Battle,

            /// <summary>
            /// 戦闘以外のダメージ
            /// </summary>
            NonBattle,
        }

        public enum EventSource
        {
            /// <summary>
            /// いずれか
            /// </summary>
            Any,

            /// <summary>
            /// ダメージを与える
            /// </summary>
            DamageSource,

            /// <summary>
            /// ダメージを受ける
            /// </summary>
            Take,
        }

        public EffectTimingDamageBeforeEvent.DamageType Type { get; }
        public EffectTimingDamageBeforeEvent.EventSource Source { get; }
        public PlayerCondition PlayerCondition { get; }
        public CardCondition CardCondition { get; }

        public EffectTimingDamageBeforeEvent(
            EffectTimingDamageBeforeEvent.DamageType Type = DamageType.Any,
            EffectTimingDamageBeforeEvent.EventSource Source = EventSource.Any,
            PlayerCondition PlayerCondition = null,
            CardCondition CardCondition = null
            )
        {
            this.Type = Type;
            this.Source = Source;
            this.PlayerCondition = PlayerCondition;
            this.CardCondition = CardCondition;
        }
    }
}
