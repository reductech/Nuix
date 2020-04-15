﻿#RunOCR

requiredNuixVersion = '7.6'
if Gem::Version.new(NUIX_VERSION) < Gem::Version.new(requiredNuixVersion)
	puts "Nuix Version is #{NUIX_VERSION} but #{requiredNuixVersion} is required"
	exit
end

requiredFeatures = Array['OCR_PROCESSING']
requiredFeatures.each do |feature|
	if !utilities.getLicence().hasFeature(feature)
		puts "Nuix Feature #{feature} is required but not available."
		exit
	end
end

require 'optparse'
params = {}
OptionParser.new do |opts|
	opts.on('--pathArg0 ARG') do |o| params[:pathArg0] = o end
	opts.on('--searchTermArg0 ARG') do |o| params[:searchTermArg0] = o end
	opts.on('--ocrProfileArg0 [ARG]') do |o| params[:ocrProfileArg0] = o end
end.parse!

puts params


def RunOCR(utilities,pathArg,searchTermArg,ocrProfileArg)

    the_case = utilities.case_factory.open(pathArg)

    searchTerm = searchTermArg    
    items = the_case.searchUnsorted(searchTerm).to_a

    puts "Running OCR on #{items.length} items"
    
    processor = utilities.createOcrProcessor()

    if ocrProfileArg != nil
        ocrProfileStore = the_case.getOcrProfileStore()
        puts "Got profile store"

        profile = ocrProfileStore.getProfile(ocrProfileArg)

        if profile != nil
            processor.process(items, profile)
            puts "Items Processed"
        else
            puts "Could not find profile '#{ocrProfileArg}'"
            exit
        end
    else
        processor.process(items)
        puts "Items Processed"
    end    
    the_case.close
end



RunOCR(utilities, params[:pathArg0], params[:searchTermArg0], params[:ocrProfileArg0])
puts '--Script Completed Successfully--'