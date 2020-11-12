﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using CSharpFunctionalExtensions;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Tests
{
    public abstract partial class NuixStepTestBase<TStep, TOutput>
    {
        /// <inheritdoc />
        protected override IEnumerable<StepCase> StepCases { get { yield break; } }

        public class UnitTest : StepCase
        {

            public UnitTest(string name,
                Sequence sequence,
                IReadOnlyCollection<string> valuesToLog,
                IEnumerable<(string key, string value)> expectedExtraArgs,
                params string[] expectedLogValues)
                : base(name, sequence, Maybe<TOutput>.None, expectedLogValues)
            {
                SetupRunner(valuesToLog, expectedExtraArgs.ToList());
            }

            public UnitTest(string name,
                TStep step,
                TOutput expectedOutput,
                IReadOnlyCollection<string> valuesToLog,
                IEnumerable<(string key, string value)> expectedExtraArgs,
                params string[] expectedLogValues)
                : base(name, step, expectedOutput, expectedLogValues)
            {
                SetupRunner(valuesToLog, expectedExtraArgs.ToList());
            }

            private void SetupRunner(IEnumerable<string> valuesToLog, IReadOnlyList<(string key, string value)> expectedArgPairs)
            {
                AddFileSystemAction(x => x.Setup(y => y.WriteFileAsync(
                     It.IsRegex(@".*\.rb"),
                     It.Is<Stream>(s=> ValidateRubyScript(s, expectedArgPairs)),
                     It.IsAny<CancellationToken>()
                 )).ReturnsAsync(Unit.Default));


                AddExternalProcessRunnerAction(externalProcessRunner =>
                    externalProcessRunner.Setup(y => y.RunExternalProcess(It.IsAny<string>(),
                        It.IsAny<ILogger>(),
                        It.IsAny<IErrorHandler>(), It.Is<IEnumerable<string>>(ie=> AreExternalArgumentsCorrect(ie, expectedArgPairs))))
                    .Callback<string, ILogger, IErrorHandler, IEnumerable<string>>((s, logger, arg3, arg4) =>
                    {
                        foreach (var val in valuesToLog)
                        {
                            logger.LogInformation(val);
                        }

                        logger.LogInformation(ScriptGenerator.UnitSuccessToken);
                    })
                    .ReturnsAsync(Unit.Default));
            }

            private static bool ValidateRubyScript(Stream stream, IEnumerable<(string key, string value)> expectedArgPairs)
            {
                var reader = new StreamReader(stream);
                var text = reader.ReadToEnd();
                text.Should().NotBeNull();

                text.Should().Contain("require 'optparse'"); //very shallow testing that this is actually a ruby script

                foreach (var expectedArgPair in expectedArgPairs)
                {
                    text.Should().Contain(expectedArgPair.key);
                }

                return true;
            }

            private static bool AreExternalArgumentsCorrect(IEnumerable<string> externalProcessArgs, IReadOnlyList<(string key, string value)> expectedArgPairs)
            {
                var list = externalProcessArgs.ToList();
                var extraArgs = list.Skip(3).ToList();
                list[0].Should().Be("-licencesourcetype");
                list[1].Should().Be("dongle");
                list[2].Should().Match("*.rb");

                var realArgPairs = new List<(string key, string value)>();

                for (var i = 0; i < extraArgs.Count() - 1; i+=2)
                {
                    var key = extraArgs[i];
                    var value = extraArgs[i + 1];

                    realArgPairs.Add((key, value));
                }

                realArgPairs.Select(x => x.key).Should().BeEquivalentTo(expectedArgPairs.Select(x => "--" + x.key));

                foreach (var ((key, realValue), (_, expectedValue)) in realArgPairs.Zip(expectedArgPairs))
                {
                    realValue.Should().Contain(expectedValue, $"values of '{key}' should match");
                }


                return true;
            }
        }
    }
}
