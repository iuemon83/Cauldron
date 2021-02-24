using System;
using System.Collections.Generic;
using System.Linq;

namespace Cauldron.Core
{
    public static class RandomUtil
    {
        public static readonly Random Random = new Random();
        public static T RandomPick<T>(IReadOnlyList<T> source) => source.Any() ? source[RandomUtil.Random.Next(source.Count)] : default;
        public static IEnumerable<T> RandomPick<T>(IReadOnlyList<T> source, int picksNum)
        {
            if (source.Count <= picksNum) return source;

            var indexList = Enumerable.Range(0, source.Count).ToList();
            var pickedList = new List<T>();
            for (var i = 0; i < picksNum; i++)
            {
                var pickedIndex = RandomPick(indexList);
                pickedList.Add(source[pickedIndex]);
                indexList.Remove(pickedIndex);
            }

            return pickedList;
        }
    }
}
