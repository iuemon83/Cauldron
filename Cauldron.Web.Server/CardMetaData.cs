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

        public static string[] GetStrings<T>() where T : struct, Enum =>
            Enum.GetValues<T>().Select(c => ToCamelCase(c.ToString())).ToArray();

        public string[] CardTypes { get; } = GetStrings<CardType>();
        public string[] CardAbilities { get; } = GetStrings<CreatureAbility>();
        public string[] EffectTimingDamageAfterEventSources { get; }
            = GetStrings<EffectTimingDamageAfterEvent.EventSource>();
        public string[] EffectTimingDamageBeforeEventSources { get; }
            = GetStrings<EffectTimingDamageBeforeEvent.EventSource>();
        public string[] EffectTimingDestroyEventSources { get; }
            = GetStrings<EffectTimingDestroyEvent.EventSource>();
        public string[] EffectTimingEndTurnEventSources { get; }
            = GetStrings<EffectTimingEndTurnEvent.EventSource>();
        public string[] EffectTimingModifyPlayerEventSources { get; }
            = GetStrings<EffectTimingModifyPlayerEvent.EventSource>();
        public string[] EffectTimingMoveCardEventSources { get; }
            = GetStrings<EffectTimingMoveCardEvent.EventSource>();
        public string[] EffectTimingPlayEventSources { get; }
            = GetStrings<EffectTimingPlayEvent.EventSource>();
        public string[] EffectTimingStartTurnEventSources { get; }
            = GetStrings<EffectTimingStartTurnEvent.EventSource>();
        public string[] ChoiceHowList { get; } = GetStrings<Choice.ChoiceHow>();
        public string[] NumConditionCompares { get; } = GetStrings<NumCondition.ConditionCompare>();
        public string[] NumValueCalculatorValueTypes { get; } = GetStrings<NumValueCalculator.ValueType>();
        public string[] NumValueModifierOperators { get; } = GetStrings<NumValueModifier.ValueModifierOperator>();
        public string[] TextConditionCompares { get; } = GetStrings<TextCondition.ConditionCompare>();
        public string[] TextValueCalculatorValueTypes { get; } = GetStrings<TextValueCalculator.ValueType>();
        public string[] ZoneNames { get; } = GetStrings<ZonePrettyName>();
        public string[] CardConditionContexts { get; } = GetStrings<CardCondition.CardConditionContext>();
        public string[] PlayerConditionContexts { get; } = GetStrings<PlayerCondition.PlayerConditionContext>();
        public string[] PlayerConditionTypes { get; } = GetStrings<PlayerCondition.PlayerConditionType>();

    }
}
