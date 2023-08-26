using System.Collections;
using YDotNet.Document.Cells;
using YDotNet.Native.Types;

namespace YDotNet.Document.Types.XmlElements.Trees;

/// <summary>
///     Represents the tree walker to provide instances of <see cref="Output" /> or
///     <c>null</c> to <see cref="XmlTreeWalker" />.
/// </summary>
internal class XmlTreeWalkerEnumerator : IEnumerator<Output>
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="XmlTreeWalkerEnumerator" /> class.
    /// </summary>
    /// <param name="treeWalker">
    ///     The <see cref="TreeWalker" /> instance used by this enumerator.
    ///     Check <see cref="TreeWalker" /> for more details.
    /// </param>
    public XmlTreeWalkerEnumerator(XmlTreeWalker treeWalker)
    {
        TreeWalker = treeWalker;
        Current = null;
    }

    /// <summary>
    ///     Gets the <see cref="TreeWalker" /> instance that holds the
    ///     <see cref="XmlTreeWalker.Handle" /> used by this enumerator.
    /// </summary>
    private XmlTreeWalker TreeWalker { get; }

    /// <inheritdoc />
    public Output? Current { get; private set; }

    /// <inheritdoc />
    object? IEnumerator.Current => Current;

    /// <inheritdoc />
    public bool MoveNext()
    {
        var handle = XmlElementChannel.TreeWalkerNext(TreeWalker.Handle);

        Current = handle != nint.Zero ? new Output(handle) : null;

        return Current != null;
    }

    /// <inheritdoc />
    public void Reset()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        TreeWalker.Dispose();
    }
}