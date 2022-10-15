using Assets.Scripts;
using Cauldron.Shared.MessagePackObjects;
using System;
using TMPro;
using UnityEngine;

public class CardDefDetailController : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI cardNameText = default;
    [SerializeField]
    private TextMeshProUGUI effectText = default;

    [SerializeField]
    private GameObject powerSpace = default;
    [SerializeField]
    private GameObject toughnessSpace = default;

    [SerializeField]
    private TextMeshProUGUI cardTypeText = default;
    [SerializeField]
    private TextMeshProUGUI costText = default;
    [SerializeField]
    private TextMeshProUGUI powerText = default;
    [SerializeField]
    private TextMeshProUGUI toughnessText = default;

    private CardBridge source;
    private int cardPoolIndex;

    private bool requireUpdate;

    private Action<CardDefId> addToDeck;
    private Action<CardDefId> removeFromDeck;
    private Action<CardDefId, int> displayFlaverTextViewer;

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
        Action<CardDefId, int> displayFlaverTextViewer
        )
    {
        this.addToDeck = addToDeck;
        this.removeFromDeck = removeFromDeck;
        this.displayFlaverTextViewer = displayFlaverTextViewer;
    }

    public void SetCard(CardDef cardDef, int cardPoolIndex)
    {
        this.source = new CardBridge(cardDef, default);
        this.cardPoolIndex = cardPoolIndex;
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
            this.displayFlaverTextViewer?.Invoke(this.source.CardDefId, this.cardPoolIndex);
        }
    }
}
