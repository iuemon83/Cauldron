using System.Collections.Generic;

namespace Cauldron.Server.Models.Effect
{
    public class Choice
    {
        public enum ChoiceHow
        {
            /// <summary>
            /// ランダム選択
            /// </summary>
            Random,
            /// <summary>
            /// 選択する
            /// </summary>
            Choose,
            /// <summary>
            /// 候補すべて
            /// </summary>
            All,
        }

        public enum ChoiceCandidateType
        {
            /// <summary>
            /// 全プレイヤー
            /// </summary>
            AllPlayer,
            /// <summary>
            /// カードの所持プレイヤー
            /// </summary>
            OwnerPlayer,
            /// <summary>
            /// カードの所持プレイヤー以外
            /// </summary>
            OtherOwnerPlayer,
            /// <summary>
            /// ターンのカレントプレイヤー
            /// </summary>
            TurnPlayer,
            /// <summary>
            /// ターンのカレントプレイヤー以外
            /// </summary>
            OtherTurnPlayer,
            /// <summary>
            /// カード
            /// </summary>
            Card,
        }

        /// <summary>
        /// 候補からの洗濯方法
        /// </summary>
        public ChoiceHow How { get; set; } = ChoiceHow.All;

        /// <summary>
        /// 候補へ含む種類
        /// </summary>
        public IReadOnlyList<ChoiceCandidateType> Candidates { get; set; } = new ChoiceCandidateType[0];

        /// <summary>
        /// 候補となるカードの条件
        /// </summary>
        public CardCondition CardCondition { get; set; } = new CardCondition();

        /// <summary>
        /// 選択する数
        /// </summary>
        public int NumPicks { get; set; } = 0;
    }
}
