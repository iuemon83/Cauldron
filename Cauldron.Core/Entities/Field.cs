using Cauldron.Shared.MessagePackObjects;

namespace Cauldron.Core.Entities
{
    public class Field
    {
        /// <summary>
        /// 順序を保存するため
        /// </summary>
        private Card?[] CardContainers { get; }

        public IReadOnlyList<Card> AllCards => this.CardContainers.OfType<Card>().ToArray();

        public IReadOnlyList<Card?> AllCardsWithIndex => this.CardContainers;
        public IReadOnlyList<bool> IsAvailabledList => this.isAvailabledList;

        public int Count => this.AllCards.Count;

        public bool Full => this.CardContainers
            .Zip(this.isAvailabledList)
            .Where(x => x.Second)
            .All(x => x.First != null);

        public int CurrentLimit => this.isAvailabledList.Count(x => x);

        public int Limit => this.isAvailabledList.Length;

        private readonly bool[] isAvailabledList;

        public Field(RuleBook ruleBook)
        {
            this.CardContainers = new Card?[ruleBook.MaxNumFields];
            this.isAvailabledList = new bool[ruleBook.MaxNumFields];
            foreach (var i in Enumerable.Range(0, ruleBook.StartMaxNumFields))
            {
                this.isAvailabledList[i] = true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="card"></param>
        /// <returns>追加した場所のインデックス</returns>
        public int Add(Card card)
        {
            var index = Enumerable.Range(0, this.CardContainers.Length)
                .FirstOrDefault(i => this.CardContainers[i] == null
                    && this.isAvailabledList[i],
                    -1);

            return this.Add(card, index);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="card"></param>
        /// <param name="index">0ベース</param>
        /// <returns>追加した場所のインデックス</returns>
        public int Add(Card card, int index)
        {
            if (this.Full)
            {
                return -1;
            }

            if (index < 0 || index >= this.CardContainers.Length)
            {
                return -1;
            }

            if (this.CardContainers[index] != null)
            {
                return -1;
            }

            this.CardContainers[index] = card;
            return index;
        }

        public void Remove(Card card)
        {
            foreach (var i in Enumerable.Range(0, this.CardContainers.Length))
            {
                if (this.CardContainers[i]?.Id == card.Id)
                {
                    this.CardContainers[i] = null;
                    return;
                }
            }
        }

        public int UpdateLimit(int newLimit)
        {
            var diff = Math.Max(0, Math.Min(newLimit, this.Limit)) - this.CurrentLimit;

            if (diff == 0)
            {
                return 0;
            }

            if (diff > 0)
            {
                var count = 0;
                foreach (var i in Enumerable.Range(0, this.isAvailabledList.Length))
                {
                    if (count == diff)
                    {
                        break;
                    }

                    if (!this.isAvailabledList[i])
                    {
                        this.isAvailabledList[i] = true;
                        count++;
                    }
                }

                return count;
            }
            else
            {
                // 数が減る
                // すでにカードが置かれている場は無効にしない
                var count = 0;
                for (var i = this.CardContainers.Length - 1; i >= 0; i--)
                {
                    if (count == diff)
                    {
                        break;
                    }

                    if (this.CardContainers[i] != null)
                    {
                        continue;
                    }

                    if (this.isAvailabledList[i])
                    {
                        this.isAvailabledList[i] = false;
                        count--;
                    }
                }

                return count;
            }
        }

        public int Index(Card card)
        {
            return Array.IndexOf(this.CardContainers, card);
        }
    }
}
