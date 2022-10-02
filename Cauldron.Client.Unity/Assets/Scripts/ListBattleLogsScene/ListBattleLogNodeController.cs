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
        this.player1NameText.text = gameReplay.Players[0].Name;
        this.player2NameText.text = gameReplay.Players[1].Name;
        this.playGameReplayAction = playGameReplayAction;
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
