using Assets.Scripts;
using Assets.Scripts.ServerShared.MessagePackObjects;
using System;
using TMPro;
using UnityEngine;

public class ListBattleLogNodeController : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI timestampText = default;
    [SerializeField]
    private TextMeshProUGUI player1NameText = default;
    [SerializeField]
    private TextMeshProUGUI player2NameText = default;

    private Action playGameReplayAction;

    private GameReplay gameReplay;

    public void Set(GameReplay gameReplay, Action playGameReplayAction)
    {
        this.gameReplay = gameReplay;
        this.timestampText.text = gameReplay.DateTime.ToString();
        this.player1NameText.text = "Player1";
        this.player2NameText.text = "Player2";
        this.playGameReplayAction = playGameReplayAction;
    }

    public void Set(GameReplay gameReplay, Action okButtonClickAction, LocalBattleLog localBattleLog)
    {
        this.Set(gameReplay, okButtonClickAction);

        this.player1NameText.text = gameReplay.PlayerIdList[0].ToString() == localBattleLog.YouIdText
            ? localBattleLog.YouName
            : localBattleLog.OpponentName;
        this.player2NameText.text = gameReplay.PlayerIdList[1].ToString() == localBattleLog.YouIdText
            ? localBattleLog.YouName
            : localBattleLog.OpponentName;
    }

    public void OnPlayReplayButtonClicked()
    {
        this.playGameReplayAction?.Invoke();
    }

    public void OnCopyGameIdButtonClicked()
    {
        GUIUtility.systemCopyBuffer = this.gameReplay.GameId.ToString();
    }
}
