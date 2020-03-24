﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using NUnit.Framework;
using Reductech.EDR.Connectors.Nuix.enums;
using Reductech.EDR.Connectors.Nuix.processes;
using Reductech.EDR.Connectors.Nuix.processes.asserts;
using Reductech.EDR.Utilities.Processes.mutable;

namespace Reductech.EDR.Connectors.Nuix.Tests
{
    public class IntegrationTests
    {
        private static readonly INuixProcessSettings NuixSettings = new NuixProcessSettings(true, @"C:\Program Files\Nuix\Nuix 8.2\nuix_console.exe");
        //TODO set these from a config file

        private const string Integration = "Integration";

        private static readonly string GeneralDataFolder = Path.Combine(Directory.GetCurrentDirectory(), "IntegrationTest");

        private static readonly string CasePath = Path.Combine(GeneralDataFolder,  "TestCase");
        private static readonly string OutputFolder = Path.Combine(GeneralDataFolder, "OutputFolder");
        private static readonly string ConcordanceFolder = Path.Combine(GeneralDataFolder, "ConcordanceFolder");
        private static readonly string NRTFolder = Path.Combine(GeneralDataFolder, "NRT");
        private static readonly string MigrationTestCaseFolder = Path.Combine(GeneralDataFolder, "MigrationTest");

        private static readonly string DataPath = Path.Combine(Directory.GetCurrentDirectory(), "AllData", "data");
        private static readonly string PoemTextImagePath = Path.Combine(Directory.GetCurrentDirectory(), "AllData", "PoemText.png");
        private static readonly string ConcordancePath = Path.Combine(Directory.GetCurrentDirectory(), "AllData", "Concordance", "loadfile.dat");
        private static readonly string MigrationPath = Path.Combine(Directory.GetCurrentDirectory(), "AllData", "MigrationTest.zip" );

        private static readonly Process DeleteCaseFolder = new DeleteItem { Path = CasePath};
        private static readonly Process DeleteOutputFolder = new DeleteItem { Path = OutputFolder};
        private static readonly Process CreateOutputFolder = new CreateDirectory { Path = OutputFolder };
        private static readonly Process AssertCaseDoesNotExist = new NuixCaseExists {CasePath = CasePath, ShouldExist = false};
        private static readonly Process CreateCase = new NuixCreateCase {CaseName = "Integration Test Case", CasePath = CasePath, Investigator = "Mark"};
        private static Process AssertCount(int expected, string searchTerm) => new NuixCount {CasePath = CasePath, Minimum = expected, Maximum = expected,  SearchTerm = searchTerm};

        private static readonly Process AddData = new NuixAddItem {CasePath = CasePath, Custodian = "Mark", Path = DataPath, FolderName = "New Folder"};

