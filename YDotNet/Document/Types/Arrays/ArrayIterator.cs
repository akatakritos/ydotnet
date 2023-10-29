using System.Collections;
using YDotNet.Document.Cells;
using YDotNet.Infrastructure;
using YDotNet.Native.Types;

namespace YDotNet.Document.Types.Arrays;

/// <summary>
///     Represents an iterator, which can be used to traverse over all elements of an <see cref="Array" />.
/// </summary>
/// <remarks>
///     The iterator can't be reused. If needed, use <see cref="Enumerable.ToArray{TSource}" /> to accumulate values.
/// </remarks>
public class ArrayIterator : UnmanagedResource, IEnumerable<Output>
{
    internal ArrayIterator(nint handle, Doc doc)
        : base(handle)
    {
        Doc = doc;
    }

    internal Doc Doc { get; }

    /// <inheritdoc />
    public IEnumerator<Output> GetEnumerator()
    {
        ThrowIfDisposed();
        return new ArrayEnumerator(this);
    }

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    /// <summary>
    ///     Finalizes an instance of the <see cref="ArrayIterator" /> class.
    /// </summary>
    ~ArrayIterator()
    {
        Dispose(disposing: false);
    }

    /// <inheritdoc />
    protected override void DisposeCore(bool disposing)
    {
        ArrayChannel.IteratorDestroy(Handle);
    }
}
