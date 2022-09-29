using Cauldron.Shared.MessagePackObjects;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ListPlayerNodeController : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private TextMeshProUGUI playerNameText = default;

    [SerializeField]
    private Image backgroundImage = default;

    public PlayerId PlayerId { get; private set; }

    private Action<ListPlayerNodeController> selectNodeAction;

    private Color deselectedTextColor;
    private Color deselectedBackgroundColor;

    private Color selectedTextColor = Color.white;
    private Color selectedBackgroundColor = new Color(0.8f, 0.2f, 0.2f, 0.4f);

    // Start is called before the first frame update
    void Start()
    {
        this.deselectedTextColor = this.playerNameText.color;
        this.deselectedBackgroundColor = this.backgroundImage.color;
    }

    public void Set(PlayerId playerId, string playerName, Action<ListPlayerNodeController> selectNodeAction)
    {
        this.PlayerId = playerId;
        this.playerNameText.text = playerName;
        this.selectNodeAction = selectNodeAction;
    }

    public void SetDeselectedColor()
    {
        this.playerNameText.color = this.deselectedTextColor;
        this.backgroundImage.color = this.deselectedBackgroundColor;
    }

    public void SetSelectedColor()
    {
        this.playerNameText.color = this.selectedTextColor;
        this.backgroundImage.color = this.selectedBackgroundColor;
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        this.selectNodeAction?.Invoke(this);
    }
}
