﻿using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Connectors.FileSystem;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core.TestHarness;
using Xunit;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Connectors.Nuix.Tests
{

public class RequirementsTest
{
    private const string FakeConstantPath = "abcd";

    public static readonly TheoryData<(string? expectedError, NuixSettings settings)> TestCases =
        new()
        {
            ("Could not get settings value: Nuix.Features",
             new NuixSettings(
                 FakeConstantPath,
                 new Version(8, 0),
                 true,
                 new List<NuixFeature>()
             )
            ),
            ("Requirement 'Nuix Version 7.0 Features: ANALYSIS' not met.",
             new NuixSettings(
                 FakeConstantPath,
                 new Version(8, 0),
                 true,
                 new List<NuixFeature> { NuixFeature.CASE_CREATION }
             )
            ),
            (null,
             new NuixSettings(
                 FakeConstantPath,
                 new Version(8, 0),
                 true,
                 new List<NuixFeature> { NuixFeature.ANALYSIS }
             )
            )
        };

    [Theory(Skip = "Currently broken")]
    [MemberData(nameof(TestCases))]
    public void TestRequirements((string? expectedError, NuixSettings settings) args)
    {
        var process = new NuixSearchAndTag { SearchTerm = Constant("a"), Tag = Constant("c") };

        var (expectedError, settings) = args;

        var result = process.Verify(
            SettingsHelpers.CreateStepFactoryStore(settings, typeof(DeleteItem).Assembly)
        );

        if (expectedError == null)
            result.ShouldBeSuccessful();
        else
            result.MapError(x => x.AsString).ShouldBeFailure(expectedError);
    }
}

}
