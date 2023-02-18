using System;
using System.Collections.Generic;

namespace DotnetPrompt.Abstractions.Chains;

public record ModelChainContext(IDictionary<string, string> Values, IList<string> Stops = null)
{
    //public IList<string> Inputs { get; }
    //string Output { get; }

    public Guid Id { get; } = Guid.NewGuid();
}