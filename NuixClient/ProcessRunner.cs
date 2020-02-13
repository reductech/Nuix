﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Newtonsoft.Json;
using NuixClient.Orchestration;

namespace NuixClient
{
    /// <summary>
    /// Runs processes from Json
    /// </summary>
    public static class ProcessRunner
    {
        /// <summary>
        /// Run process defined in Json
        /// </summary>
        /// <param name="jsonString">Json representing the process</param>
        /// <returns></returns>
        [UsedImplicitly]
        public static async IAsyncEnumerable<ResultLine> RunProcessFromJsonString(string jsonString)
        {
            Process? process = null;
            ResultLine? errorLine;
            var text = jsonString;

            try
            {           
                process = JsonConvert.DeserializeObject<Process>(text,
                    new ProcessJsonConverter(),
                    new ConditionJsonConverter());
                errorLine = null;
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                errorLine = new ResultLine(false, e.Message);
            }
#pragma warning restore CA1031 // Do not catch general exception types

            if (errorLine != null)
            {
                yield return errorLine;
            }
            else if (process == null)
            {
                yield return new ResultLine(false, "Process is null");
            }
            else
            {
                foreach (var processCondition in process.Conditions ?? Enumerable.Empty<Condition>())
                {
                    if (processCondition.IsMet())
                        yield return new ResultLine(true, processCondition.GetDescription());
                    else
                    {
                        yield return new ResultLine(false, $"CONDITION NOT MET: [{processCondition.GetDescription()}]");
                        yield break;
                    }
                }

                var resultLines = process.Execute();
                await foreach (var resultLine in resultLines)
                {
                    yield return resultLine;
                }
            }
        }

        /// <summary>
        /// Run process defined in Json found at a particular path
        /// </summary>
        /// <param name="jsonPath">Path to the JSON</param>
        /// <returns></returns>
        [UsedImplicitly]
        public static async IAsyncEnumerable<ResultLine> RunProcessFromJson(string jsonPath)
        {
            string? text;
            ResultLine? errorLine = null;
            try
            {
                text = File.ReadAllText(jsonPath);                
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                errorLine = new ResultLine(false, e.Message);
                text = null;
            }
#pragma warning restore CA1031 // Do not catch general exception types

            if (errorLine != null)
            {
                yield return errorLine;
            }
            else if (!string.IsNullOrWhiteSpace(text))
            {
                var r = RunProcessFromJsonString(text);
                await foreach(var rl in r)
                {
                    yield return rl;
                }
            }
            else
            {
                yield return new ResultLine(false, "File is empty");
            }
        }

    }
}