        public static readonly IReadOnlyCollection<Process> TestProcesses =
            new List<TestSequence>
            {
                //TODO AnnotateDocumentIdList
                //TODO ImportDocumentIds
                //TODO MigrateCase
                
                new TestSequence("Create Case",
                    DeleteCaseFolder,
                    AssertCaseDoesNotExist,
                    CreateCase,
                    new NuixCaseExists
                    {
                        CasePath = CasePath,
                        ShouldExist = true
                    },
                    DeleteCaseFolder),

                new TestSequence("Migrate Case",
                    new DeleteItem {Path = MigrationTestCaseFolder},
                    new Unzip {ArchiveFilePath = MigrationPath, DestinationDirectory = GeneralDataFolder},
                    new AssertFail {Process = new NuixCount { CasePath = MigrationTestCaseFolder, Minimum = 0, SearchTerm = "*"} }, //This should fail because we can't open the case
                    new NuixMigrateCase { CasePath = MigrationTestCaseFolder},
                    new NuixCount { CasePath = MigrationTestCaseFolder, Minimum = 0, Maximum = 0, SearchTerm = "*"},
                    new DeleteItem {Path = MigrationTestCaseFolder}
                    ),

                new TestSequence("Add file to case",
                    DeleteCaseFolder,
                    AssertCaseDoesNotExist,
                    CreateCase,
                    AssertCount(0, "*.txt"),
                    AddData,
                    AssertCount(2, "*.txt"),
                    DeleteCaseFolder
                    ),


                new TestSequence("Add concordance to case",
                    DeleteCaseFolder,
                    AssertCaseDoesNotExist,
                    CreateCase,
                    AssertCount(0, "*.txt"),
                    new NuixAddConcordance{
                        ConcordanceProfileName = "IntegrationTestProfile",
                        ConcordanceDateFormat = "yyyy-MM-dd'T'HH:mm:ss.SSSZ",
                        FilePath = ConcordancePath,
                        Custodian = "Mark",
                        FolderName = "New Folder",
                        CasePath = CasePath
                    },
                    AssertCount(1, "*.txt"),
                    DeleteCaseFolder
                    ),


                new TestSequence("Search and tag",
                    DeleteCaseFolder,
                    CreateCase,
                    AddData,
                    new NuixSearchAndTag
                    {
                        CasePath = CasePath,SearchTerm = "charm",
                        Tag = "charm"
                    },
                    AssertCount(1, "tag:charm"),
                    DeleteCaseFolder
                    
                    ),
                new TestSequence("Perform OCR",
                    DeleteCaseFolder,
                    CreateCase,
                    AssertCount(0, "sheep"),
                    new NuixAddItem {CasePath = CasePath, Custodian = "Mark", Path = PoemTextImagePath, FolderName = "New Folder"},
                    new NuixPerformOCR {CasePath= CasePath, SearchTerm = "*.png"  },
                    AssertCount(1, "sheep"),
                    DeleteCaseFolder
                    ),                
                new TestSequence("Add To Item Set", 
                    DeleteCaseFolder,
                    CreateCase,
                    AddData,
                    new NuixAddToItemSet
                    {
                        CasePath = CasePath,SearchTerm = "charm",
                        ItemSetName = "charmset"
                    },
                    AssertCount(1, "item-set:charmset"),
                    DeleteCaseFolder),

                new TestSequence("Add To Production Set", 
                    DeleteCaseFolder,
                    CreateCase,
                    AddData,
                    new NuixAddToProductionSet
                    {
                        CasePath = CasePath,
                        SearchTerm = "charm",
                        ProductionSetName = "charmset"
                    },
                    AssertCount(1, "production-set:charmset"),
                    DeleteCaseFolder),


                new TestSequence("Generate Print Previews",
                    DeleteCaseFolder,
                    CreateCase,
                    AddData,
                    new NuixAddToProductionSet
                    {
                        CasePath = CasePath,
                        SearchTerm = "*.txt",
                        ProductionSetName = "prodSet"
                    },
                    AssertCount(2, "production-set:prodSet"),
                    new NuixCheckPrintPreviewState
                    {
                        CasePath = CasePath,
                        ProductionSetName = "prodSet",
                        ExpectedState = PrintPreviewState.None
                    },
                    new NuixGeneratePrintPreviews
                    {
                        CasePath = CasePath,
                        ProductionSetName = "prodSet"
                    },
                    new NuixCheckPrintPreviewState
                    {
                        CasePath = CasePath,
                        ProductionSetName = "prodSet",
                        ExpectedState = PrintPreviewState.All
                    },

                    DeleteCaseFolder),

                new TestSequence("Export NRT Report",
                    DeleteCaseFolder,
                    new DeleteItem {Path = NRTFolder },
                    CreateCase,
                    AddData,
                    new NuixAddToProductionSet
                    {
                        CasePath = CasePath,
                        SearchTerm = "*.txt",
                        ProductionSetName = "prodset"
                    },
                    new NuixCreateNRTReport
                    {
                        CasePath = CasePath,
                        NRTPath = @"C:\Program Files\Nuix\Nuix 8.2\user-data\Reports\Case Summary.nrt",
                        OutputFormat = "PDF",
                        LocalResourcesURL = @"C:\Program Files\Nuix\Nuix 8.2\user-data\Reports\Case Summary\Resources\",
                        OutputPath = NRTFolder
                    },
                    new AssertFileContents
                    {
                        FilePath = NRTFolder,
                        ExpectedContents = "PDF-1.4"
                    },
                    new DeleteItem {Path = NRTFolder },
                    DeleteCaseFolder

                    ),

                new TestSequence("Export Concordance",
                    DeleteCaseFolder,
                    new  DeleteItem {Path = ConcordanceFolder },
                    CreateCase,
                    AddData,
                    new NuixAddToProductionSet
                    {
                        CasePath = CasePath,
                        SearchTerm = "charm",
                        ProductionSetName = "charmset"
                    },
                    new NuixExportConcordance
                    {
                        CasePath = CasePath,
                        ProductionSetName = "charmset",
                        ExportPath = ConcordanceFolder
                    },
                    new AssertFileContents
                    {
                        FilePath = ConcordanceFolder + "/loadfile.dat",
                        ExpectedContents = "þDOCIDþþPARENT_DOCIDþþATTACH_DOCIDþþBEGINBATESþþENDBATESþþBEGINGROUPþþENDGROUPþþPAGECOUNTþþITEMPATHþþTEXTPATHþ"
                    },
                    new  DeleteItem {Path = ConcordanceFolder },
                    DeleteCaseFolder
                    ),

                new TestSequence("Remove From Production Set", 
                    DeleteCaseFolder,
                    CreateCase,
                    AddData,
                    new NuixAddToProductionSet
                    {
                        CasePath = CasePath,
                        SearchTerm = "*.txt",
                        ProductionSetName = "fullset"
                    },
                    new NuixRemoveFromProductionSet
                    {
                        CasePath = CasePath,
                        SearchTerm = "Charm",
                        ProductionSetName = "fullset"
                    },
                    AssertCount(1, "production-set:fullset"),
                    DeleteCaseFolder),
                new TestSequence("Create Report",
                    DeleteCaseFolder,
                    DeleteOutputFolder,
                    CreateOutputFolder,
                    CreateCase,
                    AddData,
                    new NuixCreateReport
                    {
                        CasePath = CasePath,
                        OutputFolder = OutputFolder
                    },
                    new AssertFileContents
                    {
                        FilePath = OutputFolder + "/stats.txt", //TODO set these field
                        ExpectedContents = "Mark	type	text/plain	2"
                    },

                    DeleteCaseFolder,
                    DeleteOutputFolder
                ),
                new TestSequence("Create Term List",
                    DeleteCaseFolder,
                    DeleteOutputFolder,
                    CreateOutputFolder,
                    CreateCase,
                    AddData,
                    new NuixCreateTermList
                    {
                        CasePath = CasePath,
                        OutputFolder = OutputFolder
                    },
                    new AssertFileContents
                    {
                        FilePath = OutputFolder + "/Terms.txt", //TODO set these field
                        ExpectedContents = "yellow	2"
                    },

                    DeleteCaseFolder,
                    DeleteOutputFolder
                    ),

                new TestSequence("Extract Entities",
                    DeleteCaseFolder,
                    DeleteOutputFolder,
                    CreateOutputFolder,
                    CreateCase,
                    //Note - we have to add items with a special profile in order to extract entities
                    new NuixAddItem {CasePath = CasePath, Custodian = "Mark", Path = DataPath, FolderName = "New Folder", ProcessingProfileName = "ExtractEntities"},
                    new NuixExtractEntities
                    {
                        CasePath = CasePath,
                        OutputFolder = OutputFolder
                    },
                    new AssertFileContents
                    {
                        FilePath = OutputFolder + "/email.txt",
                        ExpectedContents = "Marianne.Moore@yahoo.com"
                    },

                    DeleteCaseFolder,
                    DeleteOutputFolder
                ),

                new TestSequence("Irregular Items",
                    DeleteCaseFolder,
                    DeleteOutputFolder,
                    CreateOutputFolder,
                    CreateCase,
                    AddData,
                    new NuixCreateIrregularItemsReport
                    {
                        CasePath = CasePath,
                        OutputFolder = OutputFolder
                    },
                    new AssertFileContents
                    {
                        FilePath = OutputFolder + "/Unrecognised.txt",
                        ExpectedContents = "New Folder/data/Theme in Yellow.txt"
                    },
                    new AssertFileContents
                    {
                        FilePath = OutputFolder + "/NeedManualExamination.txt",
                        ExpectedContents = "New Folder/data/Jellyfish.txt"
                    },
                    new AssertFileContents
                    {
                        FilePath = OutputFolder + "/Irregular.txt",
                        ExpectedContents = "Unrecognised	2"
                    },


                    DeleteCaseFolder,
                    DeleteOutputFolder
                ),

                new TestSequence("Get Item Properties",
                    DeleteCaseFolder,
                    DeleteOutputFolder,
                    CreateOutputFolder,
                    CreateCase,
                    AddData,
                    new NuixGetItemProperties
                    {
                        CasePath = CasePath,
                        OutputFolder = OutputFolder,
                        OutputFileName = "ItemProperties",
                        PropertyRegex = ".+",
                        SearchTerm = "*"

                    },
                    new AssertFileContents
                    {
                        FilePath = OutputFolder + "/ItemProperties.txt",
                        ExpectedContents = "Character Set	UTF-8	New Folder/data/Jellyfish.txt"
                    },

                    DeleteCaseFolder,
                    DeleteOutputFolder
                )

            };


        [Test]
        [TestCaseSource(nameof(TestProcesses))]
        [Category(Integration)]
        public async Task Test(Sequence sequence)
        {
            var (isSuccess, _, value, error) = sequence.TryFreeze(NuixSettings);

            Assert.IsTrue(isSuccess, error?.ToString());

            await AssertNoErrors(value.Execute());
        }


        private static async Task AssertNoErrors(IAsyncEnumerable<Result<string>> lines)
        {
            var errors = new List<string>();
            var sb = new StringBuilder();

            await foreach (var (_, isFailure, l, error) in lines)
            {
                if (isFailure)
                    errors.Add(error);
                else sb.AppendLine(l);
            }
            
            CollectionAssert.IsEmpty(errors, sb.ToString());
        }

        internal class TestSequence : Sequence
        {
            public TestSequence(string name, params  Process[] steps)
            {
                Name = name;
                Steps = steps.ToList();
            }

            /// <inheritdoc />
            public override string ToString()
            {
                return Name;
            }

            public string Name { get; }
        }
    }
}
