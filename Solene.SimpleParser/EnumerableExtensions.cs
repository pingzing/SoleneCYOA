﻿using System.Collections.Generic;

namespace Solene.SimpleParser
{
    internal static class EnumerableExtensions
    {
        internal static IEnumerable<T> SkipLastN<T>(this IEnumerable<T> source, int n)
        {
            var it = source.GetEnumerator();
            bool hasRemainingItems = false;
            var cache = new Queue<T>(n + 1);
            do
            {
                if (hasRemainingItems = it.MoveNext())
                {
                    cache.Enqueue(it.Current);
                    if (cache.Count > n)
                    {
                        yield return cache.Dequeue();
                    }
                }
            } while (hasRemainingItems);
        }
    }
}
