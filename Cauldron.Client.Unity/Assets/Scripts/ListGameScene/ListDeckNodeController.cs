using Assets.Scripts;
using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ListDeckNodeController : MonoBehaviour, IPointerClickHandler
{
    [SerializeField]
    private Text deckNameText;

    [SerializeField]
    private Image backgroundImage;

    public IDeck Source { get; set; }

    public Action<ListDeckNodeController> SelectNodeAction { get; set; }

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

    // Update is called once per frame
    void Update()
    {
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
