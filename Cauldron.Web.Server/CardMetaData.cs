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
            = Convert<EffectTimingDamageAfterEvent.DamageType>();
        public MetaDataElm[] EffectTimingDamageAfterEventSources { get; }
            = Convert<EffectTimingDamageAfterEvent.EventSource>();
        public MetaDataElm[] EffectTimingDamageBeforeDamageTypes { get; }
            = Convert<EffectTimingDamageBeforeEvent.DamageType>();
        public MetaDataElm[] EffectTimingDamageBeforeEventSources { get; }
            = Convert<EffectTimingDamageBeforeEvent.EventSource>();
        public MetaDataElm[] EffectTimingDestroyEventSources { get; }
            = Convert<EffectTimingDestroyEvent.EventSource>();
        public MetaDataElm[] EffectTimingEndTurnEventSources { get; }
            = Convert<EffectTimingEndTurnEvent.EventSource>();
        public MetaDataElm[] EffectTimingModifyPlayerEventSources { get; }
            = Convert<EffectTimingModifyPlayerEvent.EventSource>();
        public MetaDataElm[] EffectTimingMoveCardEventSources { get; }
            = Convert<EffectTimingMoveCardEvent.EventSource>();
        public MetaDataElm[] EffectTimingPlayEventSources { get; }
            = Convert<EffectTimingPlayEvent.SourceValue>();
        public MetaDataElm[] EffectTimingStartTurnEventSources { get; }
            = Convert<EffectTimingStartTurnEvent.EventSource>();
        public MetaDataElm[] ChoiceHowList { get; } = Convert<Choice.ChoiceHow>();
        public MetaDataElm[] NumConditionCompares { get; } = Convert<NumCondition.ConditionCompare>();
        public MetaDataElm[] NumValueCalculatorValueTypes { get; } = Convert<NumValueCalculator.ValueType>();
        public MetaDataElm[] NumValueModifierOperators { get; } = Convert<NumValueModifier.ValueModifierOperator>();
        public MetaDataElm[] TextConditionCompares { get; } = Convert<TextCondition.CompareValue>();
        public MetaDataElm[] TextValueCalculatorValueTypes { get; } = Convert<TextValueCalculator.ValueType>();
        public MetaDataElm[] ZoneNames { get; } = Convert<ZonePrettyName>();
        public MetaDataElm[] CardConditionContexts { get; } = Convert<CardCondition.ContextConditionValue>();
        public MetaDataElm[] PlayerConditionContexts { get; } = Convert<PlayerCondition.PlayerConditionContext>();
        public MetaDataElm[] PlayerConditionTypes { get; } = Convert<PlayerCondition.PlayerConditionType>();
        public MetaDataElm[] CardSetConditionTypes { get; } = Convert<CardSetCondition.ConditionType>();
    }
}
