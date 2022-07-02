using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class ModifyCardContext
    {
        public CardId CardId { get; }
        public int DiffPower { get; }
        public int DiffToughness { get; }

        public ModifyCardContext(
            CardId CardId,
            int DiffPower,
            int DiffToughness
            )
        {
            this.CardId = CardId;
            this.DiffPower = DiffPower;
            this.DiffToughness = DiffToughness;
        }
    }
}
