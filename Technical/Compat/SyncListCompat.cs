#if ATAS_STABLE
using System;
using System.Collections.Generic;

using Utils.Common.Collections.Synchronized;

/// <summary>
/// Compat extensions for SyncList&lt;T&gt; in Stable v7, which lacks the
/// RemoveRange/SyncGet/capacity-constructor APIs introduced upstream by PLAT-3738.
/// Beta/Latest ship these members since the 2026-09 builds. Remove this file once Stable does.
/// </summary>
internal static class SyncListCompat
{
    /// <summary>
    /// Removes <paramref name="count"/> elements starting at <paramref name="index"/>.
    /// </summary>
    public static void RemoveRange<T>(this SyncList<T> list, int index, int count)
    {
        if (list == null)
            throw new ArgumentNullException(nameof(list));

        for (var i = 0; i < count; i++)
            list.RemoveAt(index);
    }

    /// <summary>
    /// Approximation of SyncList&lt;T&gt;.SyncGet: the 8.0.13 SyncList does not expose its
    /// internal lock, so the callback runs against a snapshot copied through the list's
    /// synchronized enumerator instead of inside the lock.
    /// </summary>
    public static TResult SyncGet<T, TArg, TResult>(this SyncList<T> list, Func<List<T>, TArg, TResult> func, TArg arg)
    {
        if (list == null)
            throw new ArgumentNullException(nameof(list));

        return func(new List<T>(list), arg);
    }
}
#endif
