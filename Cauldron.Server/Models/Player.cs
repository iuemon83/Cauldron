using Cauldron.Server.Models.Effect;
using System;
using System.Collections.Generic;

namespace Cauldron.Server.Models
{
    public class Player
    {
        private RuleBook RuleBook { get; }

        public Guid Id { get; }

        public string Name { get; } = "";

        public Dictionary<Guid, Card> CardsById { get; } = new Dictionary<Guid, Card>();

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

        public Player(Guid id, string name, RuleBook ruleBook, IReadOnlyList<Card> deck)
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
                this.Hands.Add(newCard);
            }

            return newCard;
        }

        public void AddMaxMp(int x)
        {
            this.MaxMp += x;
        }

        public void UseMp(int x) => this.UsedMp = Math.Min(this.MaxMp, this.UsedMp + x);

        public void GainMp(int x) => this.UsedMp = Math.Max(0, this.UsedMp - x);

        public void FullMp() => this.UsedMp = 0;

        public void Damage(Card card) => this.Damage(card.Power);

        public void Damage(int x) => this.UsedHp = Math.Min(this.MaxHp, this.UsedHp + x);

        public void GainHp(int x) => this.UsedHp = Math.Max(0, this.UsedHp - x);

        public void Modify(PlayerModifier modifier)
        {
            if (modifier.MaxHp != null)
            {
                this.MaxHp = modifier.MaxHp.Modify(this.MaxHp);
            }

            if (modifier.Hp != null)
            {
                this.GainHp(modifier.Hp.Modify(this.CurrentHp) - this.CurrentHp);
            }

            if (modifier.MaxMp != null)
            {
                this.MaxMp = modifier.MaxMp.Modify(this.MaxMp);
            }

            if (modifier.Mp != null)
            {
                this.GainMp(modifier.Mp.Modify(this.CurrentMp) - this.CurrentMp);
            }
        }
    }
}
