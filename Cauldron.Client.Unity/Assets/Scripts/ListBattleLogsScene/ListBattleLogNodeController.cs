using Assets.Scripts.ServerShared.MessagePackObjects;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ListBattleLogNodeController : MonoBehaviour
{
    private static readonly Color backgroundColor1 = new Color(0.4433962f, 0.4433962f, 0.4433962f, 0.772549f);
    private static readonly Color backgroundColor2 = new Color(0.1886792f, 0.1886792f, 0.1886792f, 0.772549f);

    [SerializeField]
    private Image backgroundImage = default;
    [SerializeField]
    private TextMeshProUGUI timestampText = default;
    [SerializeField]
    private TextMeshProUGUI player1NameText = default;
    [SerializeField]
    private TextMeshProUGUI player2NameText = default;

    private Action playGameReplayAction;
    private Action showCopyGameIdMessage;

    private GameReplay gameReplay;

    public void Set(int index, GameReplay gameReplay, Action playGameReplayAction, Action showCopyGameIdMessage)
    {
        this.backgroundImage.color = index % 2 == 0
            ? backgroundColor1
            : backgroundColor2;

        this.gameReplay = gameReplay;
        this.timestampText.text = gameReplay.DateTime.ToString();
        this.player1NameText.text = gameReplay.Players[0].Name;
        this.player2NameText.text = gameReplay.Players[1].Name;
        this.playGameReplayAction = playGameReplayAction;
        this.showCopyGameIdMessage = showCopyGameIdMessage;
    }

    public void OnPlayReplayButtonClicked()
    {
        this.playGameReplayAction?.Invoke();
    }

    public void OnCopyGameIdButtonClicked()
    {
        GUIUtility.systemCopyBuffer = this.gameReplay.GameId.ToString();
        this.showCopyGameIdMessage?.Invoke();
    }
}
