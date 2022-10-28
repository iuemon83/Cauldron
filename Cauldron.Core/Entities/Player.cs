using Cauldron.Core.Entities.Effect;
using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using Cauldron.Shared.MessagePackObjects.Value;

namespace Cauldron.Core.Entities
{
    public class Player
    {
        private RuleBook RuleBook { get; }

        public PlayerId Id { get; }

        public string Name { get; } = "";

        public Hands Hands { get; }

        public Deck Deck { get; }

        public Field Field { get; }

        public Cemetery Cemetery { get; }

        public List<CardDef> Excludes { get; }

        public bool IsFirst => this.PlayOrder == 0;

        public int PlayOrder { get; }

        public int MaxHp { get; private set; }
        public int UsedHp { get; private set; }
        public int CurrentHp => Math.Max(0, this.MaxHp - this.UsedHp);

        /// <summary>
        /// ゲーム中のMPの上限値
        /// </summary>
        public int MaxLimitMp { get; private set; }

        /// <summary>
        /// 現時点でのMPの上限値
        /// </summary>
        public int MaxMp { get; private set; }
        public int UsedMp { get; private set; }
        public int CurrentMp => Math.Max(0, this.MaxMp - this.UsedMp);

        private readonly Dictionary<string, int> CountersByName = new();

        public Player(PlayerId id, string name, RuleBook ruleBook, IReadOnlyList<Card> deck, int playOrder)
        {
            foreach (var card in deck)
            {
                card.OwnerId = id;
            }

            this.PlayOrder = playOrder;
            this.RuleBook = ruleBook;
            this.Id = id;
            this.Name = name;
            this.MaxHp = this.RuleBook.MaxPlayerHp;
            this.UsedHp = this.MaxHp - this.RuleBook.StartPlayerHp;
            this.MaxLimitMp = this.RuleBook.MaxLimitMp;
            this.MaxMp = this.IsFirst ? this.RuleBook.FirstPlayerStartMp : this.RuleBook.SecondPlayerStartMp;
            this.Deck = new Deck(deck);
            this.Hands = new Hands(this.RuleBook);
            this.Field = new Field(this.RuleBook);
            this.Cemetery = new Cemetery();
            this.Excludes = new List<CardDef>();

            foreach (var card in deck)
            {
                card.Zone = new Zone(id, ZoneName.Deck);
            }
        }

        /// <summary>
        /// MP を最大値まで回復
        /// </summary>
        public void FullMp() => this.UsedMp = 0;

        public void Damage(int x) => this.UsedHp = Math.Min(this.MaxHp, this.UsedHp + x);

        public void UseMp(int x) => this.UsedMp = Math.Min(this.MaxMp, this.UsedMp + x);

        /// <summary>
        /// MPの最大値を増加させる
        /// </summary>
        /// <param name="x"></param>
        /// <returns>実際に増加した値</returns>
        public int AddMaxHp(int x)
        {
            var prevValue = this.MaxHp;
            this.MaxHp += x;

            return this.MaxHp - prevValue;
        }

        /// <summary>
        /// MPの最大値を増加させる
        /// </summary>
        /// <param name="x"></param>
        /// <returns>実際に増加した値</returns>
        public int AddMaxMp(int x)
        {
            var prevValue = this.MaxMp;
            this.MaxMp = Math.Min(this.MaxMp + x, this.MaxLimitMp);

            return this.MaxMp - prevValue;
        }

        /// <summary>
        /// MPを増加させる
        /// </summary>
        /// <param name="x"></param>
        /// <returns>実際に増加したMP</returns>
        public int GainMp(int x)
        {
            var prevValue = this.CurrentMp;
            this.UsedMp = Math.Max(0, this.UsedMp - x);

            return this.CurrentMp - prevValue;
        }

        /// <summary>
        /// HPを増加させる
        /// </summary>
        /// <param name="x"></param>
        /// <returns>実際に増加したHP</returns>
        public int GainHp(int x)
        {
            var prevValue = this.CurrentHp;
            this.UsedHp = Math.Max(0, this.UsedHp - x);

            return this.CurrentHp - prevValue;
        }

        public ModifyPlayerContext Modify(ModifyPlayerContext modifyPlayerContext)
        {
            return new ModifyPlayerContext(
                this.Id,
                this.AddMaxHp(modifyPlayerContext.DiffMaxHp),
                this.GainHp(modifyPlayerContext.DiffCurrentHp),
                this.AddMaxMp(modifyPlayerContext.DiffMaxMp),
                this.GainMp(modifyPlayerContext.DiffCurrentMp)
            );
        }

        public void ModifyCounter(string name, int addValue)
        {
            if (!this.CountersByName.TryGetValue(name, out var num))
            {
                this.CountersByName.Add(name, 0);
            }

            var newNum = num + addValue;

            if (newNum > 0)
            {
                this.CountersByName[name] = newNum;
            }
            else
            {
                this.CountersByName.Remove(name);
            }
        }

        public int GetCounter(string name)
        {
            return this.CountersByName.TryGetValue(name, out var num)
                ? num
                : 0;
        }
    }
}
