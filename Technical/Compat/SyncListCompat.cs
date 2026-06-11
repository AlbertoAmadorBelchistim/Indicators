#if ATAS_BETA || ATAS_LATEST || ATAS_STABLE
using System;
using System.Collections.Generic;

using Utils.Common.Collections.Synchronized;

/// <summary>
/// Compat extensions for SyncList&lt;T&gt; in Beta/Latest builds (8.0.13 DLLs) and Stable v7, which lack the
/// RemoveRange/SyncGet/capacity-constructor APIs introduced upstream by PLAT-3738.
/// Remove this file once Beta/Latest/Stable ship a Utils.Common with these members.
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
