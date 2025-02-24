using System.Text.Json.Serialization;
using YDotNet.Document.Types.Events;
using YDotNet.Extensions;
using YDotNet.Server;

namespace Demo;

public sealed class Callback : IDocumentCallback
{
    private readonly ILogger<Callback> log;

    public Callback(ILogger<Callback> log)
    {
        this.log = log;
    }

    public ValueTask OnDocumentLoadedAsync(DocumentLoadEvent @event)
    {
        if (@event.Context.DocumentName == "notifications")
        {
            return default;
        }

        var map = @event.Document.Map("increment");

        map?.ObserveDeep(changes =>
        {
            foreach (var change in changes)
            {
                var key = change.MapEvent?.Keys.FirstOrDefault(x => x.Key == "value" && x.Tag != EventKeyChangeTag.Remove);

                if (key != null)
                {
                    var valueOld = key.OldValue?.Double;
                    var valueNew = key.NewValue?.Double;

                    if (valueOld == valueNew)
                    {
                        continue;
                    }

                    log.LogInformation("Counter updated from {oldValue} to {newValue}.", valueOld, valueNew);
                }
            }
        });

        var chat = @event.Document.Array("stream");

        chat?.ObserveDeep(async changes =>
        {
            var newNotificationsRaw =
                changes
                    .Where(x => x.Tag == EventBranchTag.Array)
                    .Select(x => x.ArrayEvent)
                    .SelectMany(x => x.Delta.Where(x => x.Tag == EventChangeTag.Add))
                    .SelectMany(x => x.Values)
                    .ToArray();

            if (newNotificationsRaw.Length == 0)
            {
                return;
            }

            await Task.Delay(100);

            var notificationCtx = new DocumentContext("notifications", 0);

            await @event.Source.UpdateDocAsync(notificationCtx, (doc) =>
            {
                List<Notification> notifications;

                using (var transaction = @event.Document.ReadTransaction())
                {
                    notifications = newNotificationsRaw.Select(x => x.To<Notification>(transaction)).ToList();
                }

                var array = doc.Array("stream");

                notifications = notifications.Select(x => new Notification { Text = $"You got the follow message: {x.Text}" }).ToList();

                using (var transaction = doc.WriteTransaction() ?? throw new InvalidOperationException("Failed to open transaction."))
                {
                    array.InsertRange(transaction, array.Length, notifications.Select(x => x.ToInput()).ToArray());
                }
            });
        });


        return default;
    }

    public sealed class Notification
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }
}
