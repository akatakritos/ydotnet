using YDotNet.Document.Events;
using YDotNet.Document.Options;
using YDotNet.Document.Transactions;
using YDotNet.Document.Types;
using YDotNet.Native.Document;
using YDotNet.Native.Document.Events;

namespace YDotNet.Document;

/// <summary>
///     A Yrs document type.
/// </summary>
/// <remarks>
///     <para>
///         The documents are the most important units of collaborative resources management.
///         All shared collections live within the scope of their corresponding documents. All updates are
///         generated on per document basis (rather than generated per shared type basis). All operations on
///         shared collections happen via <see cref="Transaction" />, whose lifetime is also bound to a document.
///     </para>
///     <para>
///         Document manages the so-called root types, which are top-level shared types definitions (as opposed
///         to recursively nested types).
///     </para>
/// </remarks>
public class Doc : IDisposable
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="Doc" /> class.
    /// </summary>
    public Doc()
    {
        Handle = DocChannel.New();
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="Doc" /> class with the specified <paramref name="options" />.
    /// </summary>
    /// <param name="options">The options to be used when initializing this document.</param>
    public Doc(DocOptions options)
    {
        if (options == null)
        {
            throw new ArgumentNullException(nameof(options));
        }

        Handle = DocChannel.NewWithOptions(DocOptionsNative.From(options));
    }

    /// <summary>
    ///     Gets the unique client identifier of this <see cref="Doc" /> instance.
    /// </summary>
    public ulong Id => DocChannel.Id(Handle);

    /// <summary>
    ///     Gets the unique document identifier of this <see cref="Doc" /> instance.
    /// </summary>
    // TODO [LSViana] Check if this should be of type `Guid`.
    // There's a complication due to string format losing zero to the left in hexadecimal representation.
    public string Guid => DocChannel.Guid(Handle);

    /// <summary>
    ///     Gets the collection identifier of this <see cref="Doc" /> instance.
    /// </summary>
    /// <remarks>
    ///     If none was defined, a <c>null</c> will be returned.
    /// </remarks>
    public string? CollectionId => DocChannel.CollectionId(Handle);

    /// <summary>
    ///     Gets a value indicating whether this <see cref="Doc" /> instance requested a data load.
    /// </summary>
    /// <remarks>
    ///     This flag is often used by the parent <see cref="Doc" /> instance to check if this <see cref="Doc" />
    ///     instance requested a data load.
    /// </remarks>
    public bool ShouldLoad => DocChannel.ShouldLoad(Handle);

    /// <summary>
    ///     Gets a value indicating whether this <see cref="Doc" /> instance will auto load.
    /// </summary>
    /// <remarks>
    ///     Auto loaded nested <see cref="Doc" /> instances automatically send a load request to their parent
    ///     <see cref="Doc" /> instances.
    /// </remarks>
    public bool AutoLoad => DocChannel.AutoLoad(Handle);

    /// <summary>
    ///     Gets the handle to the native resource.
    /// </summary>
    internal nint Handle { get; private set; }

    /// <inheritdoc />
    public void Dispose()
    {
        DocChannel.Destroy(Handle);

        Handle = default;
    }

    /// <summary>
    ///     Gets or creates a new shared <see cref="Text" /> data type instance as a root-level type of a given document.
    /// </summary>
    /// <remarks>
    ///     This structure can later be accessed using its <c>name</c>.
    /// </remarks>
    /// <param name="name">The name of the <see cref="Text" /> instance to get.</param>
    /// <returns>The <see cref="Text" /> instance related to the <c>name</c> provided.</returns>
    public Text Text(string name)
    {
        return new Text(DocChannel.Text(Handle, name));
    }

    /// <summary>
    ///     Starts a new read-write transaction on a given document.
    /// </summary>
    /// <returns>The transaction to perform operations in the document.</returns>
    public Transaction WriteTransaction()
    {
        return new Transaction(DocChannel.WriteTransaction(Handle, originLength: 0, nint.Zero));
    }

    /// <summary>
    ///     Subscribes callback function for changes performed within transaction scope.
    /// </summary>
    /// <remarks>
    ///     The updates are encoded using <c>lib0</c> V1 encoding and they can be  passed to remote peers right away.
    /// </remarks>
    /// <param name="action">The callback to be executed when a transaction is committed.</param>
    /// <returns>The subscription for event. It may be used to unsubscribe later.</returns>
    public EventSubscription ObserveUpdatesV1(Action<UpdateEvent> action)
    {
        var subscriptionId = DocChannel.ObserveUpdatesV1(
            Handle,
            nint.Zero,
            (state, length, data) => action(UpdateEvent.From(length, data)));

        return new EventSubscription(subscriptionId);
    }

    /// <summary>
    ///     Unsubscribes a callback function, represented by an <see cref="EventSubscription" /> instance, for changes
    ///     performed within transaction scope.
    /// </summary>
    /// <param name="subscription">The subscription that represents the callback function to be unobserved.</param>
    public void UnobserveUpdatesV1(EventSubscription subscription)
    {
        DocChannel.UnobserveUpdatesV1(Handle, subscription.Id);
    }

    /// <summary>
    ///     Subscribes callback function for changes performed within transaction scope.
    /// </summary>
    /// <remarks>
    ///     The updates are encoded using <c>lib0</c> V1 encoding and they can be  passed to remote peers right away.
    /// </remarks>
    /// <param name="action">The callback to be executed when a transaction is committed.</param>
    /// <returns>The subscription for event. It may be used to unsubscribe later.</returns>
    public EventSubscription ObserveUpdatesV2(Action<UpdateEvent> action)
    {
        var subscriptionId = DocChannel.ObserveUpdatesV2(
            Handle,
            nint.Zero,
            (state, length, data) => action(UpdateEvent.From(length, data)));

        return new EventSubscription(subscriptionId);
    }

    /// <summary>
    ///     Unsubscribes a callback function, represented by an <see cref="EventSubscription" /> instance, for changes
    ///     performed within transaction scope.
    /// </summary>
    /// <param name="subscription">The subscription that represents the callback function to be unobserved.</param>
    public void UnobserveUpdatesV2(EventSubscription subscription)
    {
        DocChannel.UnobserveUpdatesV2(Handle, subscription.Id);
    }

    /// <summary>
    ///     Subscribes callback function for changes performed within transaction scope.
    /// </summary>
    /// <remarks>
    ///     The updates are encoded using <c>lib0</c> V1 encoding and they can be  passed to remote peers right away.
    /// </remarks>
    /// <param name="action">The callback to be executed when a transaction is committed.</param>
    /// <returns>The subscription for event. It may be used to unsubscribe later.</returns>
    public EventSubscription ObserveAfterTransaction(Action<AfterTransactionEvent> action)
    {
        var subscriptionId = DocChannel.ObserveAfterTransaction(
            Handle,
            nint.Zero,
            (state, afterTransactionEvent) => action(afterTransactionEvent.ToAfterTransactionEvent()));

        return new EventSubscription(subscriptionId);
    }

    /// <summary>
    ///     Unsubscribes a callback function, represented by an <see cref="EventSubscription" /> instance, for changes
    ///     performed within transaction scope.
    /// </summary>
    /// <param name="subscription">The subscription that represents the callback function to be unobserved.</param>
    public void UnobserveAfterTransaction(EventSubscription subscription)
    {
        DocChannel.UnobserveAfterTransaction(Handle, subscription.Id);
    }
}
