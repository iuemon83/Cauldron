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
        this.messageText.text = "�� �� �J �n";
        this.messageText.color = Color.black;
        this.backgroundImage.color = Color.white;
    }

    public void AsYou()
    {
        this.messageText.text = "���Ȃ��̃^�[��";
        this.messageText.color = Color.white;
        this.backgroundImage.color = BattleSceneController.Instance.YouColor;
    }

    public void AsOpponent()
    {
        this.messageText.text = "����̃^�[��";
        this.messageText.color = Color.white;
        this.backgroundImage.color = BattleSceneController.Instance.OpponentColor;
    }
}
