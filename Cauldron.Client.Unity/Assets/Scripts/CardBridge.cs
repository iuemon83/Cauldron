#nullable enable

using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using System;
using System.Collections.Generic;

namespace Assets.Scripts
{
    public class CardBridge
    {
        private readonly CardDef cardDef;
        private readonly Card card;

        public CardBridge(CardDef cardDef, Card card)
        {
            this.cardDef = cardDef;
            this.card = card;
        }

        public CardDefId CardDefId => this.cardDef.Id;

        public int Cost => this.cardDef?.Cost ?? this.card?.Cost ?? default;
        public int BaseCost => this.cardDef?.Cost ?? this.card?.BaseCost ?? default;
        public string CardSetName => this.cardDef?.CardSetName ?? this.card?.CardSetName ?? "";
        public string Name => this.cardDef?.Name ?? this.card?.Name ?? "";
        public string FullName => this.cardDef?.FullName ?? this.card?.FullName ?? "";
        public string FlavorText => this.cardDef?.FlavorText ?? this.card?.FlavorText ?? "";
        public IReadOnlyList<string> Annotations => this.cardDef?.Annotations ?? this.card?.Annotations
            ?? (IReadOnlyList<string>)Array.Empty<string>();
        public bool IsToken => this.cardDef?.IsToken ?? this.card?.IsToken ?? default;
        public CardType Type => this.cardDef?.Type ?? this.card?.Type ?? default;
        public int Power => this.cardDef?.Power ?? this.card?.Power ?? default;
        public int BasePower => this.cardDef?.Power ?? this.card?.BasePower ?? default;
        public int Toughness => this.cardDef?.Toughness ?? this.card?.Toughness ?? default;
        public int BaseToughness => this.cardDef?.Toughness ?? this.card?.BaseToughness ?? default;
        public IReadOnlyList<CreatureAbility> Abilities => this.cardDef?.Abilities ?? this.card?.Abilities
            ?? (IReadOnlyList<CreatureAbility>)Array.Empty<CreatureAbility>();
        public int NumTurnsToCanAttackToCreature => this.cardDef?.NumTurnsToCanAttackToCreature
            ?? this.card?.NumTurnsToCanAttackToCreature
            ?? default;
        public int NumTurnsToCanAttackToPlayer => this.cardDef?.NumTurnsToCanAttackToPlayer
            ?? this.card?.NumTurnsToCanAttackToPlayer
            ?? default;
        public int NumAttacksLimitInTurn => this.cardDef?.NumAttacksLimitInTurn
            ?? this.card?.NumAttacksLimitInTurn
            ?? default;
        public int LimitNumCardsInDeck => this.cardDef?.LimitNumCardsInDeck ?? default;
        public string EffectDescription => this.cardDef?.EffectDescription ?? this.card?.EffectDescription ?? "";
        public int NumTurnsInField => this.card?.NumTurnsInField ?? default;
        public Zone? Zone => this.card?.Zone ?? default;
        public IDictionary<string, int> CountersByName => this.card?.CountersByName ?? new Dictionary<string, int>();
    }
}
