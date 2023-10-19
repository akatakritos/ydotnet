using YDotNet.Document.Cells;
using YDotNet.Document.Events;
using YDotNet.Document.StickyIndexes;
using YDotNet.Document.Transactions;
using YDotNet.Document.Types.Branches;
using YDotNet.Document.Types.Texts.Events;
using YDotNet.Infrastructure;
using YDotNet.Native.StickyIndex;
using YDotNet.Native.Types;
using YDotNet.Native.Types.Texts;

namespace YDotNet.Document.Types.Texts;

/// <summary>
///     A shared data type that represents a text string.
/// </summary>
public class Text : Branch
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Text" /> class.
    /// </summary>
    /// <param name="handle">The handle to the native resource.</param>
    internal Text(nint handle)
        : base(handle)
    {
        // Nothing here.
    }

    /// <summary>
    ///     Inserts a string in the given <c>index</c>.
    /// </summary>
    /// <param name="transaction">The transaction that wraps this write operation.</param>
    /// <param name="index">The index must be between 0 and <see cref="Length" /> or an exception will be thrown.</param>
    /// <param name="value">The text to be inserted.</param>
    /// <param name="attributes">
    ///     Optional, the attributes to be added to the inserted text. The value must be the result of a call to
    ///     <see cref="Input" />.<see cref="Input.Object" />.
    /// </param>
    public void Insert(Transaction transaction, uint index, string value, Input? attributes = null)
    {
        var valueHandle = MemoryWriter.WriteUtf8String(value);
        MemoryWriter.TryToWriteStruct(attributes?.InputNative, out var attributesHandle);

        TextChannel.Insert(Handle, transaction.Handle, index, valueHandle, attributesHandle);

        MemoryWriter.TryRelease(attributesHandle);
        MemoryWriter.Release(valueHandle);
    }

    /// <summary>
    ///     Inserts a content in the given <c>index</c>.
    /// </summary>
    /// <param name="transaction">The transaction that wraps this write operation.</param>
    /// <param name="index">The index must be between 0 and <see cref="Length" /> or an exception will be thrown.</param>
    /// <param name="content">The text to be inserted.</param>
    /// <param name="attributes">
    ///     Optional, the attributes to be added to the inserted text. The value must be the result of a call to
    ///     <see cref="Input" />.<see cref="Input.Object" />.
    /// </param>
    public void InsertEmbed(Transaction transaction, uint index, Input content, Input? attributes = null)
    {
        MemoryWriter.TryToWriteStruct(attributes?.InputNative, out var attributesPointer);
        MemoryWriter.TryToWriteStruct(content.InputNative, out var contentPointer);
        TextChannel.InsertEmbed(Handle, transaction.Handle, index, contentPointer, attributesPointer);
        MemoryWriter.TryRelease(attributesPointer);
        MemoryWriter.TryRelease(contentPointer);
    }

    /// <summary>
    ///     Removes a range of characters.
    /// </summary>
    /// <param name="transaction">The transaction that wraps this write operation.</param>
    /// <param name="index">The index must be between 0 and <see cref="Length" /> or an exception will be thrown.</param>
    /// <param name="length">
    ///     The length of the text to be removed, relative to the index and must not go over total
    ///     <see cref="Length" />.
    /// </param>
    public void RemoveRange(Transaction transaction, uint index, uint length)
    {
        TextChannel.RemoveRange(Handle, transaction.Handle, index, length);
    }

    /// <summary>
    ///     Formats a string in the given range defined by <c>index</c> and <c>length</c>.
    /// </summary>
    /// <param name="transaction">The transaction that wraps this write operation.</param>
    /// <param name="index">The index must be between 0 and <see cref="Length" /> or an exception will be thrown.</param>
    /// <param name="length">
    ///     The length of the text to be formatted, relative to the index and must not go over total
    ///     <see cref="Length" />.
    /// </param>
    /// <param name="attributes">
    ///     Optional, the attributes to be added to the inserted text. The value must be the result of a call to
    ///     <see cref="Input" />.<see cref="Input.Object" />.
    /// </param>
    public void Format(Transaction transaction, uint index, uint length, Input attributes)
    {
        var attributesPointer = MemoryWriter.WriteStruct(attributes.InputNative);
        TextChannel.Format(Handle, transaction.Handle, index, length, attributesPointer);
        MemoryWriter.Release(attributesPointer);
    }

    /// <summary>
    ///     Returns the <see cref="TextChunks" /> that compose this <see cref="Text" />.
    /// </summary>
    /// <param name="transaction">The transaction that wraps this read operation.</param>
    /// <returns>The <see cref="TextChunks" /> that compose this <see cref="Text" />.</returns>
    public TextChunks Chunks(Transaction transaction)
    {
        var handle = TextChannel.Chunks(Handle, transaction.Handle, out var length);

        return new TextChunks(handle, length);
    }

    /// <summary>
    ///     Returns the full string stored in the instance.
    /// </summary>
    /// <param name="transaction">The transaction that wraps this read operation.</param>
    /// <returns>The full string stored in the instance.</returns>
    public string String(Transaction transaction)
    {
        // Get the string pointer and read it into a managed instance.
        var handle = TextChannel.String(Handle, transaction.Handle);
        var result = MemoryReader.ReadUtf8String(handle);

        // Dispose the resources used by the underlying string.
        StringChannel.Destroy(handle);

        return result;
    }

    /// <summary>
    ///     Returns the length of the string, in bytes, stored in the instance.
    /// </summary>
    /// <remarks>
    ///     The returned value doesn't include the null terminator.
    /// </remarks>
    /// <param name="transaction">The transaction that wraps this read operation.</param>
    /// <returns>The length, in bytes, of the string stored in the instance.</returns>
    public uint Length(Transaction transaction)
    {
        return TextChannel.Length(Handle, transaction.Handle);
    }

    /// <summary>
    ///     Subscribes a callback function for changes performed within <see cref="Text" /> scope.
    /// </summary>
    /// <param name="action">The callback to be executed when a <see cref="Transaction" /> is committed.</param>
    /// <returns>The subscription for the event. It may be used to unsubscribe later.</returns>
    public EventSubscription Observe(Action<TextEvent> action)
    {
        TextChannel.ObserveCallback callback = (_, eventHandle) => action(new TextEvent(eventHandle));

        var subscriptionId = TextChannel.Observe(Handle, nint.Zero, callback);

        return new EventSubscription(subscriptionId, callback);
    }

    /// <summary>
    ///     Unsubscribes a callback function, represented by an <see cref="EventSubscription" /> instance, for changes
    ///     performed within <see cref="Text" /> scope.
    /// </summary>
    /// <param name="subscription">The subscription that represents the callback function to be unobserved.</param>
    public void Unobserve(EventSubscription subscription)
    {
        TextChannel.Unobserve(Handle, subscription.Id);
    }

    /// <summary>
    ///     Retrieves a <see cref="StickyIndex" /> corresponding to a given human-readable <see cref="index" /> pointing into
    ///     the <see cref="Branch" />.
    /// </summary>
    /// <param name="transaction">The transaction that wraps this operation.</param>
    /// <param name="index">The numeric index to place the <see cref="StickyIndex" />.</param>
    /// <param name="associationType">The type of the <see cref="StickyIndex" />.</param>
    /// <returns>The <see cref="StickyIndex" /> in the <see cref="index" /> with the given <see cref="associationType" />.</returns>
    public StickyIndex? StickyIndex(Transaction transaction, uint index, StickyAssociationType associationType)
    {
        return ReferenceAccessor.Access(
            new StickyIndex(StickyIndexChannel.FromIndex(Handle, transaction.Handle, index, (sbyte) associationType)));
    }
}
