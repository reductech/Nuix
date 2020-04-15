﻿#CreateTermList

requiredNuixVersion = '5.0'
if Gem::Version.new(NUIX_VERSION) < Gem::Version.new(requiredNuixVersion)
	puts "Nuix Version is #{NUIX_VERSION} but #{requiredNuixVersion} is required"
	exit
end

require 'optparse'
params = {}
OptionParser.new do |opts|
	opts.on('--casePathArg0 ARG') do |o| params[:casePathArg0] = o end
end.parse!

puts params


def CreateTermList(utilities,casePathArg)

    the_case = utilities.case_factory.open(casePathArg)

    puts "Generating Report:"   
    caseStatistics = the_case.getStatistics()
    termStatistics = caseStatistics.getTermStatistics("", {"sort" => "on", "deduplicate" => "md5"}) #for some reason this takes strings rather than symbols
    #todo terms per custodian
    puts "#{termStatistics.length} terms"

    text = "Term\tCount"

    termStatistics.each do |term, count|
        text << "\n#{term}\t#{count}"
    end

    the_case.close
    return text
end



result0 = CreateTermList(utilities, params[:casePathArg0])
puts "--Final Result: #{result0}"