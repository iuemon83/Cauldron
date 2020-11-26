using System;
using System.Collections.Generic;

namespace Cauldron.Core
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

        public int Hp { get; private set; }
        public int MaxMp { get; private set; }
        public int UsedMp { get; private set; }
        public int UsableMp => Math.Max(0, this.MaxMp - this.UsedMp);

        public Player(Guid id, string name, int hp, RuleBook ruleBook, IReadOnlyList<Card> deck)
        {
            foreach (var card in deck)
            {
                card.OwnerId = id;
            }

            this.RuleBook = ruleBook;
            this.Id = id;
            this.Name = name;
            this.Hp = hp;
            this.MaxMp = this.RuleBook.InitialMp;
            this.Deck = new Deck(deck);
            this.Hands = new Hands();
            this.Field = new Field(this.RuleBook);

            foreach (var card in deck)
            {
                this.CardsById.Add(card.Id, card);
            }
        }

        public void Draw()
        {
            var newCard = this.Deck.Draw();
            this.Hands.Add(newCard);
        }

        public void ModifyMaxMp(int x)
        {
            this.MaxMp += x;
        }

        public void UseMp(int x)
        {
            this.UsedMp = Math.Min(this.MaxMp, this.UsedMp + x);
        }

        public void RecoverMp(int x)
        {
            this.UsedMp = Math.Max(0, this.UsedMp - x);
        }

        public void FullMp()
        {
            this.UsedMp = 0;
        }

        public void Damage(Card card)
        {
            this.Hp = Math.Max(0, this.Hp - card.Power);
        }

        public void Damage(int damage)
        {
            this.Hp = Math.Max(0, this.Hp - damage);
        }
    }
}
