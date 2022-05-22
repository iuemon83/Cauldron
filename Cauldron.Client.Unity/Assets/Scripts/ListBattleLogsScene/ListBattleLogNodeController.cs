using Assets.Scripts;
using TMPro;
using UnityEngine;

public class ListBattleLogNodeController : MonoBehaviour
{
    [SerializeField]
    private Color winColor = default;
    [SerializeField]
    private Color loseColor = default;

    [SerializeField]
    private TextMeshProUGUI timestampText = default;
    [SerializeField]
    private TextMeshProUGUI IsWinText = default;
    [SerializeField]
    private TextMeshProUGUI opponentNameText = default;

    public LocalBattleLog Source { get; private set; }

    public void Set(LocalBattleLog source)
    {
        this.Source = source;
        this.timestampText.text = source.TimestampText;
        this.IsWinText.text = source.IsWin ? "Win" : "Lose";
        this.IsWinText.color = source.IsWin ? winColor : loseColor;
        this.opponentNameText.text = source.OpponentName;
    }
}
