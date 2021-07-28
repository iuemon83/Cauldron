using Assets.Scripts.ServerShared.MessagePackObjects;
using Cauldron.Shared.MessagePackObjects.Value;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class Choice
    {
        public enum HowValue
        {
            /// <summary>
            /// ランダム選択
            /// </summary>
            [DisplayText("ランダム")]
            Random,
            /// <summary>
            /// 選択する
            /// </summary>
            [DisplayText("選択")]
            Choose,
            /// <summary>
            /// 候補すべて
            /// </summary>
            [DisplayText("すべて")]
            All,
        }

        /// <summary>
        /// 選択候補
        /// </summary>
        public ChoiceSource Source { get; }

        /// <summary>
        /// 候補からの選択方法
        /// </summary>
        public HowValue How { get; }

        /// <summary>
        /// 選択する数
        /// </summary>
        public NumValue NumPicks { get; }

        public Choice(ChoiceSource source, HowValue how = HowValue.All, NumValue numPicks = null)
        {
            this.Source = source;
            this.How = how;
            this.NumPicks = numPicks ?? new NumValue(1);
        }
    }
}
