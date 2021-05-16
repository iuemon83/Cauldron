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
        /// 選択候補
        /// </summary>
        public ChoiceSource Source { get; }

        /// <summary>
        /// 候補からの選択方法
        /// </summary>
        public ChoiceHow How { get; }

        /// <summary>
        /// 選択する数
        /// </summary>
        public int NumPicks { get; }

        public Choice(ChoiceSource source, ChoiceHow how = ChoiceHow.All, int numPicks = 1)
        {
            this.Source = source;
            this.How = how;
            this.NumPicks = numPicks;
        }
    }
}
