using Assets.Scripts.ServerShared.MessagePackObjects;
using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.MessagePackObjects.Value;
using System;
using System.Linq;

namespace Cauldron.Web.Server
{
    public class CardMetaData
    {
        public static string ToCamelCase(string value)
            => Char.ToLower(value[0]) + (value.Length > 1 ? value[1..] : "");

        private static string GetDisplayText(Enum value)
        {
            var fieldInfo = value.GetType().GetField(value.ToString());
            Attribute[] authors = Attribute.GetCustomAttributes(fieldInfo, typeof(DisplayTextAttribute));
            foreach (Attribute att in authors)
            {
                if (att is DisplayTextAttribute displayText)
                {
                    return displayText.Value;
                }
            }

            return "";
        }

        public static MetaDataElm[] Convert<T>() where T : struct, Enum =>
            Enum.GetValues<T>()
                .Select(v => new MetaDataElm(ToCamelCase(v.ToString()), GetDisplayText(v)))
                .ToArray();

        public record MetaDataElm(string Code, string DisplayText);

        public MetaDataElm[] CardTypes { get; } = Convert<CardType>();
        public MetaDataElm[] CardAbilities { get; } = Convert<CreatureAbility>();
        public MetaDataElm[] EffectTimingDamageAfterDamageTypes { get; }
            = Convert<EffectTimingDamageAfterEvent.TypeValue>();
        public MetaDataElm[] EffectTimingDamageAfterEventSources { get; }
            = Convert<EffectTimingDamageAfterEvent.SourceValue>();
        public MetaDataElm[] EffectTimingDamageBeforeDamageTypes { get; }
            = Convert<EffectTimingDamageBeforeEvent.TypeValue>();
        public MetaDataElm[] EffectTimingDamageBeforeEventSources { get; }
            = Convert<EffectTimingDamageBeforeEvent.SourceValue>();
        public MetaDataElm[] EffectTimingDestroyEventSources { get; }
            = Convert<EffectTimingDestroyEvent.SourceValue>();
        public MetaDataElm[] EffectTimingEndTurnEventSources { get; }
            = Convert<EffectTimingEndTurnEvent.SourceValue>();
        public MetaDataElm[] EffectTimingModifyPlayerEventSources { get; }
            = Convert<EffectTimingModifyPlayerEvent.SourceValue>();
        public MetaDataElm[] EffectTimingMoveCardEventSources { get; }
            = Convert<EffectTimingMoveCardEvent.SourceValue>();
        public MetaDataElm[] EffectTimingPlayEventSources { get; }
            = Convert<EffectTimingPlayEvent.SourceValue>();
        public MetaDataElm[] EffectTimingStartTurnEventSources { get; }
            = Convert<EffectTimingStartTurnEvent.SourceValue>();
        public MetaDataElm[] ChoiceHowList { get; } = Convert<Choice.HowValue>();
        public MetaDataElm[] ChoiceSourceHowList { get; } = Convert<ChoiceSource.HowValue>();
        public MetaDataElm[] NumConditionCompares { get; } = Convert<NumCondition.CompareValue>();
        public MetaDataElm[] NumValueCalculatorForCardValueTypes { get; } = Convert<NumValueCalculatorForCard.TypeValue>();
        public MetaDataElm[] NumValueModifierOperators { get; } = Convert<NumValueModifier.OperatorValue>();
        public MetaDataElm[] TextConditionCompares { get; } = Convert<TextCondition.CompareValue>();
        public MetaDataElm[] TextValueCalculatorValueTypes { get; } = Convert<TextValueCalculator.TypeValue>();
        public MetaDataElm[] ZoneNames { get; } = Convert<ZonePrettyName>();
        public MetaDataElm[] CardConditionContexts { get; } = Convert<CardCondition.ContextConditionValue>();
        public MetaDataElm[] OwnerConditionValues { get; } = Convert<CardCondition.OwnerConditionValue>();
        public MetaDataElm[] PositionTypeValues { get; } = Convert<InsertCardPosition.PositionTypeValue>();
        public MetaDataElm[] PlayerConditionContexts { get; } = Convert<PlayerCondition.ContextValue>();
        public MetaDataElm[] PlayerConditionTypes { get; } = Convert<PlayerCondition.TypeValue>();
        public MetaDataElm[] CardSetConditionTypes { get; } = Convert<CardSetCondition.TypeValue>();
    }
}
