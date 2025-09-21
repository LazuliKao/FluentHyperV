using System;
using System.Collections.Generic;
using System.Linq;

namespace FluentHyperV.Desktop.SourceGenerator;

public static class EnumerableExtensions
{
    public static IEnumerable<T> DistinctBy<T, TO>(
        this IEnumerable<T> source,
        Func<T, TO> keySelector,
        IEqualityComparer<TO>? comparer = null
    )
    {
        return source.Distinct(new EnumerableExtensions<T, TO>(keySelector, comparer));
    }
}

file class EnumerableExtensions<T, TO>(
    Func<T, TO> keySelector,
    IEqualityComparer<TO>? comparer = null
) : IEqualityComparer<T>
{
    private readonly Func<T, TO> _keySelector =
        keySelector ?? throw new ArgumentNullException(nameof(keySelector));

    public bool Equals(T? x, T? y)
    {
        if (x is null && y is null)
            return true;
        if (x is null || y is null)
            return false;
        return comparer?.Equals(_keySelector(x), _keySelector(y))
            ?? EqualityComparer<TO>.Default.Equals(_keySelector(x), _keySelector(y));
    }

    public int GetHashCode(T obj)
    {
        return _keySelector(obj)?.GetHashCode() ?? 0;
    }
}
