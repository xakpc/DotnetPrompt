using DotnetPrompt.Abstractions.LLM;
using DotnetPrompt.LLM.CohereAI;
using NUnit.Framework;
using DotnetPrompt.LLM.OpenAI;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace DotnetPrompt.Tests.Integration;

public class OpenAiModelTests
{

    private BaseModel BuildModel(string llm)
    {
        switch (llm)
        {
            case "openai":
                return new OpenAIModel(Constants.OpenAIKey,
                    OpenAIModelConfiguration.Default with { MaxTokens = 10 },
                    TestLogger.Create<OpenAIModel>());

            case "cohereai":
                return new CohereAIModel(Constants.CohereAIKey,
                    new CohereAIModelConfiguration() { MaxTokens = 10 },
                    TestLogger.Create<OpenAIModel>());

            default: throw new ArgumentException(nameof(llm));
        }
    }

    [TestCase("openai")]
    [TestCase("cohereai")]
    public async Task PromptAsync_SanityCheck_StringOutputNotEmpty(string model)
    {
        // Arrange
        var llm = BuildModel(model);

        // Act
        var output = await llm.PromptAsync("Say foo:");

        // Assert
        Assert.IsNotEmpty(output);
    }

    [Test]
    public async Task PromptAsync_WithNegativeMaxTokensAndSinglePrompt_StringOutputNotEmpty()
    {
        // Arrange
        var llm = new OpenAIModel(Constants.OpenAIKey,
            OpenAIModelConfiguration.Default with { MaxTokens = -1 },
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

        var llm = new OpenAIModel(Constants.OpenAIKey,
            OpenAIModelConfiguration.Default with { MaxTokens = -1 },
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

        var llm = new OpenAIModel(Constants.OpenAIKey,
            OpenAIModelConfiguration.Default with { MaxTokens = -1 },
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
    public async Task PromptAsync_WithExtraArguments_StringOutputNotEmpty()
    {
        // Arrange
        var llm = new OpenAIModel(Constants.OpenAIKey,
            OpenAIModelConfiguration.Default with { MaxTokens = 10 },
            TestLogger.Create<OpenAIModel>())
        {
            ModelExtraArguments =
            {
                ["user"] = "DotnetPrompt.Test"
            }
        };

        // Act
        var output = await llm.PromptAsync("Say foo:");

        // Assert
        Assert.IsNotEmpty(output);
    }


    [Test]
    public void Model_ModelExtraArguments_AddedProperlyAndValidated()
    {
        // Arrange
        var llm1 = new OpenAIModel(Constants.OpenAIKey,
            OpenAIModelConfiguration.Default with { MaxTokens = 10 },
            TestLogger.Create<OpenAIModel>())
        {
            ModelExtraArguments = new Dictionary<string, object> { { "foo", 3 } }
        };

        var llm2 = new OpenAIModel(Constants.CohereAIKey, OpenAIModelConfiguration.Default, TestLogger.Create<OpenAIModel>())
            { ModelExtraArguments = new Dictionary<string, object> { { "foo", 3 }, { "bar", 2 } } };

        // Assert
        Assert.AreEqual(10, llm1.DefaultModelConfiguration.MaxTokens);
        Assert.AreEqual(1, llm1.ModelExtraArguments.Count);
        Assert.AreEqual(3, llm1.ModelExtraArguments["foo"]);

        Assert.AreEqual(2, llm2.ModelExtraArguments.Count);
        Assert.AreEqual(3, llm2.ModelExtraArguments["foo"]);
        Assert.AreEqual(2, llm2.ModelExtraArguments["bar"]);

        // todo
        //Assert.Throws<ArgumentException>(() =>
        //    new OpenAIModel(TestLogger.Create<OpenAIModel>(), Key)(foo: 3, model_kwargs: new Dictionary<string, object> { { "foo", 2 } }));
    }

    [Test]
    public async Task PromptAsync_WithStop_ValidConfiguration()
    {
        // Arrange
        var query = "write an ordered list of five items";

        var firstLlm = new OpenAIModel(Constants.OpenAIKey, OpenAIModelConfiguration.Default with { Temperature = 0 },
            TestLogger.Create<OpenAIModel>())
        {
            DefaultStop = new List<string> { "3" },
        };

        var secondLlm = new OpenAIModel(Constants.OpenAIKey, OpenAIModelConfiguration.Default with { Temperature = 0 },
            TestLogger.Create<OpenAIModel>());

        var thirdLlm = new OpenAIModel(Constants.OpenAIKey, OpenAIModelConfiguration.Default with { Stop = new List<string> { "3" }, Temperature = 0 },
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
        var llm = new OpenAIModel(Constants.OpenAIKey,
            OpenAIModelConfiguration.Default
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
        var llm = new OpenAIModel(Constants.OpenAIKey,
            OpenAIModelConfiguration.Default
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

    ///// <summary>
    ///// Test saving/loading an OpenAPI LLM.
    ///// </summary>
    //[Test]
    //public void Test_Saving_Loading_LLM()
    //{
    //    // Arrange
    //    var tmp_path = Path.GetTempPath();
    //    var file_path = Path.Combine(tmp_path, "openai.yaml");
    //    var llm = new OpenAIModel(Key)(max_tokens: 10);

    //    // Act
    //    llm.Save(file_path);
    //    var loaded_llm = OpenAIModel.Load(file_path);

    //    // Assert
    //    Assert.AreEqual(llm.max_tokens, loaded_llm.max_tokens);
    //}

    ///// <summary>
    ///// Test streaming tokens from OpenAI.
    ///// </summary>
    //[Test]
    //public void Test_Openai_Streaming()
    //{
    //    // Arrange
    //    OpenAI llm = new OpenAIModel(Key)(max_tokens: 10);
    //    IEnumerator<Dictionary<string, object>> generator = llm.Stream("I'm Pickle Rick");

    //    // Assert
    //    Assert.IsInstanceOf(typeof(IEnumerator<Dictionary<string, object>>), generator);

    //    // Act & Assert
    //    while (generator.MoveNext())
    //    {
    //        Assert.IsInstanceOf(typeof(string), generator.Current["choices"][0]["text"]);
    //    }
    //}

    ///// <summary>
    ///// Test error handling in stream.
    ///// </summary>
    //[Test]
    //public void TestOpenaiStreamingError()
    //{
    //    // Arrange
    //    var llm = new OpenAIModel(Key)(bestOf: 2);

    //    // Act & Assert
    //    Assert.Throws<ArgumentException>(() => llm.Stream("I'm Pickle Rick"));
    //}

    ///// <summary>
    ///// Test validation for streaming fails if best_of is not 1.
    ///// </summary>
    //[Test]
    //public void TestOpenaiStreamingBestOfError()
    //{
    //    // Arrange, Act & Assert
    //    Assert.Throws<ArgumentException>(() => new OpenAIModel(Key)(bestOf: 2, streaming: true));
    //}
}