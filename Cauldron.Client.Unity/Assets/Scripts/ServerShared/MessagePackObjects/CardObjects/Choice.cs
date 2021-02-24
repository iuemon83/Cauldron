using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
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

        /// <summary>
        /// 候補からの洗濯方法
        /// </summary>
        public ChoiceHow How { get; set; } = ChoiceHow.All;

        /// <summary>
        /// 候補となるプレイヤーの条件
        /// </summary>
        public PlayerCondition PlayerCondition { get; set; }

        /// <summary>
        /// 候補となるカードの条件
        /// </summary>
        public CardCondition CardCondition { get; set; }

        /// <summary>
        /// 選択する数
        /// </summary>
        public int NumPicks { get; set; } = 1;
    }
}
