using DotnetPrompt.Abstractions.LLM;
using DotnetPrompt.Tests.Integration;
using DotnetPrompt.Tests.Unit.LargeLanguageModels;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NUnit.Framework;

namespace DotnetPrompt.Tests.Unit.LLM;

public class BaseModelCacheTests
{
    [Test]
    public async Task Test()
    {
        var settings = new MemoryDistributedCacheOptions()
        {
           TrackStatistics = true
        };

        var appSettingsOptions = Options.Create(settings);
        var cache = new MemoryDistributedCache(appSettingsOptions);

        var fakeLLM = new FakeLargeLanguageModel(TestLogger.Create<FakeLargeLanguageModel>(), cache)
        {
            UseCache = true,
            Queries = new Dictionary<string, string>()
            {
                {"hello?", "hello!"}
            }
        };

        var first = await fakeLLM.PromptAsync("hello?");
        var second = await fakeLLM.PromptAsync("hello?");

        Assert.AreEqual(first,second);
    }
}