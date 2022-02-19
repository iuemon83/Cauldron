#nullable enable

using Assets.Scripts.ServerShared.MessagePackObjects;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class EffectTimingDamageBeforeEvent
    {
        public enum TypeValue
        {
            /// <summary>
            /// いずれか
            /// </summary>
            [DisplayText("いずれか")]
            Any,

            /// <summary>
            /// 戦闘ダメージ
            /// </summary>
            [DisplayText("戦闘ダメージ")]
            Battle,

            /// <summary>
            /// 戦闘以外のダメージ
            /// </summary>
            [DisplayText("戦闘以外のダメージ")]
            NonBattle,
        }

        public enum SourceValue
        {
            /// <summary>
            /// いずれか
            /// </summary>
            [DisplayText("いずれか")]
            Any,

            /// <summary>
            /// ダメージを与える側
            /// </summary>
            [DisplayText("ダメージを与える側")]
            DamageSource,

            /// <summary>
            /// ダメージを受ける側
            /// </summary>
            [DisplayText("ダメージを受ける側")]
            Take,
        }

        public EffectTimingDamageBeforeEvent.TypeValue Type { get; }
        public EffectTimingDamageBeforeEvent.SourceValue Source { get; }
        public PlayerCondition? PlayerCondition { get; }
        public CardCondition? CardCondition { get; }

        public EffectTimingDamageBeforeEvent(
            EffectTimingDamageBeforeEvent.TypeValue Type = TypeValue.Any,
            EffectTimingDamageBeforeEvent.SourceValue Source = SourceValue.Any,
            PlayerCondition? PlayerCondition = null,
            CardCondition? CardCondition = null
            )
        {
            this.Type = Type;
            this.Source = Source;
            this.PlayerCondition = PlayerCondition;
            this.CardCondition = CardCondition;
        }
    }
}
