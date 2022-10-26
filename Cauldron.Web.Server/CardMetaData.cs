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
        public MetaDataElm[] EffectTimingDamageBeforeDamageTypes { get; }
            = Convert<EffectTimingDamageBeforeEvent.TypeValue>();
        public MetaDataElm[] EffectTimingModifyCounterOnCardEventOperators { get; }
            = Convert<EffectTimingModifyCounterOnCardEvent.OperatorValue>();
        public MetaDataElm[] ChoiceHowList { get; } = Convert<Choice.HowValue>();
        public MetaDataElm[] ChoiceSourceHowList { get; } = Convert<ChoiceSource.HowValue>();
        public MetaDataElm[] NumConditionCompares { get; } = Convert<NumCompare.CompareValue>();
        public MetaDataElm[] NumValueCalculatorEventContexts { get; } = Convert<NumValueCalculator.EventContextValue>();
        public MetaDataElm[] NumValueCalculatorForCardTypes { get; } = Convert<NumValueCalculatorForCard.TypeValue>();
        public MetaDataElm[] NumValueCalculatorForPlayerTypes { get; } = Convert<NumValueCalculatorForPlayer.TypeValue>();
        public MetaDataElm[] NumValueModifierOperators { get; } = Convert<NumValueModifier.OperatorValue>();
        public MetaDataElm[] TextConditionCompares { get; } = Convert<TextCompare.CompareValue>();
        public MetaDataElm[] TextValueCalculatorValueTypes { get; } = Convert<TextValueCalculator.TypeValue>();
        public MetaDataElm[] ZoneNames { get; } = Convert<ZonePrettyName>();
        public MetaDataElm[] OutZoneNames { get; } = Convert<OutZonePrettyName>();
        public MetaDataElm[] CardConditionContexts { get; }
            = Convert<CardCondition.ContextConditionValue>();
        public MetaDataElm[] CardConditionDamageEventContextConditions { get; }
            = Convert<CardCondition.DamageEventContextConditionValue>();
        public MetaDataElm[] CardConditionBattleEventContextConditions { get; }
            = Convert<CardCondition.BattleEventContextConditionValue>();
        public MetaDataElm[] OwnerConditionValues { get; } = Convert<CardCondition.OwnerConditionValue>();
        public MetaDataElm[] PositionTypeValues { get; } = Convert<InsertCardPosition.PositionTypeValue>();
        public MetaDataElm[] PlayerConditionContexts { get; } = Convert<PlayerCondition.ContextValue>();
        public MetaDataElm[] CardSetConditionTypes { get; } = Convert<CardSetCondition.TypeValue>();
        public MetaDataElm[] ActionContextCountersOfModifyCounterTypes { get; }
            = Convert<ActionContextCountersOfModifyCounter.TypeValue>();
        public MetaDataElm[] ActionContextCardsOfAddCardTypes { get; }
            = Convert<ActionContextCardsOfAddCard.TypeValue>();
        public MetaDataElm[] ActionContextCardsOfAddEffectTypes { get; }
            = Convert<ActionContextCardsOfAddEffect.TypeValue>();
        public MetaDataElm[] ActionContextCardsOfDamageTypes { get; }
            = Convert<ActionContextCardsOfDamage.TypeValue>();
        public MetaDataElm[] ActionContextCardsOfDestroyCardTypes { get; }
            = Convert<ActionContextCardsOfDestroyCard.TypeValue>();
        public MetaDataElm[] ActionContextCardsOfDrawCardTypes { get; }
            = Convert<ActionContextCardsOfDrawCard.TypeValue>();
        public MetaDataElm[] ActionContextCardsOfExcludeCardTypes { get; }
            = Convert<ActionContextCardsOfExcludeCard.TypeValue>();
        public MetaDataElm[] ActionContextCardsOfModifyCardTypes { get; }
            = Convert<ActionContextCardsOfModifyCard.TypeValue>();
        public MetaDataElm[] ActionContextCardsOfModifyCounterTypes { get; }
            = Convert<ActionContextCardsOfModifyCounter.TypeValue>();
        public MetaDataElm[] ActionContextCardsOfMoveCardTypes { get; }
            = Convert<ActionContextCardsOfMoveCard.TypeValue>();
        public MetaDataElm[] CreatureAbilityModifierOperators { get; }
            = Convert<CreatureAbilityModifier.OperatorValue>();
        public MetaDataElm[] AnnotationsModifierOperators { get; }
            = Convert<AnnotationsModifier.OperatorValue>();
        public MetaDataElm[] ActionContextCardsOfChoiceTypes { get; }
            = Convert<ActionContextCardsOfChoice.TypeValue>();
        public MetaDataElm[] ActionContextPlayersOfChoiceTypes { get; }
            = Convert<ActionContextPlayersOfChoice.TypeValue>();
    }
}
