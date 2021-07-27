using MessagePack;
using System;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class AskMessage
    {
        public Guid QuestionId { get; }
        public ChoiceCandidates ChoiceCandidates { get; }
        public int NumPicks { get; }

        public AskMessage(
            Guid QuestionId,
            ChoiceCandidates ChoiceCandidates,
            int NumPicks
            )
        {
            this.QuestionId = QuestionId;
            this.ChoiceCandidates = ChoiceCandidates;
            this.NumPicks = NumPicks;
        }
    }
}
