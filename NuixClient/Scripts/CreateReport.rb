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

    puts "Generating Report:"

    fields = {
    all: "",
    email: "kind:email",
    calendar: "kind:calendar",
    contact: "kind:contact",
    document: "kind:document",
    spreadsheet: "kind:spreadsheet",
    presentation: "kind:presentation",
    drawing: "kind:drawing",
    otherDocument: "kind:other-document",
    image: "kind:image",
    multimedia: "kind:multimedia",
    database: "kind:database",
    container: "kind:container",
    system: "kind:system",
    noData: "kind:no-data",
    unrecognised: "kind:unrecognised",
    log: "kind:log",
    chatConversation: "kind:chat-conversation",
    chatMessage: "kind:chat-message",
    deleted: "flag:deleted",
    empty: "mime-type:application/x-empty"
    }

    fields.each do |key, value|
        items = the_case.search(value, {})
        puts "#{key.to_s}: #{items.length}"

    end

    the_case.close
    puts "Case Closed"
    
end