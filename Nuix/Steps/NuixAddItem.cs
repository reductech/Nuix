﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using CSharpFunctionalExtensions;
using Reductech.EDR.Connectors.Nuix.Steps.Meta;
using Reductech.EDR.Core;
using Reductech.EDR.Core.Attributes;
using Reductech.EDR.Core.Entities;
using Reductech.EDR.Core.Internal;
using Reductech.EDR.Core.Internal.Errors;
using Reductech.EDR.Core.Util;

namespace Reductech.EDR.Connectors.Nuix.Steps
{
    /// <summary>
    /// Adds a file or directory to a Nuix Case.
    /// </summary>
    public sealed class NuixAddItemStepFactory : RubyScriptStepFactory<NuixAddItem, Unit>
    {
        private NuixAddItemStepFactory() { }

        /// <summary>
        /// The instance.
        /// </summary>
        public static RubyScriptStepFactory<NuixAddItem, Unit> Instance { get; } = new NuixAddItemStepFactory();

        /// <inheritdoc />
        public override Version RequiredNuixVersion => new Version(3, 2);

        /// <inheritdoc />
        public override IReadOnlyCollection<NuixFeature> RequiredFeatures { get; } = new List<NuixFeature> { NuixFeature.CASE_CREATION };


        /// <inheritdoc />
        public override string FunctionName => "AddToCase";

        /// <inheritdoc />
        public override string RubyFunctionText => @"

    ds = args[""datastream""]

    the_case = $utilities.case_factory.open(pathArg)
    processor = the_case.create_processor

    #Read special mime type settings from data stream
    if ds != nil
        
        log ""Mime Type Data stream reading started""
        mimeTypes = []

        while !ds.closed? or !ds.empty?
            data = ds.pop
            break if ds.closed? and data.nil?
            mimeTypes << data
        end
        log ""Mime Type Data stream reading finished (#{mimeTypes.count} elements)""


        version_mimes = []
		$utilities.getItemTypeUtility().getAllTypes().each do |mime|
			version_mimes << mime.to_s
		end


        mimeTypes.each do |mime_type|
            mimeTypeString = mime_type[""mimeType""].to_s
            if (version_mimes.include?(mimeTypeString) == true)
                mime_type.delete(""mimeType"") #remove this value from the hash as it isn't part of the settings
                nuix_processor.setMimeTypeProcessingSettings(mimeTypeString, mime_type)
            end        
        end
    end


    #This only works in 7.6 or later
    if processingProfileNameArg != nil
        processor.setProcessingProfile(processingProfileNameArg)
    elsif processingProfilePathArg != nil
        profileBuilder = $utilities.getProcessingProfileBuilder()
        profileBuilder.load(processingProfilePathArg)
        profile = profileBuilder.build()

        if profile == nil
            raise ""Could not find processing profile at #{processingProfilePathArg}""
            exit
        end

        processor.setProcessingProfileObject(profile)
    end

    if processingSettingsArg != nil
        processor.setProcessingSettings(processingSettingsArg)
    end

    if parallelProcessingSettingsArg != nil
        processor.setParallelProcessingSettings(parallelProcessingSettingsArg)
    end


#This only works in 7.2 or later
    if passwordFilePathArg != nil
        lines = File.read(passwordFilePathArg, mode: 'r:bom|utf-8').split

        passwords = lines.map {|p| p.chars.to_java(:char)}
        listName = 'MyPasswordList'

        processor.addPasswordList(listName, passwords)
        processor.setPasswordDiscoverySettings({'mode' => ""word-list"", 'word-list' => listName })
    end


    folder = processor.new_evidence_container(folderNameArg)

    folder.description = folderDescriptionArg if folderDescriptionArg != nil
    folder.initial_custodian = folderCustodianArg

    filePathsArgs.each do |path|
        folder.add_file(path)
        log ""Added Evidence from Path: #{path} to Container: #{folderNameArg}""
    end

    
    folder.save

    log 'Adding items'
    processor.process
    log 'Items added'
    the_case.close";

    }

    /// <summary>
    /// Adds a file or directory to a Nuix Case.
    /// </summary>
    public sealed class NuixAddItem : RubyScriptStepBase<Unit>
    {
        /// <inheritdoc />
        public override IRubyScriptStepFactory<Unit> RubyScriptStepFactory => NuixAddItemStepFactory.Instance;



