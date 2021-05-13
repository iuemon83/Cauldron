using MessagePack;
using System;

namespace Cauldron.Shared.MessagePackObjects
{
    [MessagePackObject(true)]
    public class AskMessage
    {
        public Guid QuestionId { get; set; }
        public ChoiceCandidates ChoiceCandidates { get; set; }
        public int NumPicks { get; set; }
    }
}
