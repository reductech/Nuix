﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Connectors.Nuix.Steps.Meta.ConnectionObjects;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Steps;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Util;
using static Reductech.EDR.Core.TestHarness.StaticHelpers;

namespace Reductech.EDR.Connectors.Nuix.Tests
{

public static class Constants
{
    //TODO set paths from a config file, or something
    public const string Nuix70Path = @"C:\Program Files\Nuix\Nuix 7.0";
    public const string Nuix7Path = @"C:\Program Files\Nuix\Nuix 7.8";
    public const string Nuix8Path = @"C:\Program Files\Nuix\Nuix 8.8";
    public const string Nuix9Path = @"C:\Program Files\Nuix\Nuix 9.0";

    public const string NuixConsoleExe = "nuix_console.exe";

    public static List<NuixFeature> AllNuixFeatures =>
        Enum.GetValues(typeof(NuixFeature)).Cast<NuixFeature>().ToList();

    // The latest version supported by this connector should always be the first one in this list
    public static IReadOnlyCollection<SCLSettings> NuixSettingsList => new List<SCLSettings>
    {
        NuixSettings.CreateSettings(
            Path.Combine(Nuix9Path, NuixConsoleExe),
            new Version(9, 0),
            true,
            AllNuixFeatures
        ),
        NuixSettings.CreateSettings(
            Path.Combine(Nuix8Path, NuixConsoleExe),
            new Version(8, 8),
            true,
            AllNuixFeatures
        ),
        NuixSettings.CreateSettings(
            Path.Combine(Nuix7Path, NuixConsoleExe),
            new Version(7, 8),
            true,
            AllNuixFeatures
        ),
        NuixSettings.CreateSettings(
                Path.Combine(Nuix70Path, NuixConsoleExe),
                new Version(7, 0),
                true,
                AllNuixFeatures
            )
            .WithProperty(
                @"\Awarning\Z", //We don't need to catch any java warnings from Nuix7
                SCLSettings.ConnectorsKey,
                NuixSettings.NuixSettingsKey,
                NuixSettings.IgnoreWarningsRegexKey
            )
            .WithProperty(
                @"\Aerror\Z",
                SCLSettings.ConnectorsKey,
                NuixSettings.NuixSettingsKey,
                NuixSettings.IgnoreErrorsRegexKey
            )
    };

    public static readonly string GeneralDataFolder = Path.Combine(
        Directory.GetCurrentDirectory(),
        "IntegrationTest"
    );

    public static readonly string CasePathString = Path.Combine(GeneralDataFolder, "TestCase");

    public static readonly IStep<StringStream> CasePath = Constant(CasePathString);
    public static readonly string OutputFolder = Path.Combine(GeneralDataFolder, "OutputFolder");

    public static readonly string ConcordanceFolder = Path.Combine(
        GeneralDataFolder,
        "ConcordanceFolder"
    );

    public static readonly IStep<StringStream> NRTFolder =
        Constant(Path.Combine(GeneralDataFolder, "NRT"));

    public static readonly IStep<StringStream> MigrationTestCaseFolder =
        Constant(Path.Combine(GeneralDataFolder, "MigrationTest"));

    public static readonly string DataPathString = Path.Combine(
        Directory.GetCurrentDirectory(),
        "AllData",
        "data"
    );

    public static readonly IStep<StringStream> DataPath = Constant(DataPathString);
    public static readonly IStep<Array<StringStream>> DataPaths = Array(DataPathString);

    public static readonly IStep<Array<StringStream>> EncryptedDataPaths = Array(
        Path.Combine(Directory.GetCurrentDirectory(), "AllData", "EncryptedData")
    );

    public static readonly IStep<StringStream> PasswordFilePath = Constant(
        Path.Combine(Directory.GetCurrentDirectory(), "AllData", "Passwords.txt")
    );

    public static readonly IStep<StringStream> DefaultOCRProfilePath = Constant(
        Path.Combine(Directory.GetCurrentDirectory(), "AllData", "DefaultOCRProfile.xml")
    );

    public static readonly IStep<StringStream> DefaultProcessingProfilePath = Constant(
        Path.Combine(Directory.GetCurrentDirectory(), "AllData", "DefaultProcessingProfile.xml")
    );

    public static readonly IStep<StringStream> TestProductionProfilePath = Constant(
        Path.Combine(
            Directory.GetCurrentDirectory(),
            "AllData",
            "IntegrationTestProductionProfile.xml"
        )
    );

    public static readonly IStep<Array<StringStream>> PoemTextImagePaths =
        Array(
            Path.Combine(Directory.GetCurrentDirectory(), "AllData", "PoemText.png"),
            Path.Combine(Directory.GetCurrentDirectory(), "AllData", "PoemText2.png")
        );

    public static readonly string ConcordancePathString = Path.Combine(
        Directory.GetCurrentDirectory(),
        "AllData",
        "Concordance",
        "loadfile.dat"
    );

    public static readonly IStep<StringStream> ConcordancePath = Constant(ConcordancePathString);

    public static readonly IStep<StringStream> MigrationPath = Constant(
        Path.Combine(Directory.GetCurrentDirectory(), "AllData", "MigrationTest.zip")
    );

    public static readonly IStep<Unit> DeleteCaseFolder = new DeleteItem { Path = CasePath };

    public static readonly IStep<Unit> DeleteOutputFolder =
        new DeleteItem { Path = Constant(OutputFolder) };

    public static readonly IStep<Unit> CreateOutputFolder =
        new CreateDirectory { Path = Constant(OutputFolder) };

    public static readonly IStep<Unit> AssertCaseDoesNotExist = new AssertTrue
    {
        Boolean = new Not { Boolean = new NuixDoesCaseExist { CasePath = CasePath } }
    };

    public static readonly IStep<Unit> CreateCase = new NuixCreateCase
    {
        CaseName     = Constant("Integration Test Case"),
        CasePath     = CasePath,
        Investigator = Constant("Mark")
    };

    public static IStep<Unit> AssertFileContains(
        string folderName,
        string fileName,
        string expectedContents)
    {
        return new AssertTrue
        {
            Boolean = new StringContains
            {
                IgnoreCase = Constant(true),
                Substring  = Constant(expectedContents),
                String = new FileRead
                {
                    Path = new PathCombine { Paths = Array(folderName, fileName) }
                }
            }
        };
    }

    public static IStep<Unit> AssertDirectoryDoesNotExist(IStep<StringStream> path)
    {
        return new AssertTrue
        {
            Boolean = new Not { Boolean = new DirectoryExists { Path = path } }
        };
    }

    public static IStep<Unit> AssertCount(
        int expected,
        string searchTerm) => new AssertTrue { Boolean = ItemsCountEqual(expected, searchTerm) };

    public static IStep<bool> ItemsCountEqual(
        int right,
        string searchTerm)
    {
        return new Equals<int>()
        {
            Terms = new ArrayNew<int>()
            {
                Elements = new List<IStep<int>>()
                {
                    Constant(right),
                    new NuixCountItems { SearchTerm = Constant(searchTerm) }
                }
            }
        };
    }

    public static readonly IStep<Unit> OpenCase = new NuixOpenCase() { CasePath = CasePath };

    public static readonly IStep<Unit> AddData = new NuixAddItem
    {
        Custodian = Constant("Mark"), Paths = DataPaths, Container = Constant("New Folder")
    };

    public static ExternalProcessAction HelperAction(string name) => new(
        new ConnectionCommand { Command = name, FunctionDefinition = "", IsHelper = true },
        new ConnectionOutput { Result   = new ConnectionOutputResult { Data = "helper_success" } }
    );
}

}
