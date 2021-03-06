﻿using Cauldron.Core.Entities.Effect;
using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Cauldron.Core.Entities
{
    public class Player
    {
        private RuleBook RuleBook { get; }

        public PlayerId Id { get; }

        public string Name { get; } = "";

        public Dictionary<CardId, Card> CardsById { get; } = new();

        public Hands Hands { get; }

        public Deck Deck { get; }

        public Field Field { get; }

        public Cemetery Cemetery { get; }

        public int MaxHp { get; private set; }
        public int UsedHp { get; private set; }
        public int CurrentHp => Math.Max(0, this.MaxHp - this.UsedHp);
        public int MaxLimitMp { get; private set; }
        public int MaxMp { get; private set; }
        public int UsedMp { get; private set; }
        public int CurrentMp => Math.Max(0, this.MaxMp - this.UsedMp);

        public Player(PlayerId id, string name, RuleBook ruleBook, IReadOnlyList<Card> deck)
        {
            foreach (var card in deck)
            {
                card.OwnerId = id;
            }

            this.RuleBook = ruleBook;
            this.Id = id;
            this.Name = name;
            this.MaxHp = this.RuleBook.MaxPlayerHp;
            this.UsedHp = this.MaxHp - this.RuleBook.InitialPlayerHp;
            this.MaxLimitMp = this.RuleBook.MaxLimitMp;
            this.MaxMp = this.RuleBook.InitialMp;
            this.Deck = new Deck(deck);
            this.Hands = new Hands();
            this.Field = new Field(this.RuleBook);
            this.Cemetery = new Cemetery();

            foreach (var card in deck)
            {
                this.CardsById.Add(card.Id, card);
            }
        }

        public Card Draw()
        {
            var newCard = this.Deck.Draw();
            if (newCard == null)
            {
                this.Damage(1);
            }
            else
            {
                Console.WriteLine("draw");
                newCard.Zone = new(newCard.OwnerId, ZoneName.Hand);
                this.Hands.Add(newCard);
            }

            return newCard;
        }

        public void AddMaxMp(int x)
        {
            this.MaxMp = Math.Min(this.MaxMp + x, this.MaxLimitMp);
        }

        public void UseMp(int x) => this.UsedMp = Math.Min(this.MaxMp, this.UsedMp + x);

        public void GainMp(int x) => this.UsedMp = Math.Max(0, this.UsedMp - x);

        /// <summary>
        /// MP を最大値まで回復
        /// </summary>
        public void FullMp() => this.UsedMp = 0;

        public void Damage(Card card) => this.Damage(card.Power);

        public void Damage(int x) => this.UsedHp = Math.Min(this.MaxHp, this.UsedHp + x);

        public void GainHp(int x) => this.UsedHp = Math.Max(0, this.UsedHp - x);

        public async ValueTask Modify(PlayerModifier modifier, Card effectOwnerCard, EffectEventArgs effectEventArgs)
        {
            if (modifier.MaxHp != null)
            {
                this.MaxHp = await modifier.MaxHp.Modify(effectOwnerCard, effectEventArgs, this.MaxHp);
            }

            if (modifier.Hp != null)
            {
                this.GainHp(await modifier.Hp.Modify(effectOwnerCard, effectEventArgs, this.CurrentHp) - this.CurrentHp);
            }

            if (modifier.MaxMp != null)
            {
                this.MaxMp = await modifier.MaxMp.Modify(effectOwnerCard, effectEventArgs, this.MaxMp);
            }

            if (modifier.Mp != null)
            {
                this.GainMp(await modifier.Mp.Modify(effectOwnerCard, effectEventArgs, this.CurrentMp) - this.CurrentMp);
            }
        }

        public PrivatePlayerInfo PrivatePlayerInfo => new PrivatePlayerInfo(this.PublicPlayerInfo, this.Hands.AllCards.ToArray());
        public PublicPlayerInfo PublicPlayerInfo => new PublicPlayerInfo(
            this.Id,
            this.Name,
            this.Field.AllCards.ToArray(),
            this.Deck.Count,
            this.Cemetery.AllCards.ToArray(),
            this.Hands.Count,
            this.MaxHp,
            this.CurrentHp,
            this.MaxMp,
            this.CurrentMp
            );
    }
}
