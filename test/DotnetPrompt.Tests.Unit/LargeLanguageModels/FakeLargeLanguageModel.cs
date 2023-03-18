using DotnetPrompt.Abstractions.LLM;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;
using DotnetPrompt.Abstractions.LLM.Schema;
using Microsoft.Extensions.Logging.Abstractions;

namespace DotnetPrompt.Tests.Unit.LargeLanguageModels;

public class FakeLargeLanguageModel : BaseModel
{
    public IDictionary<string, string> Queries { get; set; }

    public string Call(string prompt, IList<string> stop = null)
    {
        if (Queries != null)
        {
            return Queries[prompt];
        }

        return stop == null ? "foo" : "bar";
    }

    protected override string AsUniqueString()
    {
        var serializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        return JsonSerializer.Serialize(this, serializerOptions);
    }

    protected override async Task<ModelResult> GenerateInternalAsync(IList<string> prompts, IList<string> stop = null)
    {
        var generations = new List<IList<Generation>>();
        foreach (var prompt in prompts)
        {
            var text = await Task.FromResult(Call(prompt, stop));
            generations.Add(new List<Generation> { new() { Text = text } });
        }

        return new ModelResult { Generations = generations };
    }


    public override string LLMType => "fake";
    public override int MaxRequestTokens => 100;
    public FakeLargeLanguageModel(ILogger logger, IDistributedCache cache = null) : base(logger, cache)
    {
    }

    public FakeLargeLanguageModel(Dictionary<string, string> queries = null) : base(NullLogger.Instance, null)
    {
        Queries = queries;
    }
}