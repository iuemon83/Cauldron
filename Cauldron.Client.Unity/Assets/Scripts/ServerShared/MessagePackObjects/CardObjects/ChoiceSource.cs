using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    /// <summary>
    /// 選択候補
    /// </summary>
    [MessagePackObject(true)]
    public class ChoiceSource
    {
        public enum ChoiceHow
        {
            /// <summary>
            /// ランダム選択
            /// </summary>
            Random,
            /// <summary>
            /// すべて
            /// </summary>
            All,
        }

        /// <summary>
        /// 抽出方法
        /// </summary>
        public ChoiceHow How { get; }

        /// <summary>
        /// 抽出する数
        /// </summary>
        public int NumPicks { get; }

        /// <summary>
        /// 候補となるプレイヤーのOR条件の一覧。
        /// </summary>
        public PlayerCondition[] OrPlayerConditions { get; }

        /// <summary>
        /// 候補となるカードのOR条件の一覧。
        /// </summary>
        public CardCondition[] OrCardConditions { get; }

        public ChoiceSource(ChoiceHow how = ChoiceHow.All, int numPicks = 1, PlayerCondition[] orPlayerConditions = null, CardCondition[] orCardConditions = null)
        {
            this.How = how;
            this.NumPicks = numPicks;
            this.OrPlayerConditions = orPlayerConditions ?? new PlayerCondition[0];
            this.OrCardConditions = orCardConditions ?? new CardCondition[0];
        }
    }
}
