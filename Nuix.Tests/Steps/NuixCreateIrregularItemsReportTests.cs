﻿using System.Collections.Generic;
using Reductech.EDR.Connectors.FileSystem;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Core;
using static Reductech.EDR.Connectors.Nuix.Tests.Constants;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Connectors.Nuix.Tests.Steps
{

public partial class
    NuixCreateIrregularItemsReportTests : NuixStepTestBase<NuixCreateIrregularItemsReport,
        StringStream>
{
    /// <inheritdoc />
    protected override IEnumerable<NuixIntegrationTestCase> NuixTestCases
    {
        get
        {
            yield return new NuixIntegrationTestCase(
                "Irregular Items",
                SetupCase,
                DeleteOutputFolder,
                CreateOutputFolder,
                new FileWrite
                {
                    Stream = new NuixCreateIrregularItemsReport(),
                    Path   = new PathCombine { Paths = Array(OutputFolder, "Irregular.txt") }
                },
                AssertFileContains(
                    OutputFolder,
                    "Irregular.txt",
                    "Unrecognised\tNew Folder/data/Theme in Yellow.txt"
                ),
                AssertFileContains(
                    OutputFolder,
                    "Irregular.txt",
                    "NeedManualExamination\tNew Folder/data/Jellyfish.txt"
                ),
                CleanupCase,
                DeleteOutputFolder
            );
        }
    }
}

}
