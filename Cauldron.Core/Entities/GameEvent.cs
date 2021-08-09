namespace Cauldron.Core.Entities
{
    public enum GameEvent
    {
        /// <summary>
        /// カードのプレイ時
        /// </summary>
        OnPlay,

        /// <summary>
        /// カードの破壊時
        /// </summary>
        OnDestroy,

        /// <summary>
        /// いずれかがダメージを受ける前
        /// </summary>
        OnDamageBefore,

        /// <summary>
        /// いずれかがダメージを受けるあと
        /// </summary>
        OnDamage,

        /// <summary>
        /// 攻撃前
        /// </summary>
        OnAttackBefore,

        /// <summary>
        /// 攻撃後
        /// </summary>
        OnAttack,

        /// <summary>
        /// カードのドロー時
        /// </summary>
        OnDraw,

        /// <summary>
        /// ライフの回復時
        /// </summary>
        OnGainLife,

        /// <summary>
        /// カードの領域間の移動時
        /// </summary>
        OnMoveCard,

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
