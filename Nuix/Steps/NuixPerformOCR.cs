using System;
using System.Collections.Generic;
using CSharpFunctionalExtensions;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Steps
{

/// <summary>
/// Performs optical character recognition on files in a NUIX case.
/// </summary>
public sealed class NuixPerformOCRStepFactory : RubyScriptStepFactory<NuixPerformOCR, Unit>
{
    private NuixPerformOCRStepFactory() { }

    /// <summary>
    /// The instance.
    /// </summary>
    public static RubyScriptStepFactory<NuixPerformOCR, Unit> Instance { get; } =
        new NuixPerformOCRStepFactory();

    /// <inheritdoc />`,
    public override Version RequiredNuixVersion => new(7, 6);

    /// <inheritdoc />
    public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } =
        new List<NuixFeature>() { NuixFeature.OCR_PROCESSING };

    /// <inheritdoc />
    public override string FunctionName => "RunOCR";

    /// <inheritdoc />
    public override string RubyFunctionText => @"
    searchTerm = searchTermArg
    items = $currentCase.searchUnsorted(searchTerm).to_a

    log ""Running OCR on #{items.length} items""

    processor = $utilities.createOcrProcessor() #since Nuix 7.0 but seems to work with earlier versions anyway

    if ocrProfileArg != nil
        ocrOptions = {:ocrProfileName => ocrProfileArg}
        processor.process(items, ocrOptions)
        log ""Items Processed""
    elsif ocrProfilePathArg != nil
        profileBuilder = $utilities.getOcrProfileBuilder()
        profile = profileBuilder.load(ocrProfilePathArg)

        if profile == nil
            log ""Could not find processing profile at #{ocrProfilePathArg}""
            exit
        end

        processor.setOcrProfileObject(profile)
    else
        processor.process(items)
        log ""Items Processed""
    end";
}

/// <summary>
/// Performs optical character recognition on files in a NUIX case.
/// </summary>
public sealed class NuixPerformOCR : RubyCaseScriptStepBase<Unit>
{
    /// <inheritdoc />
    public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory =>
        NuixPerformOCRStepFactory.Instance;

    private const string DefaultSearchTerm =
        "NOT flag:encrypted AND ((mime-type:application/pdf AND NOT content:*) OR (mime-type:image/* AND ( flag:text_not_indexed OR content:( NOT * ) )))";

    /// <summary>
    /// The term to use for searching for files to OCR.
    /// </summary>
    [StepProperty(1)]
    [DefaultValueExplanation(DefaultSearchTerm)]
    [RubyArgument("searchTermArg")]
    [Alias("Search")]
    public IStep<StringStream> SearchTerm { get; set; } =
        new StringConstant(DefaultSearchTerm);

    /// <summary>
    /// The name of the OCR profile to use.
    /// This cannot be set at the same time as OCRProfilePath.
    /// </summary>
    [StepProperty(2)]
    [DefaultValueExplanation("The default profile will be used.")]
    [Example("MyOcrProfile")]
    [RubyArgument("ocrProfileArg")]
    [Alias("Profile")]
    public IStep<StringStream>? OCRProfileName { get; set; }

    /// <summary>
    /// Path to the OCR profile to use.
    /// This cannot be set at the same times as OCRProfileName.
    /// </summary>
    [StepProperty(3)]
    [RequiredVersion("Nuix", "7.6")]
    [DefaultValueExplanation("The Default profile will be used.")]
    [Example("C:\\Profiles\\MyProfile.xml")]
    [RubyArgument("ocrProfilePathArg")]
    [Alias("ProfilePath")]
    public IStep<StringStream>? OCRProfilePath { get; set; }

    /// <inheritdoc />
    public override Result<Unit, IError> VerifyThis(ISettings settings)
    {
        if (OCRProfileName != null && OCRProfilePath != null)
            return new SingleError(
                new StepErrorLocation(this),
                ErrorCode.ConflictingParameters,
                nameof(OCRProfileName),
                nameof(OCRProfilePath)
            );

        return base.VerifyThis(settings);
    }
}

}