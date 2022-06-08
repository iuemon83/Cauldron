using MessagePack;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class ModifyPlayerContext
    {
        public PlayerId PlayerId { get; }
        public int DiffMaxHp { get; }
        public int DiffCurrentHp { get; }
        public int DiffMaxMp { get; }
        public int DiffCurrentMp { get; }

        public ModifyPlayerContext(
            PlayerId playerId,
            int DiffMaxHp,
            int DiffHp,
            int DiffMaxMp,
            int DiffMp
            )
        {
            this.PlayerId = playerId;
            this.DiffMaxHp = DiffMaxHp;
            this.DiffCurrentHp = DiffHp;
            this.DiffMaxMp = DiffMaxMp;
            this.DiffCurrentMp = DiffMp;
        }
    }
}
