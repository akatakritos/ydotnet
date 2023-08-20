using NUnit.Framework;
using YDotNet.Document;

namespace YDotNet.Tests.Unit.XmlTexts;

public class UnobserveDeepTests
{
    [Test]
    public void TriggersWhenChangedUntilUnobserved()
    {
        // Arrange
        var doc = new Doc();
        var xmlText = doc.XmlText("xml-text");
        var called = 0;
        var subscription = xmlText.ObserveDeep(_ => called++);

        // Act
        var transaction = doc.WriteTransaction();
        xmlText.Insert(transaction, index: 0, "World");
        transaction.Commit();

        // Assert
        Assert.That(called, Is.EqualTo(expected: 1));

        // Act
        xmlText.UnobserveDeep(subscription);

        transaction = doc.WriteTransaction();
        xmlText.Insert(transaction, index: 0, "Hello, ");
        transaction.Commit();

        // Assert
        Assert.That(called, Is.EqualTo(expected: 1));
    }
}
