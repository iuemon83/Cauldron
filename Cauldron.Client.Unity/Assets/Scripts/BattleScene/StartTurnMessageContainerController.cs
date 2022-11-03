using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartTurnMessageContainerController : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI startGameMessageText = default;
    [SerializeField]
    private TextMeshProUGUI messageText = default;
    [SerializeField]
    private TextMeshProUGUI turnMessageText = default;
    [SerializeField]
    private Image backgroundImage = default;

    public float Width => this.messageText.rectTransform.rect.width;

    public void AsStartGame()
    {
        this.messageText.gameObject.SetActive(false);
        this.turnMessageText.gameObject.SetActive(false);

        this.startGameMessageText.text = "対 戦 開 始";
        this.startGameMessageText.color = Color.black;
        this.startGameMessageText.gameObject.SetActive(true);

        this.backgroundImage.color = Color.white;
    }

    public void AsYou(int turnCount)
    {
        this.startGameMessageText.gameObject.SetActive(false);

        this.messageText.text = $"あなたのターン";
        this.messageText.color = Color.white;
        this.turnMessageText.text = $"~{turnCount}~";
        this.turnMessageText.color = Color.white;

        this.messageText.gameObject.SetActive(true);
        this.turnMessageText.gameObject.SetActive(true);

        this.backgroundImage.color = BattleSceneController.Instance.YouColor;
    }

    public void AsOpponent(int turnCount)
    {
        this.startGameMessageText.gameObject.SetActive(false);

        this.messageText.text = $"相手のターン";
        this.messageText.color = Color.white;
        this.turnMessageText.text = $"~{turnCount}~";
        this.turnMessageText.color = Color.white;

        this.messageText.gameObject.SetActive(true);
        this.turnMessageText.gameObject.SetActive(true);

        this.backgroundImage.color = BattleSceneController.Instance.OpponentColor;
    }
}
