using DotnetPrompt.Abstractions.LLM;
using NUnit.Framework;
using DotnetPrompt.LLM.OpenAI;
using DotnetPrompt.Prompts;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace DotnetPrompt.Tests.Integration;

public class ChatGptModelTests
{

    private BaseModel BuildModel()
    {
        return new ChatGptModel(Constants.OpenAIKey, ChatGptModelConfiguration.Default,
            TestLogger.Create<ChatGptModel>());
    }

    [Test]
    public async Task PromptAsync_SanityCheck_StringOutputNotEmpty()
    {
        // Arrange
        var llm = BuildModel();

        // Act
        var output = await llm.PromptAsync("Say foo:");

        // Assert
        Assert.IsNotEmpty(output);
    }

    [Test]
    public async Task PromptAsyncWithProperType_SanityCheck_StringOutputNotEmpty()
    {
        // Arrange
        var llm = BuildModel();
        var prompt = new ChatMLPromptTemplate("Say foo:");

        // Act
        var output = await llm.PromptAsync(prompt.Format());

        // Assert
        Assert.IsNotEmpty(output);
    }

    [Test]
    public async Task PromptAsync_WithNegativeMaxTokensAndSinglePrompt_StringOutputNotEmpty()
    {
        // Arrange
        var llm = new ChatGptModel(Constants.OpenAIKey,
            ChatGptModelConfiguration.Default with { MaxTokens = -1 },
            TestLogger.Create<OpenAIModel>());

        // Act
        var output = await llm.PromptAsync("Write me a essay on foo:");

        // Assert
        Assert.IsNotEmpty(output);
    }

    [Test]
    public async Task PromptAsync_WithCache_SameResult()
    {
        // Arrange
        var settings = new MemoryDistributedCacheOptions() { TrackStatistics = true };
        var appSettingsOptions = Options.Create(settings);
        var cache = new MemoryDistributedCache(appSettingsOptions);

        var llm = new ChatGptModel(Constants.OpenAIKey,
            ChatGptModelConfiguration.Default with { MaxTokens = -1 },
            TestLogger.Create<OpenAIModel>(),
            cache)
        {
            UseCache = true
        };

        // Act
        var output = await llm.PromptAsync("Say foo:");
        var output2 = await llm.PromptAsync("Say foo:");

        // Assert
        Assert.AreEqual(output, output2);
    }

    [Test]
    public async Task PromptAsync_WithCache_SecondComeWithoutLLMData()
    {
        // Arrange
        var settings = new MemoryDistributedCacheOptions() { TrackStatistics = true };
        var appSettingsOptions = Options.Create(settings);
        var cache = new MemoryDistributedCache(appSettingsOptions);

        var llm = new ChatGptModel(Constants.OpenAIKey,
            ChatGptModelConfiguration.Default with { MaxTokens = -1 },
            TestLogger.Create<OpenAIModel>(),
            cache)
        {
            UseCache = true
        };

        // Act
        var output = await llm.GenerateAsync(new List<string> {"Say foo:"});
        var output2 = await llm.GenerateAsync(new List<string> { "Say foo:"});

        // Assert
        Assert.AreEqual(output.Generations[0][0].Text, output2.Generations[0][0].Text);
        Assert.NotNull(output.Output);
        Assert.Null(output2.Output);
    }

    [Test]
    public async Task PromptAsync_WithStop_ValidConfiguration()
    {
        // Arrange
        var query = "write an ordered list of five items";

        var firstLlm = new ChatGptModel(Constants.OpenAIKey, ChatGptModelConfiguration.Default with { Temperature = 0 },
            TestLogger.Create<OpenAIModel>())
        {
            DefaultStop = new List<string> { "3" },
        };

        var secondLlm = new ChatGptModel(Constants.OpenAIKey, ChatGptModelConfiguration.Default with { Temperature = 0 },
            TestLogger.Create<OpenAIModel>());

        var thirdLlm = new ChatGptModel(Constants.OpenAIKey, ChatGptModelConfiguration.Default with { Stop = new List<string> { "3" }, Temperature = 0 },
            TestLogger.Create<OpenAIModel>());

        // Act
        var firstOutput = await firstLlm.PromptAsync(query);
        var secondOutput = await secondLlm.PromptAsync(query, stop: new List<string> { "3" });
        var thirdOutput = await thirdLlm.PromptAsync(query);

        // Assert
        Assert.AreEqual(firstOutput, secondOutput);
        Assert.AreEqual(firstOutput, thirdOutput);
        Assert.AreEqual(secondOutput, thirdOutput);
    }

    /// <summary>
    /// Test OpenAPI stop logic on bad configuration.
    /// </summary>
    [Test]
    public void Model_WithSeveralStops_InvalidOperationException()
    {
        // Arrange
        var llm = new ChatGptModel(Constants.OpenAIKey,
            ChatGptModelConfiguration.Default
                with
                {
                    Temperature = 0,
                    Stop = new[] { "3" }
                },
            TestLogger.Create<OpenAIModel>());

        // Assert
        Assert.ThrowsAsync<InvalidOperationException>(() => llm.PromptAsync("write an ordered list of five items", stop: new List<string> { "\n" }));
    }

    [Test]
    public void Model_WithSeveralStops2_InvalidOperationException()
    {
        // Arrange
        var llm = new ChatGptModel(Constants.OpenAIKey,
            ChatGptModelConfiguration.Default
                with
                {
                    Temperature = 0,
                },
            TestLogger.Create<OpenAIModel>())
        {
            DefaultStop = new[] { "3" }
        };

        // Assert
        Assert.ThrowsAsync<InvalidOperationException>(() => llm.PromptAsync("write an ordered list of five items", stop: new List<string> { "\n" }));
    }
}