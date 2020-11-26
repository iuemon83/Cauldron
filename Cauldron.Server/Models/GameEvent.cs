namespace Cauldron.Server.Models
{
    public enum GameEvent
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
        /// すべてのターン開始時
        /// </summary>
        OnStartTurn,

        /// <summary>
        /// すべてのターン終了時
        /// </summary>
        OnEndTurn,
    }
}
