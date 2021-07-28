using Cauldron.Shared.MessagePackObjects.Value;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    /// <summary>
    /// 選択候補
    /// </summary>
    [MessagePackObject(true)]
    public class ChoiceSource
    {
        public enum HowValue
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
        public HowValue How { get; }

        /// <summary>
        /// 抽出する数
        /// </summary>
        public NumValue NumPicks { get; }

        /// <summary>
        /// 候補となるプレイヤーのOR条件の一覧。
        /// </summary>
        public PlayerCondition[] OrPlayerConditions { get; }

        /// <summary>
        /// 候補となるカードのOR条件の一覧。
        /// </summary>
        public CardCondition[] OrCardConditions { get; }

        public ChoiceSource(HowValue how = HowValue.All, NumValue numPicks = null,
            PlayerCondition[] orPlayerConditions = null, CardCondition[] orCardConditions = null)
        {
            this.How = how;
            this.NumPicks = numPicks ?? new NumValue(1);
            this.OrPlayerConditions = orPlayerConditions ?? new PlayerCondition[0];
            this.OrCardConditions = orCardConditions ?? new CardCondition[0];
        }
    }
}
