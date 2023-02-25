# Create a custom prompt template

Let's suppose we want the LLM to generate English language explanations of a python function given its name. To achieve this task, we will create a custom prompt template that takes in the function name as input, and formats the prompt template to provide the source code of the function.

## Why are custom prompt templates needed?

DotnetPrompt provides a set of default prompt templates that can be used to generate prompts for a variety of tasks. However, there may be cases where the default prompt templates do not meet your needs. For example, you may want to create a prompt template with specific dynamic instructions for your language model. In such cases, you can create a custom prompt template.

Take a look at the current set of default prompt templates here.

## Create a custom prompt template

The only two requirements for all prompt templates are:

1. They have a `InputVariables` property that exposes what input variables this prompt template expects.
1. They expose a `Format` method which takes in keyword arguments corresponding to the expected `InputVariables` and returns the formatted prompt.

These requirements set in `IPromptTemplate` interface, so your custom prompt template should implement it.

[!code-csharp[](../../../../DotnetPrompt.Abstractions/Prompts/IPromptTemplate.cs#L6-L23)]

Let's create a custom prompt template that takes in the python file and function name as input, and formats the prompt template to provide the source code of the function.

First, we need a function that will return the source code of a function given its name. For that the helper method `PythonHelpers.GetPythonMethods` would parse python class with Regex to extract methods.

```cs
IEnumerable<(string Name, string Def, string Body)> GetPythonMethods(string filePath)
```

Next, we'll create a custom prompt template that takes in the file path and the function name as input, and formats the prompt template to provide the source code of the function.

[!code-csharp[](../../../../DotnetPrompt.Tests.Examples/Prompts/PromptTemplateExamples.cs#Example_CustomPromptTemplate_FunctionExplainerPromptTemplate)]

# Use the custom prompt template

Now that we have created a custom prompt template, we can use it to generate prompts for our task.

[!code-csharp[](../../../../DotnetPrompt.Tests.Examples/Prompts/PromptTemplateExamples.cs#Example_CustomPromptTemplate)]

Generated output:

```text
Given the function name and source code, generate an English language explanation of the function.
Function Name: format

Source Code:
def format(self, **kwargs: Any) -> str:
        """Format the prompt with the inputs.

        Args:
            kwargs: Any arguments to be passed to the prompt template.

        Returns:
            A formatted string.

        Example:

        .. code-block:: python

            prompt.format(variable1="foo")
        """
        # Get the examples to use.
        examples = self._get_examples(**kwargs)
        # Format the examples.
        example_strings = [
            self.example_prompt.format(**example) for example in examples
        ]
        # Create the overall template.
        pieces = [self.prefix, *example_strings, self.suffix]
        template = self.example_separator.join([piece for piece in pieces if piece])
        # Format the template with the input variables.
        return DEFAULT_FORMATTER_MAPPING[self.template_format](template, **kwargs)
Explanation:
```        