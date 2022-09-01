using System;
using System.Collections.Generic;
using System.Linq;

public static class IEnumerableExtensions
{
    #region Methods

    public static T Random<T>(this IEnumerable<T> collection)
    {
        var random  = new Random(Randomizer.Seed);
        var index   = random.Next(0, collection.Count());

        return collection.ElementAt(index);
    }

    #endregion
}