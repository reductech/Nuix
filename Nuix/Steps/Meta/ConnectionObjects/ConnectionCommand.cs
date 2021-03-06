﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace Reductech.EDR.Connectors.Nuix.Steps.Meta.ConnectionObjects
{

/// <summary>
/// A command that can be sent to nuix.
/// </summary>
public class ConnectionCommand
{
    /// <inheritdoc />
    public override string ToString() => Command;

    /// <summary>
    /// The name of the command to send.
    /// </summary>
    [JsonProperty("cmd")]
    public string Command { get; set; } = null!;

    /// <summary>
    /// The function definition, if it is the first time this command is being sent.
    /// </summary>
    [JsonProperty("def")]
    public string? FunctionDefinition { get; set; }

    /// <summary>
    /// Arguments to the function.
    /// </summary>
    [JsonProperty("args")]
    public Dictionary<string, object>? Arguments { get; set; }

    /// <summary>
    /// The path to the case.
    /// If this is not set the current case will be used
    /// </summary>
    [JsonProperty("casepath")]
    public string? CasePath { get; set; }

    /// <summary>
    /// Whether this process takes a stream as an argument
    /// </summary>
    // ReSharper disable once StringLiteralTypo
    [JsonProperty("isstream")]
    public bool? IsStream { get; set; }

    /// <summary>
    /// True if this function is a helper and does not need to be executed.
    /// </summary>
    [JsonProperty("ishelper")]
    public bool? IsHelper { get; set; }
}

}
