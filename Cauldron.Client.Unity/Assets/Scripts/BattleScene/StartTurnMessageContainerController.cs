using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StartTurnMessageContainerController : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI messageText = default;
    [SerializeField]
    private Image backgroundImage = default;

    public float Width => this.messageText.rectTransform.rect.width;

    public void AsStartGame()
    {
        this.messageText.text = "対 戦 開 始";
        this.messageText.color = Color.black;
        this.backgroundImage.color = Color.white;
    }

    public void AsYou()
    {
        this.messageText.text = "あなたのターン";
        this.messageText.color = Color.white;
        this.backgroundImage.color = BattleSceneController.Instance.YouColor;
    }

    public void AsOpponent()
    {
        this.messageText.text = "相手のターン";
        this.messageText.color = Color.white;
        this.backgroundImage.color = BattleSceneController.Instance.OpponentColor;
    }
}
