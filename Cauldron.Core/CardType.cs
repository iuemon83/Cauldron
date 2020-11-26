namespace Cauldron.Core
{
    public enum CardType
    {
        /// <summary>
        /// 不明
        /// </summary>
        Unknown,

        /// <summary>
        /// クリーチャー
        /// </summary>
        Creature,

        /// <summary>
        /// クリーチャーじゃないけどフィールドに残る
        /// </summary>
        Artifact,

        /// <summary>
        /// 魔法
        /// </summary>
        Sorcery,

        Token,

        Counter,
    }
}
