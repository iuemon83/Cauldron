using System.Collections.Generic;

namespace Assets.Scripts
{
    public interface IDeck
    {
        public string Id { get; }
        public string Name { get; }
        public IReadOnlyList<string> PackNames { get; }
        public IReadOnlyList<string> CardDefNames { get; }
    }
}
