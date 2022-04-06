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

        public EffectTimingDamageBeforeEvent.TypeValue Type { get; }
        public CardCondition? SourceCardCondition { get; }
        public PlayerCondition? TakePlayerCondition { get; }
        public CardCondition? TakeCardCondition { get; }

        public EffectTimingDamageBeforeEvent(
            EffectTimingDamageBeforeEvent.TypeValue Type = TypeValue.Any,
            CardCondition? SourceCardCondition = null,
            PlayerCondition? TakePlayerCondition = null,
            CardCondition? TakeCardCondition = null
            )
        {
            this.Type = Type;
            this.TakePlayerCondition = TakePlayerCondition;
            this.SourceCardCondition = SourceCardCondition;
            this.TakeCardCondition = TakeCardCondition;
        }
    }
}
