namespace Cauldron.Shared.MessagePackObjects
{
    public enum GameMasterStatusCode
    {
        OK,

        /// <summary>
        /// ゲームがまだ開始していない
        /// </summary>
        NotStart,

        /// <summary>
        /// ターンがまだ開始していない
        /// </summary>
        NotTurnStart,

        /// <summary>
        /// ターンがすでに開始している
        /// </summary>
        AlreadyTurnStarted,

        /// <summary>
        /// 不正なデッキ
        /// </summary>
        InvalidDeck,

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
        CantAttack,

        /// <summary>
        /// 質問ID が正しくない
        /// </summary>
        InvalidQuestionId,
    }
}
