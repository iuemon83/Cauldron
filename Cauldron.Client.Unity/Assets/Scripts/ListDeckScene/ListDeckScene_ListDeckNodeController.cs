using Assets.Scripts;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ListDeckScene_ListDeckNodeController : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private TextMeshProUGUI deckNameText = default;

    [SerializeField]
    private Image backgroundImage = default;

    public IDeck Source { get; private set; }

    public Action<ListDeckScene_ListDeckNodeController> SelectNodeAction { get; set; }

    private Color DeselectedTextColor;
    private Color DeselectedBackgroundColor;

    private Color SelectedTextColor = Color.white;
    private Color SelectedBackgroundColor = new Color(0.8f, 0.2f, 0.2f, 0.4f);

    // Start is called before the first frame update
    void Start()
    {
        this.DeselectedTextColor = this.deckNameText.color;
        this.DeselectedBackgroundColor = this.backgroundImage.color;
    }

    public void Set(IDeck source)
    {
        this.Source = source;
        this.deckNameText.text = source.Name;
    }

    public void SetDeselectedColor()
    {
        this.deckNameText.color = this.DeselectedTextColor;
        this.backgroundImage.color = this.DeselectedBackgroundColor;
    }

    public void SetSelectedColor()
    {
        this.deckNameText.color = this.SelectedTextColor;
        this.backgroundImage.color = this.SelectedBackgroundColor;
    }

    void IPointerClickHandler.OnPointerClick(PointerEventData eventData)
    {
        this.SelectNodeAction?.Invoke(this);
    }
}
