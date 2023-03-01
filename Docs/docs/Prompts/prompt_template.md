# Create a prompt template

You can create prompts using the `PromptTemplate` class. Prompt templates can take any number of *input variables*, and can be formatted with *input values* to generate a prompt.

_An example prompt with no input variables_

```csharp
var noInputPrompt = new PromptTemplate("Tell me a joke.");
Console.WriteLine(noInputPrompt.Format(new Dictionary<string, string>()));
```

```text
> Tell me a joke.
```

_An example prompt with one input variable_

```csharp
var oneInputPrompt =
    new PromptTemplate(template: "Tell me a {adjective} joke.", inputVariables: new[] { "adjective" });

var valuesOneInput = new Dictionary<string, string>
{
    { "adjective", "funny" }
};
Console.WriteLine(oneInputPrompt.Format(valuesOneInput));
```

```text
> Tell me a funny joke.
```

_An example prompt with multiple input variables_

```csharp
var multipleInputPrompt = new PromptTemplate("Tell me a {adjective} joke about {content}.",
    new[] { "adjective", "content" });

var valuesMultipleInput = new Dictionary<string, string>
{
    { "adjective", "funny" },
    { "content", "chickens" }
};
Console.WriteLine(multipleInputPrompt.Format(valuesMultipleInput));
```

```text
> Tell me a funny joke about chickens.
```

You could pass input variables into `PromptTempalte` constructor as a `List<string>` but also you could omit it, then it would be built automatically.

```csharp
var oneInputPrompt = new PromptTemplate(template: "Tell me a {adjective} joke.");
```

If you want to pass symbols '{' or '}' as a part of a prompt you should escape them like this

```csharp
 new PromptTemplate(template: "{{ \"code\": \"{value}\" }}");
```

which will produce final string after formatting and set up value

```text
{ "code": "input value" }
```

When you want to fill template with values you need to use `Dictionary<string, string>` where keys should be the same as your input variables and values could be
any valid string that need to be fill in template.
