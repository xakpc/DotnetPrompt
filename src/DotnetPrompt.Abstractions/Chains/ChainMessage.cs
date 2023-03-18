using System;
using System.Collections.Generic;

namespace DotnetPrompt.Abstractions.Chains;

/// <summary>
/// Message that goes through chains
/// </summary>
/// <param name="Values"></param>
/// <param name="Stops"></param>
public record ChainMessage(IDictionary<string, string> Values, IList<string> Stops = null)
{
    public Guid Id { get; set; } = Guid.NewGuid();
}