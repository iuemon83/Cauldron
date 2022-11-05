using Cauldron.Shared.MessagePackObjects;
using System;
using UnityEngine;

namespace Assets.Scripts.BattleScene
{
    class EndGameDialog
    {
        public static void ShowEndGameDialog(
            EndGameNotifyMessage notify, ConfirmDialogController dialog, Canvas canvas, PlayerId playerId, Action onOkAction)
        {
            var title = "ゲーム終了";
            var isWin = notify.WinnerPlayerId == playerId;
            var winOrLoseText = isWin
                ? "Win !"
                : "Lose...";

            var reasonText = notify.EndGameReason switch
            {
                EndGameReason.HpIsZero => $"{(isWin ? "相手" : "あなた")}のHPが0になりました。",
                EndGameReason.CardEffect => $"「{notify.EffectOwnerCard?.Name ?? ""}」の効果でゲームが終了しました。",
                EndGameReason.Surrender => $"{(isWin ? "相手" : "あなた")}が降参しました。",
                EndGameReason.LibraryOut => $"{(isWin ? "相手" : "あなた")}がデッキからカードをドローできませんでした。",
                _ => throw new NotImplementedException($"正しくない値が指定されました。EndGameReason={notify.EndGameReason}"),
            };

            var message = winOrLoseText + Environment.NewLine + reasonText;

            AudioController.CreateOrFind().PlaySe(isWin ? SeAudioCache.SeAudioType.Win : SeAudioCache.SeAudioType.Lose);

            dialog.Init(title, message, ConfirmDialogController.DialogType.Message,
                onOkAction: onOkAction);
            dialog.transform.SetParent(canvas.transform, false);
        }
    }
}
