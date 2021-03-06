﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Reductech.EDR.Connectors.FileSystem;
using Reductech.EDR.Connectors.Nuix.Steps;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Connectors.Nuix.Steps.Meta.ConnectionObjects;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Steps;
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
    public static IReadOnlyCollection<NuixSettings> NuixSettingsList => new List<NuixSettings>
    {
        new(
            Path.Combine(Nuix9Path, NuixConsoleExe),
            new Version(9, 0),
            true,
            AllNuixFeatures
        ),
        new(
            Path.Combine(Nuix8Path, NuixConsoleExe),
            new Version(8, 8),
            true,
            AllNuixFeatures
        ),
        new(
            Path.Combine(Nuix7Path, NuixConsoleExe),
            new Version(7, 8),
            true,
            AllNuixFeatures
        ),
        new(
            Path.Combine(Nuix70Path, NuixConsoleExe),
            new Version(7, 0),
            true,
            AllNuixFeatures
        )
        {
            IgnoreWarningsRegex =
                @"\Awarning\Z", //We don't need to catch any java warnings from Nuix7
            IgnoreErrorsRegex = @"\Aerror\Z"
        }
    };

    public static readonly string GeneralDataFolder = Path.Combine(
        Directory.GetCurrentDirectory(),
        "IntegrationTest"
    );

    public static readonly string CasePathString = Path.Combine(GeneralDataFolder, "TestCase");

    public static readonly IStep<StringStream> CasePath = Constant(CasePathString);

    public static readonly string OutputFolder = Path.Combine(GeneralDataFolder, "OutputFolder");

    public static readonly IStep<StringStream> NRTFolder =
        Constant(Path.Combine(GeneralDataFolder, "NRT"));

    public static readonly IStep<StringStream> MigrationTestCaseFolder =
        Constant(Path.Combine(GeneralDataFolder, "MigrationTest"));

    public static readonly string DataPathString = Path.Combine(
        Directory.GetCurrentDirectory(),
        "AllData",
        "data"
    );

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

    public static readonly string OpticonPathString = Path.Combine(
        Directory.GetCurrentDirectory(),
        "AllData",
        "Concordance",
        "loadfile.opt"
    );

    public static readonly IStep<StringStream> OpticonPath = Constant(OpticonPathString);

    public static readonly IStep<StringStream> MigrationPath = Constant(
        Path.Combine(Directory.GetCurrentDirectory(), "AllData", "MigrationTest.zip")
    );

    public static readonly IStep<Unit> DeleteCaseFolder = new DeleteItem { Path = CasePath };

    public static readonly IStep<Unit> DeleteOutputFolder =
        new DeleteItem { Path = Constant(OutputFolder) };

    public static readonly IStep<Unit> CreateOutputFolder =
        new DirectoryCreate { Path = Constant(OutputFolder) };

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
        return new Equals<int>
        {
            Terms = new ArrayNew<int>
            {
                Elements = new List<IStep<int>>
                {
                    Constant(right),
                    new NuixCountItems { SearchTerm = Constant(searchTerm) }
                }
            }
        };
    }

    public static IStep<Unit> AssertEquals<T>(IStep<T> expected, IStep<T> actual)
        where T : IComparable<T> => new AssertEqual<T> { Left = expected, Right = actual };

    public static IStep<Unit> AssertPropertyValueEquals<T>(string property, IStep<T> expected)
        where T : IComparable<T> => AssertPropertyValueEquals(
        VariableName.Item.Name,
        property,
        expected
    );

    public static IStep<Unit> AssertPropertyValueEquals<T>(
        string variable,
        string property,
        IStep<T> expected)
        where T : IComparable<T> => new AssertEqual<T>
    {
        Left = expected,
        Right =
            new EntityGetValue<T>
            {
                Entity   = new GetVariable<Entity> { Variable = new VariableName(variable) },
                Property = Constant(property)
            }
    };

    public static readonly IStep<Unit> OpenCase = new NuixOpenCase { CasePath = CasePath };

    public static ExternalProcessAction HelperAction(string name) => new(
        new ConnectionCommand { Command = name, FunctionDefinition = "", IsHelper = true },
        new ConnectionOutput { Result   = new ConnectionOutputResult { Data = "helper_success" } }
    );

    private static List<IStep<StringStream>> TestCasePath => new()
    {
        Constant("AllData\\Cases\\TestCase-"),
        new EntityGetValue<StringStream>
        {
            Entity = new GetSettings(), Property = Constant("Connectors.Nuix.Version")
        },
        Constant(".zip")
    };

    public static IStep<Unit> SetupCase => new Sequence<Unit>
    {
        InitialSteps = new IStep<Unit>[]
        {
            new DeleteItem { Path = CasePath },
            new FileExtract
            {
                ArchiveFilePath = new PathCombine
                {
                    Paths = new ArrayNew<StringStream>
                    {
                        Elements = new List<IStep<StringStream>>
                        {
                            new StringJoin
                            {
                                Strings = new ArrayNew<StringStream>
                                {
                                    Elements = TestCasePath
                                }
                            }
                        }
                    }
                },
                Destination = Constant(GeneralDataFolder)
            }
        },
        FinalStep = new NuixOpenCase { CasePath = CasePath }
    };

    public static IStep<Unit> CleanupCase => new Sequence<Unit>
    {
        InitialSteps = new IStep<Unit>[] { new NuixCloseConnection() },
        FinalStep    = DeleteCaseFolder
    };
}

}
