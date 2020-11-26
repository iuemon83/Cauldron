namespace Cauldron.Core
{
    public enum CardEffectType
    {
        /// <summary>
        /// このカードのプレイ時
        /// </summary>
        OnPlay,

        /// <summary>
        /// このカードの破壊時
        /// </summary>
        OnDestroy,

        /// <summary>
        /// すべてのカードの破壊時
        /// </summary>
        OnEveryDestroy,

        /// <summary>
        /// すべてのカードのプレイ時
        /// </summary>
        OnEveryPlay,

        /// <summary>
        /// このカードの被ダメージ時
        /// </summary>
        OnDamage,

        /// <summary>
        /// すべてのカードの被ダメージ時
        /// </summary>
        OnEveryDamage,

        /// <summary>
        /// ターン開始時
        /// </summary>
        OnStartTurn,

        /// <summary>
        /// ターン終了時
        /// </summary>
        OnEndTurn,
    }
}
