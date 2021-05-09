using Assets.Scripts;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ListDeckNodeController : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private TextMeshProUGUI deckNameText;

    [SerializeField]
    private Image backgroundImage;

    public IDeck Source { get; private set; }

    private Action<ListDeckNodeController> selectNodeAction;

    private Color deselectedTextColor;
    private Color deselectedBackgroundColor;

    private Color selectedTextColor = Color.white;
    private Color selectedBackgroundColor = new Color(0.8f, 0.2f, 0.2f, 0.4f);

    // Start is called before the first frame update
    void Start()
    {
        this.deselectedTextColor = this.deckNameText.color;
        this.deselectedBackgroundColor = this.backgroundImage.color;
    }

    public void Set(IDeck source, Action<ListDeckNodeController> selectNodeAction)
    {
        this.Source = source;
        this.deckNameText.text = source.Name;
        this.selectNodeAction = selectNodeAction;
    }

    public void SetDeselectedColor()
    {
        this.deckNameText.color = this.deselectedTextColor;
        this.backgroundImage.color = this.deselectedBackgroundColor;
    }

    public void SetSelectedColor()
    {
        this.deckNameText.color = this.selectedTextColor;
        this.backgroundImage.color = this.selectedBackgroundColor;
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        this.selectNodeAction?.Invoke(this);
    }
}
