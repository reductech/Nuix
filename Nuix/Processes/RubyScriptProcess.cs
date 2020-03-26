﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using CSharpFunctionalExtensions;
using Reductech.EDR.Utilities.Processes;
using Reductech.EDR.Utilities.Processes.immutable;
using Reductech.EDR.Utilities.Processes.mutable;

namespace Reductech.EDR.Connectors.Nuix.processes
{
    /// <summary>
    /// A process that runs a ruby script against NUIX
    /// </summary>
    public abstract class RubyScriptProcess : Process
    {
        /// <summary>
        /// Checks if the current set of arguments is valid
        /// </summary>
        /// <returns></returns>
        internal abstract string ScriptText { get; }

        internal abstract string MethodName { get; }


        /// <summary>
        /// Get arguments that will be given to the nuix script.
        /// </summary>
        /// <returns></returns>
        internal abstract IEnumerable<(string arg, string? val, bool valueCanBeNull)> GetArgumentValues();


        internal virtual IEnumerable<string> GetAdditionalArgumentErrors()
        {
            yield break;
        }

        /// <inheritdoc />
        public override Result<ImmutableProcess, ErrorList> TryFreeze(IProcessSettings processSettings)
        {
            var errors = new List<string>();

            var arguments = new List<KeyValuePair<string, string?>>();
            var parameterNames = new List<string>() {"utilities" }; //always provide the utilities argument
            var nuixExePath = "";
            var useDongle = false;

            if (!(processSettings is INuixProcessSettings nps))
                errors.Add($"Process Settings must be an instance of {typeof(INuixProcessSettings).Name}");
            else
            {
                if (string.IsNullOrWhiteSpace(nps.NuixExeConsolePath))
                    errors.Add($"'{nameof(nps.NuixExeConsolePath)}' must not be empty");
                else
                    nuixExePath = nps.NuixExeConsolePath;
                useDongle = nps.UseDongle;
            }

            foreach (var (key, value, canBeNull) in GetArgumentValues())
            {
                if (string.IsNullOrWhiteSpace(value) && !canBeNull)
                {
                    errors.Add($"Argument '{key}' must not be null"); //todo - this isn't the real argument names -> fix that
                }
                else
                {
                    parameterNames.Add(key);
                    arguments.Add(new KeyValuePair<string, string?>(key,value)); //Argument will be escaped later
                }
            }

            errors.AddRange(GetAdditionalArgumentErrors());

            if (errors.Any())
                return Result.Failure<ImmutableProcess, ErrorList>(new ErrorList(errors));
            else
            {
                var methodBuilder = new StringBuilder();
                var methodHeader = $@"def {MethodName}({string.Join(",", parameterNames)})";

                methodBuilder.AppendLine(methodHeader);
                methodBuilder.AppendLine(ScriptText);
                methodBuilder.AppendLine("end");

                var methodCalls = new ImmutableRubyScriptProcess.MethodCall(MethodName, arguments);

                var ip = new ImmutableRubyScriptProcess(GetName(), 
                        nuixExePath, 
                        useDongle, new List<string>{methodBuilder.ToString()} , new []{methodCalls});

                return  Result.Success<ImmutableProcess, ErrorList>(ip);
            }
        }
    }
}