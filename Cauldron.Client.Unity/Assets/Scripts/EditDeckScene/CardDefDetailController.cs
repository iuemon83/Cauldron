using Assets.Scripts;
using Cauldron.Shared;
using Cauldron.Shared.MessagePackObjects;
using System;
using System.Linq;
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
    private TextMeshProUGUI costText = default;
    [SerializeField]
    private TextMeshProUGUI powerText = default;
    [SerializeField]
    private TextMeshProUGUI toughnessText = default;
    [SerializeField]
    private TextMeshProUGUI otherText = default;

    protected CardDef source;
    private bool requireUpdate;

    private void Start()
    {
        this.cardNameText.text = "";
        this.effectText.text = "";
        this.costText.text = "";
        this.otherText.text = "";

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
        this.effectText.text = this.EffectDescription(this.source);
        this.costText.text = this.source.Cost.ToString();
        this.otherText.text = this.OtherText(this.source);

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

    private string EffectDescription(CardDef cardDef)
    {
        return string.Join(Environment.NewLine,
            new[]
            {
                cardDef.IsToken ? "<color=\"red\">トークン</color>" : "",
                Utility.DisplayTextForNumAttacksLimitInTurn(cardDef.NumAttacksLimitInTurn.Value),
                Utility.DisplayTextForNumTurnsToCanAttack(cardDef),
                cardDef.EffectDescription
            }
            .Where(x => !string.IsNullOrWhiteSpace(x))
            );
    }

    private string OtherText(CardDef cardDef)
    {
        var annnotationsText = cardDef.Annotations.Count == 0
            ? "なし"
            : string.Join(",", cardDef.Annotations);

        var abilitiesText = cardDef.Abilities.Count == 0
            ? "なし"
            : string.Join(",", cardDef.Abilities.Select(Utility.DisplayText));

        var result =
$@"{Utility.DisplayText(cardDef.Type)}
{annnotationsText}
{abilitiesText}";

        if (cardDef.Type == CardType.Creature)
        {
            result += Environment.NewLine +
$@"攻撃回数 | {cardDef.NumAttacksLimitInTurn}
攻撃可能までのターン
  → クリーチャー | {cardDef.NumTurnsToCanAttackToCreature}
  → プレイヤー | {cardDef.NumTurnsToCanAttackToPlayer}";
        }

        return result;
    }

    public void SetCard(CardDef cardDef)
    {
        this.source = cardDef;
        this.requireUpdate = true;
    }
}
