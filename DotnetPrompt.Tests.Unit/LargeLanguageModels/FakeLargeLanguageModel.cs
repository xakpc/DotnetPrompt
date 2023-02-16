using DotnetPrompt.Abstractions.LLM;
using DotnetPrompt.Abstractions.Schema;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Text.Json.Serialization;
using System.Text.Json;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace DotnetPrompt.Tests.Unit.LargeLanguageModels;

public class FakeLargeLanguageModel : BaseModel
{
    public IDictionary<string, string> Queries { get; set; } = null;

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

    protected override async Task<LLMResult> GenerateInternalAsync(List<string> prompts, IList<string> stop = null)
    {
        var generations = new List<List<Generation>>();
        foreach (var prompt in prompts)
        {
            var text = await Task.FromResult(Call(prompt, stop));
            generations.Add(new List<Generation> { new() { Text = text } });
        }

        return new LLMResult { Generations = generations };
    }

    public override string LLMType => "fake";

    public FakeLargeLanguageModel(ILogger logger, IDistributedCache cache = null) : base(logger, cache)
    {
    }
}