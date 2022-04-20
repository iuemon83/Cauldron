using Assets.Scripts.ServerShared.MessagePackObjects;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    /// <summary>
    /// カード破壊アクションのコンテキスト（カード型）
    /// </summary>
    [MessagePackObject(true)]
    public class ActionContextCardsOfDestroyCard
    {
        public enum TypeValue
        {
            [DisplayText("破壊されたカード")]
            DestroyedCards
        }

        public string ActionName { get; }

        public TypeValue Type { get; }

        public ActionContextCardsOfDestroyCard(
            string ActionName,
            TypeValue Type
            )
        {
            this.ActionName = ActionName;
            this.Type = Type;
        }
    }
}
