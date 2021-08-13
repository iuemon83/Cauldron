using Assets.Scripts.ServerShared.MessagePackObjects;

namespace Cauldron.Shared
{
    public enum OutZonePrettyName
    {
        [DisplayText("なし")]
        None,
        [DisplayText("カードプール")]
        CardPool,
        [DisplayText("あなたの除外")]
        YouExcluded,
        [DisplayText("相手の除外")]
        OpponentExcluded,
        [DisplayText("持ち主の除外")]
        OwnerExcluded,
    }
}
