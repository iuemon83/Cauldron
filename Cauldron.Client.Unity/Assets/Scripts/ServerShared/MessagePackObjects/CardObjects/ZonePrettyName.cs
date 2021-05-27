using Assets.Scripts.ServerShared.MessagePackObjects;

namespace Cauldron.Shared
{
    public enum ZonePrettyName
    {
        [DisplayText("なし")]
        None,
        [DisplayText("カードプール")]
        CardPool,
        [DisplayText("あなたの場")]
        YouField,
        [DisplayText("相手の場")]
        OpponentField,
        [DisplayText("あなたの手札")]
        YouHand,
        [DisplayText("相手の手札")]
        OpponentHand,
        [DisplayText("あなたのデッキ")]
        YouDeck,
        [DisplayText("相手のデッキ")]
        OpponentDeck,
        [DisplayText("あなたの墓地")]
        YouCemetery,
        [DisplayText("相手の墓地")]
        OpponentCemetery,
    }
}
