using DotnetPrompt.Tools;
using NUnit.Framework;

namespace DotnetPrompt.Tests.Unit.Tools;

[TestFixture]
public class StringHelpersTests
{
    [Test]
    public void SplitStringIntoChunks_WithEmptyString_EmptyResults()
    {
        // Arrange
        var input = "";
        var expectedChunks = new List<string>();

        // Act
        var actualChunks = StringHelpers.SplitStringIntoChunks(input, 10);

        // Assert
        CollectionAssert.IsEmpty(actualChunks);
    }

    [Test]
    public void SplitStringIntoChunks_WithShortString_SingleChunk()
    {
        // Arrange
        var input = "This is a short string.";
        var expectedChunks = new List<string> { input };

        // Act
        var actualChunks = StringHelpers.SplitStringIntoChunks(input, 100);

        // Assert
        Assert.AreEqual(expectedChunks, actualChunks);
    }

    [Test]
    public void SplitStringIntoChunks_WithLongString_SeveralChunks()
    {
        // Arrange
        var input = "This is a longer string. It has more words and needs to be split into several chunks. The chunks should end on a punctuation mark or a new line.";
        var expectedChunks = new List<string> { "This is a longer string.", "It has more words and needs to be split into several chunks.", 
            "The chunks should end on a punctuation mark or a new line." };

        // Act
        var actualChunks = StringHelpers.SplitStringIntoChunks(input, 30);

        // Assert
        Assert.AreEqual(expectedChunks, actualChunks);
    }

    [Test]
    public void SplitStringIntoChunks_WithConsecutiveSpacesAndNewLines_SeveralChunks()
    {
        // Arrange
        var input = "This is\na    string\n  with multiple\n   spaces.";
        var expectedChunks = new List<string> { "This is", "a    string", "with multiple", "spaces." };

        // Act
        var actualChunks = StringHelpers.SplitStringIntoChunks(input, 10);

        // Assert
        Assert.AreEqual(expectedChunks, actualChunks);
    }
}
