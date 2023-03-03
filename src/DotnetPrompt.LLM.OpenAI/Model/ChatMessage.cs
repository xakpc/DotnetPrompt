using System.Text.Json.Serialization;

namespace DotnetPrompt.LLM.OpenAI.Model;

public record ChatMessage
{
    public ChatMessage(string role, string content)
    {
        this.Role = role;
        this.Content = content;
    }

    [JsonPropertyName("role")]
    public string Role { get; init; }

    [JsonPropertyName("content")]
    public string Content { get; init; }
}