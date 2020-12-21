namespace Cauldron.Server.Models
{
    public enum GameMasterStatusCode
    {
        OK,

        /// <summary>
        /// デッキにトークンが含まれている
        /// </summary>
        IsIncludedTokensInDeck,

        /// <summary>
        /// 指定されたカードが存在しません
        /// </summary>
        CardNotExists,

        /// <summary>
        /// 指定されたカードがプレイ不能です
        /// </summary>
        CardCantPlay,

        /// <summary>
        /// すでにゲームが終了している
        /// </summary>
        GameOver,

        /// <summary>
        /// このプレイヤーのターンではありません
        /// </summary>
        NotActivePlayer,
    }
}
