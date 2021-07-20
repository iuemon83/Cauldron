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
        /// 上から（下から）1枚目=1
        /// </summary>
        public int PositionIndex { get; }

        public InsertCardPosition(PositionTypeValue PositionType, int PositionIndex = 1)
        {
            this.PositionType = PositionType;
            this.PositionIndex = PositionIndex;
        }
    }
}
