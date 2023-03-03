using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using DotnetPrompt.Abstractions.Prompts;

namespace DotnetPrompt.Prompts;

/// <summary>
/// List of chatml roles
/// </summary>
public static class ChatRoles
{
    /// <summary>
    /// User Role
    /// </summary>
    public const string User = "user";
    /// <summary>
    /// System Role
    /// </summary>
    public const string System = "system";
    /// <summary>
    /// Assistant Role
    /// </summary>
    public const string Assistant = "assistant";
}

/// <summary>
/// Prompt template of chat message
/// </summary>
/// <param name="Role">One of the chat role</param>
/// <param name="ContentTemplate">Template for content, might be parameterless</param>
public record ChatMessageTemplate(string Role, IPromptTemplate ContentTemplate);

/// <summary>
/// PromptTemplate for generating ChatML messages
/// </summary>
public class ChatMLPromptTemplate : IPromptTemplate
{
    /// <summary>
    /// List of <see cref="ChatMessageTemplate"/>
    /// </summary>
    public IList<ChatMessageTemplate> Messages { get; } = new List<ChatMessageTemplate>();

    /// <inheritdoc />
    public IList<string> InputVariables { get; } = new List<string>();

    #region Constructors

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="content">PromptTemplate for content string</param>
    /// <param name="system">System string</param>
    /// <param name="priorMessages"></param>
    public ChatMLPromptTemplate(IPromptTemplate content, string? system = default, IList<(string userMessage, string assistantMessage)>? priorMessages = default)
    {
        Messages.Add(new ChatMessageTemplate(ChatRoles.User, content));
        foreach (var contentInputVariable in content.InputVariables)
        {
            InputVariables.Add(contentInputVariable);
        }

        SetupPreUserMessages(system, priorMessages);
    }

    /// <summary>
    /// ctor
    /// </summary>
    /// <param name="content">Content string</param>
    /// <param name="system">System string</param>
    /// <param name="priorMessages"></param>
    public ChatMLPromptTemplate(string content, string? system = default, IList<(string userMessage,string assistantMessage)>? priorMessages = default)
    {
        var prompt = new PromptTemplate(content);
        Messages.Add(new ChatMessageTemplate(ChatRoles.User, prompt));
        foreach (var contentInputVariable in prompt.InputVariables)
        {
            InputVariables.Add(contentInputVariable);
        }

        SetupPreUserMessages(system, priorMessages);
    }

    private void SetupPreUserMessages(string? system, IList<(string userMessage, string assistantMessage)>? priorMessages)
    {
        if (!string.IsNullOrEmpty(system))
        {
            AddSystemMessage(system);
        }

        if (priorMessages == null)
        {
            return;
        }

        foreach (var valueTuple in priorMessages)
        {
            AddPriorResponse(valueTuple.Item1, valueTuple.Item2);
        }
    }
    #endregion

    /// <summary>
    /// Add prior response pair
    /// </summary>
    /// <param name="userMessage"></param>
    /// <param name="assistantMessage"></param>
    /// <returns></returns>
    public ChatMLPromptTemplate AddPriorResponse(string userMessage, string assistantMessage)
    {
        // insert before last User message
        var messageUserIndex = Messages.IndexOf(Messages.Last(i => i.Role == ChatRoles.User));
        var userMessageTemplate = new ChatMessageTemplate(ChatRoles.User, new PromptTemplate(userMessage));
        Messages.Insert(messageUserIndex, userMessageTemplate);
        Messages.Insert(Messages.IndexOf(userMessageTemplate) + 1, new ChatMessageTemplate(ChatRoles.Assistant, new PromptTemplate(assistantMessage)));

        return this;
    }

    /// <summary>
    /// Add system message
    /// </summary>
    /// <param name="systemMessage"></param>
    /// <returns></returns>
    public ChatMLPromptTemplate AddSystemMessage(string systemMessage)
    {
        // insert before last User message
        Messages.Insert(0, new ChatMessageTemplate(ChatRoles.System, new PromptTemplate(systemMessage)));

        return this;
    }

    /// <inheritdoc />
    public string Format(IDictionary<string, string>? values = default)
    {
        var messages = Messages.Select(m => new { m.Role, Content = m.ContentTemplate.Format(values) });
        return JsonSerializer.Serialize(messages, new JsonSerializerOptions()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }
}