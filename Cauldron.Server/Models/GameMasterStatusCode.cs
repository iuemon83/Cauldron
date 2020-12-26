﻿namespace Cauldron.Server.Models
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
        /// 指定されたカードがプレイできません
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

        /// <summary>
        /// 指定されたプレイヤーが存在しません
        /// </summary>
        PlayerNotExists,

        /// <summary>
        /// 攻撃できません
        /// </summary>
        CandAttack,
    }
}
