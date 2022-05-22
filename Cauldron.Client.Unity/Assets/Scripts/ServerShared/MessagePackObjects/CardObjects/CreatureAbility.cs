using Assets.Scripts.ServerShared.MessagePackObjects;

namespace Cauldron.Shared
{
    public enum CreatureAbility
    {
        [DisplayText("なし")]
        None,

        /// <summary>
        /// 守護
        /// </summary>
        [DisplayText("守護")]
        Cover,

        /// <summary>
        /// ステルス
        /// </summary>
        [DisplayText("ステルス")]
        Stealth,

        /// <summary>
        /// 必殺
        /// </summary>
        [DisplayText("必殺")]
        Deadly,

        /// <summary>
        /// 封印
        /// </summary>
        [DisplayText("封印")]
        Sealed
    }
}
