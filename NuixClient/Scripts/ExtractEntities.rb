require 'optparse'

hash_options = {}
OptionParser.new do |opts|
  opts.banner = "Usage: your_app [options]"
  opts.on('-p [ARG]', '--path [ARG]', "Case Path") do |v|
    hash_options[:pathArg] = v
  end 
  opts.on('-h', '--help', 'Display this help') do 
    puts opts
    exit
  end

end.parse!

requiredArguments = [:pathArg]

unless requiredArguments.all? {|a| hash_options[a] != nil}
    puts "Missing arguments #{(requiredArguments.select {|a| hash_options[a] == nil}).to_s}"


else
    puts "Opening Case"
    
    the_case = utilities.case_factory.open(hash_options[:pathArg])

    puts "Extracting Entities:"

    entityTypes = the_case.getAllEntityTypes()

    results = Hash.new { |h, k| h[k] = Hash.new { [] } }

    if entityTypes.length > 0
        allItems = the_case.searchUnsorted("named-entities:*")    

        allItems.each do |i|            
            entityTypes.each do |et|
                entities = i.getEntities(et)
                entities.each do |e|
                   results[et][e] =  results[et][e].push(i.getGuid())
                end
            end
        end

        puts "Found entities for #{allItems.length} items"

        puts "OutputEntities:type\tvalue\tcount" #The headers for the entities file

        results.each do |et, values|
            totalCount = values.map{|x,y| y.length}.reduce(:+)
            puts "OutputEntities:#{et}\t*\t#{totalCount}" #The total count for entities of this type
            puts "Output#{et}:value\tguid" #The header for this types's file
            values.each do |value, guids|
                puts "OutputEntities:#{et}\t#{value}\t#{guids.length}" #The row in the entities file
                guids.each do |guid|                
                    puts "Output#{et}:#{value}\t#{guid}" #The row in this entity type file
                end				                
            end
        end
    else
        puts "Case has no entities"
    end

    the_case.close
    puts "Case Closed"
    
end