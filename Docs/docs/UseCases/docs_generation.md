# Documentation Generation

Code documentation is crucial for any software development project. It helps to make code understandable, maintainable and reduces the time required to fix bugs. 
Documentation for classes and functions is particularly important as it gives an overview of what the code does, how to use it, and any constraints or limitations. 
However, writing documentation can be time-consuming and often gets neglected. 

In this article, we will explore how with help of `DotnetPrompt` a Large Language Model (LLM) can be used to automatically generate documentation for a class with minimal effort.

## Problem:

We have a source file with a `Logger` class. This class provides logging functionality to other parts of the codebase.
The `Logger` class has a predefined log messages in methods that look like this. 

```csharp
public void GetDocumentCodes(string id, int[] codes)
{
    WriteDebug(10001, new { id, codes },
        () => $"Reading document codes for id {id}.");
}

public void CreateMetadataCompleted(Guid id, TimeSpan timeToFilter)
{
    WriteInfo(11004, new { id, timeToFilter },
         () => $"Get comparison metadata for id {id} in {timeToFilter:c}");
}

public void IndexClientFailed(Exception exception)
{
    WriteError(13001, new { message = exception.Message },
        () => $"Failed to execute index client with error {exception.Message}", exception);
}
```

We need to create documentation for this class that includes a log code, log level and log message. 
This documentation should be presented in a markdown table format.

## Solution:

We will use custom `Chain` with an LLM to generate the documentation for the Logger class. 
The `Chain` will take the source file as input, and generate markdown table documentation for the class.
Inside the chain we will setup `OpenAIModel` with few prompt examples.

### Step 1: Install Required Packages

We will be using the OpenAI GPT-3 API to generate the documentation. You will need to sign up for an API key from OpenAI and install the `DotnetPrompt` from NuGet.

```ps
> dotnet add package DotnetPrompt.All --version 1.0.0-alpha.1
```

### Step 2: Initialize OpenAI API Key

We would not use configuration or something like this, so we just store API key as a constant.

```csharp
public static class Constants
{
    public const string OpenAIKey = "YOUR-KEY";    
}
```

### Step 3: Define the Prompt Examples and setup `ModelChain`

We will define a few prompt examples for the LLM to generate the documentation. 
These examples will give the LLM an idea of the format and structure we want the documentation to take.

Inside out custom chain we will use basic `ModelChain` to make a call to LLM.

[!code-csharp[](../../../test/DotnetPrompt.Tests.Examples/UseCases/UseCaseGenerateDocumentation.cs#ModelSetup)]

### Step 4: Build the dataflow

Next we need to setup our dataflow

[!code-csharp[](../../../test/DotnetPrompt.Tests.Examples/UseCases/UseCaseGenerateDocumentation.cs#BuildDataflow)]

The dataflow here goes like this:

1. `TransformManyBlock` -> get a single `ChainMessage` with file name and produce `ChainMessage` for each method.
1. `ModelChain` -> Consume `ChainMessage` with method and extract documentation row from it (this could be launch in parallel, so we could simultaniously generate 5-10 rows).
1. `BatchBlock` -> Collect results `ChainMessage` from models.
1. `TransformBlock` -> Combine results from `BatchBlock` into a final `ChainMessage` with a table.

In fact you could pass any data between internal blocks, only first and last need to consume and return `ChainMessage`.
The only recomendation here is to pass `Id` from input block to output block (without it executor would not work for example)

First and last data block we would make as a field to publish them as Input and Output of our chain.

```csharp
public ITargetBlock<ChainMessage> InputBlock => _transformationBlockOne;
public ISourceBlock<ChainMessage> OutputBlock => _finalizatorBlock;
```

### Step 5: Parse the Source File

Before we can generate the markdown table documentation, we need to extract the methods of the Logger class from the source file. 
For that we have a `_transformationBlockOne` which action `ReadMethodsFromFile` could look like this.

[!code-csharp[](../../../test/DotnetPrompt.Tests.Examples/UseCases/UseCaseGenerateDocumentation.cs#ReadMethodsFromFile)]

### Step 6: Combine rows into table

[!code-csharp[](../../../test/DotnetPrompt.Tests.Examples/UseCases/UseCaseGenerateDocumentation.cs#CombineRowsToTable)]

### Step 7: Run method

We need implemetation of `Run` method. Here we consume input `ChainMessage` with a single value - file name and post it to dataflow.

[!code-csharp[](../../../test/DotnetPrompt.Tests.Examples/UseCases/UseCaseGenerateDocumentation.cs#RunMethod)]

Note the `Complete` method. We telling our chain that no more data will be added after inital file name.
It's important to complete this one, because otherwise `BufferBlock` will be waiting forever or until it capacity full.

### Step 8: Generate the Markdown Table Documentation

The usage of the chain is starighforward: we run the chain and wait for completion:

[!code-csharp[](../../../test/DotnetPrompt.Tests.Examples/UseCases/UseCaseGenerateDocumentation.cs#GenerateDocumentation)]

Final result

```markdown
  Standard Output: 
 | Info | 17001 | WorkerStart | Start worker |
 | Info | 17003 | StartProcessingDocument | Start processing document |
 | Info | 17004 | EndProcessingDocument | End processing document. ElapsedMillisecond: {elapsedMillisecond} |
 | Error | 17100 | ProcessDocumentHandledException | Processing Handled Exception |
 | Warning | 171001 | ChangeTypeArgumentOutOfRangeWarning | Wrong change type. Exception message: {exception.Message} |
```

And here is a log of how our chain worked
```text
2023-03-02T23:15:09 | Information | Reading file Data\Logger.cs
2023-03-02T23:15:09 | Information | Extracted 5 methods
2023-03-02T23:15:09 | Trace | Sending LLM request
2023-03-02T23:15:10 | Information | Result of ModelChain:  | Info | 17001 | WorkerStart | Start worker |
2023-03-02T23:15:10 | Trace | Sending LLM request
2023-03-02T23:15:11 | Information | Result of ModelChain:  | Info | 17003 | StartProcessingDocument | Start processing document |
2023-03-02T23:15:11 | Trace | Sending LLM request
2023-03-02T23:15:13 | Information | Result of ModelChain:  | Info | 17004 | EndProcessingDocument | End processing document. ElapsedMillisecond: {elapsedMillisecond}
2023-03-02T23:15:13 | Trace | Sending LLM request
2023-03-02T23:15:14 | Information | Result of ModelChain:  | Error | 17100 | ProcessDocumentHandledException | Processing Handled Exception |
2023-03-02T23:15:14 | Trace | Sending LLM request
2023-03-02T23:15:16 | Information | Result of ModelChain:  | Warning | 171001 | ChangeTypeArgumentOutOfRangeWarning | Wrong change type. Exception message: {exception.Message} |
2023-03-02T23:15:16 | Information | Finalization
```

### Conclusion

In this article, we have seen how an LLM can be used to automatically generate documentation for a class. 
We used the OpenAI API to generate markdown table documentation for a Logger class by defining a few prompt examples and parsing the source file. 

While the documentation generated by the LLM may not be perfect, it can serve as a starting point for further refinement and can save developersa significant amount of time. 

The obvious improvement would be to provide list of examples as a parameter to make this chain suitable to generate documentation based on any kind of methods.
This approach can be extended to generate documentation for other classes and functions in a codebase, making documentation a less time-consuming and tedious task.