        /// <summary>
        /// The path to the case.
        /// </summary>
        [Required]
        [StepProperty]
        [Example("C:/Cases/MyCase")]
        [RubyArgument("pathArg", 1)]
        public IStep<string> CasePath { get; set; } = null!;

        /// <summary>
        /// The name of the folder to create.
        /// </summary>
        [Required]
        [StepProperty]
        [RubyArgument("folderNameArg", 2)]
        public IStep<string> FolderName { get; set; } = null!;

        /// <summary>
        /// The description of the new folder.
        /// </summary>
        [StepProperty]
        [RubyArgument("folderDescriptionArg", 3)]
        [DefaultValueExplanation("No Description")]
        public IStep<string>? Description { get; set; }

        /// <summary>
        /// The custodian to assign to the new folder.
        /// </summary>
        [Required]
        [StepProperty]
        [RubyArgument("folderCustodianArg", 4)]
        public IStep<string> Custodian { get; set; } = null!;



        /// <summary>
        /// The path of the file or directory to add to the case.
        /// </summary>
        [Required]
        [StepProperty]
        [Example("C:/Data/File.txt")]
        [RubyArgument("filePathsArgs", 5)]
        public IStep<List<string>> Paths { get; set; } = null!;

        /// <summary>
        /// The name of the Processing profile to use.
        /// </summary>

        [RequiredVersion("Nuix", "7.6")]
        [StepProperty]
        [Example("MyProcessingProfile")]
        [DefaultValueExplanation("The default processing profile will be used.")]
        [RubyArgument("processingProfileNameArg", 6)]
        public IStep<string>? ProcessingProfileName { get; set; }

        /// <summary>
        /// The path to the Processing profile to use
        /// </summary>
        [RequiredVersion("Nuix", "7.6")]
        [StepProperty]
        [Example("C:/Profiles/MyProcessingProfile.xml")]
        [DefaultValueExplanation("The default processing profile will be used.")]
        [RubyArgument("processingProfilePathArg", 7)]
        public IStep<string>? ProcessingProfilePath { get; set; }

        /// <summary>
        /// Sets the processing settings to use.
        /// These settings correspond to the same settings in the desktop application, however the user's preferences are not used to derive the defaults.
        /// </summary>
        [StepProperty]
        [DefaultValueExplanation("Processing settings will not be changed")]
        [RubyArgument("processingSettingsArg", 8)]
        public IStep<Core.Entities.Entity>? ProcessingSettings { get; set; }

        /// <summary>
        /// Sets the parallel processing settings to use.
        /// These settings correspond to the same settings in the desktop application, however the user's preferences are not used to derive the defaults.
        /// </summary>
        [StepProperty]
        [DefaultValueExplanation("Parallel processing settings will not be changed")]
        [RubyArgument("parallelProcessingSettingsArg", 8)]
        public IStep<Core.Entities.Entity>? ParallelProcessingSettings { get; set; }


        /// <summary>
        /// The path of a file containing passwords to use for decryption.
        /// </summary>
        [RequiredVersion("Nuix", "7.6")]
        [StepProperty]
        [Example("C:/Data/Passwords.txt")]
        [RubyArgument("passwordFilePathArg", 9)]
        [DefaultValueExplanation("Do not attempt decryption")]
        public IStep<string>? PasswordFilePath { get; set; }


        /// <summary>
        /// Special settings for individual mime types.
        /// Should have a 'mime_type' property and then any other special properties.
        /// </summary>
        [RequiredVersion("Nuix", "8.2")]
        [StepProperty]
        [RubyArgument("mimeTypeDataStreamArg", 10)]
        [DefaultValueExplanation("Use default settings for all MIME types")]
        public IStep<EntityStream>? MimeTypeSettings { get; set; }

        /// <inheritdoc />
        public override Result<Unit, IError> VerifyThis(ISettings settings)
        {
            if (ProcessingProfileName != null && ProcessingProfilePath != null)
            {
                return new SingleError(
                    $"Only one of {nameof(ProcessingProfileName)} and {nameof(ProcessingProfilePath)} may be set.",
                    ErrorCode.ConflictingParameters,
                    new StepErrorLocation(this));
            }

            return base.VerifyThis(settings);
        }
    }
}