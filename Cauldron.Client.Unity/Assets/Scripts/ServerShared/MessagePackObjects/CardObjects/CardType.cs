using Assets.Scripts.ServerShared.MessagePackObjects;

namespace Cauldron.Shared
{
    public enum CardType
    {
        /// <summary>
        /// クリーチャー
        /// </summary>
        [DisplayText("クリーチャー")]
        Creature,

        /// <summary>
        /// クリーチャーじゃないけどフィールドに残る
        /// </summary>
        [DisplayText("アーティファクト")]
        Artifact,

        /// <summary>
        /// 魔法
        /// </summary>
        [DisplayText("魔法")]
        Sorcery,
    }
}
