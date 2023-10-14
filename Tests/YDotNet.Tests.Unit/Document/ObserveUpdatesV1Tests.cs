using NUnit.Framework;
using YDotNet.Document;

namespace YDotNet.Tests.Unit.Document;

public class ObserveUpdatesV1
{
    [Test]
    public void TriggersWhenTransactionIsCommittedUntilUnobserve()
    {
        // Arrange
        var doc = new Doc();

        byte[]? data = null;
        var called = 0;

        var text = doc.Text("country");
        var subscription = doc.ObserveUpdatesV1(
            e =>
            {
                called++;
                data = e.Update;
            });

        // Act
        var transaction = doc.WriteTransaction();
        text.Insert(transaction, index: 0, "Brazil");
        transaction.Commit();

        // Assert
        Assert.That(called, Is.EqualTo(expected: 1));
        Assert.That(data, Is.Not.Null);
        Assert.That(data, Has.Length.InRange(from: 25, to: 30));

        // Act
        data = null;
        transaction = doc.WriteTransaction();
        text.Insert(transaction, index: 0, "Great ");
        transaction.Commit();

        // Assert
        Assert.That(called, Is.EqualTo(expected: 2));
        Assert.That(data, Is.Not.Null);
        Assert.That(data, Has.Length.InRange(from: 23, to: 31));

        // Act
        data = null;
        doc.UnobserveUpdatesV1(subscription);

        transaction = doc.WriteTransaction();
        text.Insert(transaction, index: 0, "The ");
        transaction.Commit();

        // Assert
        Assert.That(called, Is.EqualTo(expected: 2));
        Assert.That(data, Is.Null);
    }
}
