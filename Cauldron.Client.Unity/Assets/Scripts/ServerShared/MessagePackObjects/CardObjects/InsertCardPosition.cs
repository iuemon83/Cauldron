using MessagePack;

namespace Cauldron.Shared
{
    [MessagePackObject(true)]
    public class InsertCardPosition
    {
        public enum PositionTypeValue
        {
            /// <summary>
            /// 前からのインデックス
            /// </summary>
            Top,

            /// <summary>
            /// 後ろからのインデックス
            /// </summary>
            Bottom,

            /// <summary>
            /// ランダムな位置
            /// </summary>
            Random
        }

        public PositionTypeValue PositionType { get; }

        /// <summary>
        /// 1から始まるインデックス
        /// </summary>
        public int PositionIndex { get; }

        public InsertCardPosition(PositionTypeValue PositionType, int PositionIndex)
        {
            this.PositionType = PositionType;
            this.PositionIndex = PositionIndex;
        }
    }
}
