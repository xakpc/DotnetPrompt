# Getting Started

The LLM class is a class designed for interfacing with LLMs. Any model class in DotnetPrompt should have a base class of `BaseModel`
There are lots of LLM providers (OpenAI, Cohere, Hugging Face, etc) - this class is designed to provide a standard interface for all of them. 

In this part of the article, we will focus on generic LLM functionality. 
For details on working with a specific LLM wrapper, please see the examples in the Provider section.

For this article, we will work with an OpenAI Model, although the functionalities highlighted are generic for all LLM types.

LLMs are distributed as separate NuGet packages and could be constructed directly or through [dependecy injection](../getting_started.md#dependency-injecton). 

> [!NOTE]
> If model is constructed through DI it would require its configuration te be set through `IConfiguration`.

```text
dotnet add package DotnetPrompt.LLM.OpenAI --version 0.0.1-alpha1
```

To create model manually

[!code-csharp[](../../../test/DotnetPrompt.Tests.Examples/LLMs/OpenAIModelExamples.cs#CreateOpenAiModel)]

**Generate Text**: The most basic functionality an LLM has is the ability to call `PromptAsync` it, passing in a string and getting back a string.

[!code-csharp[](../../../test/DotnetPrompt.Tests.Examples/LLMs/OpenAIModelExamples.cs#Example_PromptLLM)]

Result would be:

```text
> Why did the chicken cross the road?
> To get to the other side!
```

**Generate**: More broadly, you can call `GenerateAsync` with a list of inputs, getting back a more complete response than just the text. 
This complete response includes things like multiple top responses, as well as LLM providers' specific information.

[!code-csharp[](../../../test/DotnetPrompt.Tests.Examples/LLMs/OpenAIModelExamples.cs#Example_GenerateAsync)]

This example will take two inputs and generate two completions per each input. 

Here is how final result might be looking

```json
{
  "Generations": [ 
    [
      {
        "Text": "\n\nQ: What did the fish say when it hit its head?\nA: \"OW!\"",
        "Info": {
          "finish_reason": "stop",
          "logprobs": null
        }
      },
      {
        "Text": "\n\nQ: What did the fish say when he hit the wall? \nA: Dam!",
        "Info": {
          "finish_reason": "stop",
          "logprobs": null
        }
      } 
    ],
    [
      {
         "Text": "\n\nThe fields of green and the standing stones,\nThe cool wind against my skin,\nThe distant harbor and rolling hills,\nA beauty I'm lost in.\n\nThe birds that sing in the silent trees,\nThe whispering of the brook,\nThe heather-clad peaks of the highland hills,\nAn enchantment I took.\n\nThe mists of morning and twilight's hush,\nThe blanket of stars that night,\nThe rustling of leaves and the swirl of snow,\nA world of pure delight.",
         "Info": {
            "finish_reason": "stop",
            "logprobs": null
         }
      },
      {
         "Text": "\n\nRoses are red\nViolets are blue\nSugar is sweet\nAnd so are you!",
         "Info": {
           "finish_reason": "stop",
           "logprobs": null
         }
      }
    ]
  ],
  "Output": {
    "token_usage": {
      "completion_tokens": 178,
      "prompt_tokens": 8,
      "total_tokens": 186
    }
  }
}
```