using Assets.Scripts.ServerShared.MessagePackObjects;
using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    /// <summary>
    /// プレイヤー選択アクションのコンテキスト（プレイヤー型）
    /// </summary>
    [MessagePackObject(true)]
    public class ActionContextPlayersOfChoice
    {
        public enum TypeValue
        {
            [DisplayText("選択されたプレイヤー")]
            ChoicePlayers
        }

        public string ActionName { get; }
        public TypeValue Type { get; }

        public ActionContextPlayersOfChoice(
            string ActionName,
            TypeValue Type)
        {
            this.ActionName = ActionName;
            this.Type = Type;
        }
    }
}
