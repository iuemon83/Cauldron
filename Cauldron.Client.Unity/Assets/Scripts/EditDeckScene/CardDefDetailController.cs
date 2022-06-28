using Assets.Scripts;
using Cauldron.Shared.MessagePackObjects;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardDefDetailController : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI cardNameText = default;
    [SerializeField]
    private TextMeshProUGUI effectText = default;

    [SerializeField]
    private Image powerSpace = default;
    [SerializeField]
    private Image toughnessSpace = default;

    [SerializeField]
    private TextMeshProUGUI cardTypeText = default;
    [SerializeField]
    private TextMeshProUGUI costText = default;
    [SerializeField]
    private TextMeshProUGUI powerText = default;
    [SerializeField]
    private TextMeshProUGUI toughnessText = default;

    protected CardBridge source;
    private bool requireUpdate;

    private Action<CardDefId> addToDeck;
    private Action<CardDefId> removeFromDeck;
    private Action<CardDefId> displayBigDetail;

    private void Start()
    {
        this.cardNameText.text = "";
        this.effectText.text = "";
        this.costText.text = "";

        this.powerSpace.gameObject.SetActive(false);
        this.toughnessSpace.gameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (this.source == null || !this.requireUpdate)
        {
            return;
        }

        this.cardNameText.text = this.source.Name;
        this.effectText.text = Utility.EffectDescription(this.source);
        this.cardTypeText.text = Utility.CardTypeIconUnicode(this.source.Type);

        this.costText.text = this.source.Cost.ToString();

        switch (this.source.Type)
        {
            case CardType.Creature:
                this.powerText.text = this.source.Power.ToString();
                this.toughnessText.text = this.source.Toughness.ToString();
                this.powerSpace.gameObject.SetActive(true);
                this.toughnessSpace.gameObject.SetActive(true);
                break;

            default:
                this.powerSpace.gameObject.SetActive(false);
                this.toughnessSpace.gameObject.SetActive(false);
                break;
        }
    }

    public void Init(
        Action<CardDefId> addToDeck,
        Action<CardDefId> removeFromDeck,
        Action<CardDefId> displayBigDetail
        )
    {
        this.addToDeck = addToDeck;
        this.removeFromDeck = removeFromDeck;
        this.displayBigDetail = displayBigDetail;
    }

    public void SetCard(CardDef cardDef)
    {
        this.source = new CardBridge(cardDef, default);
        this.requireUpdate = true;
    }

    public void OnAddToDeckButtonClick()
    {
        if (this.source != null)
        {
            this.addToDeck?.Invoke(this.source.CardDefId);
        }
    }

    public void OnRemoveFromDeckButtonClick()
    {
        if (this.source != null)
        {
            this.removeFromDeck?.Invoke(this.source.CardDefId);
        }
    }

    public void OnDisplayBigDetailButtonClick()
    {
        if (this.source != null)
        {
            this.displayBigDetail?.Invoke(this.source.CardDefId);
        }
    }
}
