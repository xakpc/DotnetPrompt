using System;
using System.Collections.Generic;

namespace DotnetPrompt.Abstractions.Chains;

public record DataflowRequest<T>(T Value)
{
    public Guid Id { get; } = Guid.NewGuid();
}

public record ChainMessage(IDictionary<string, string> Values, IList<string> Stops = null)
{
    public Guid Id { get; set;  } = Guid.NewGuid();
}