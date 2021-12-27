using Assets.Scripts.ServerShared.MessagePackObjects;

namespace Cauldron.Shared
{
    public enum ZonePrettyName
    {
        [DisplayText("なし")]
        None,
        [DisplayText("あなたの場")]
        YouField,
        [DisplayText("相手の場")]
        OpponentField,
        [DisplayText("持ち主の場")]
        OwnerField,
        [DisplayText("あなたの手札")]
        YouHand,
        [DisplayText("相手の手札")]
        OpponentHand,
        [DisplayText("持ち主の手札")]
        OwnerHand,
        [DisplayText("あなたのデッキ")]
        YouDeck,
        [DisplayText("相手のデッキ")]
        OpponentDeck,
        [DisplayText("持ち主のデッキ")]
        OwnerDeck,
        [DisplayText("あなたの墓地")]
        YouCemetery,
        [DisplayText("相手の墓地")]
        OpponentCemetery,
        [DisplayText("持ち主の墓地")]
        OwnerCemetery,

        [DisplayText("一時領域")]
        Temporary,

        [DisplayText("いずれか")]
        Any,
    }
}
