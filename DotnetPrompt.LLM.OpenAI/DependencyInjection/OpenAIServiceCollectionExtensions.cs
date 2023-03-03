using DotnetPrompt.Abstractions.LLM;
using DotnetPrompt.Abstractions.Tools;
using Microsoft.Extensions.DependencyInjection;

namespace DotnetPrompt.LLM.OpenAI.DependencyInjection;

/// <summary>
/// Provides extension methods for registering <see cref="OpenAIModel"/> and <see cref="AzureOpenAIModel"/> in an <see cref="IServiceCollection"/>.
/// </summary>
public static class OpenAIServiceCollectionExtensions
{
    /// <summary>
    /// Register the dependencies OpenAIModel and OpenAIEmbeddings 
    /// </summary>
    public static IServiceCollection AddOpenAIModel(this IServiceCollection services)
    {
        // Register the dependencies required by your application here
        services.AddTransient<ILargeLanguageModel, OpenAIModel>();
        services.AddTransient<IEmbeddings, OpenAIEmbeddings>();

        return services;
    }

    /// <summary>
    /// Register the dependencies AzureOpenAIModel and AzureOpenAIEmbeddings 
    /// </summary>
    public static IServiceCollection AddAzureOpenAIModel(this IServiceCollection services)
    {
        // Register the dependencies required by your application here
        services.AddTransient<ILargeLanguageModel, AzureOpenAIModel>();
        //services.AddTransient<IEmbeddings, OpenAIEmbeddings>(); todo

        return services;
    }
}