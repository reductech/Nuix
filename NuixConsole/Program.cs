﻿using System;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using InstantConsole;
using NuixClient;
using NuixClient.processes;
using Processes;

namespace NuixConsole
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.OutputEncoding = Encoding.UTF8;

            var rubyScriptProcessAssembly = Assembly.GetAssembly(typeof(RubyScriptProcess));
            Debug.Assert(rubyScriptProcessAssembly != null, nameof(rubyScriptProcessAssembly) + " != null");

            var useDongleString =  ConfigurationManager.AppSettings["NuixUseDongle"];
            var nuixExeConsolePath = ConfigurationManager.AppSettings["NuixExeConsolePath"];

            if (!bool.TryParse(useDongleString, out var useDongle))
            {
                Console.WriteLine("Please set the property 'NuixUseDongle' in the settings file");
                return;
            }

            if (string.IsNullOrWhiteSpace(nuixExeConsolePath))
            {
                Console.WriteLine("Please set the property 'NuixExeConsolePath' in the settings file");
                return;
            }

            var nuixProcessSettings = new NuixProcessSettings(useDongle, nuixExeConsolePath);

            var methods = typeof(YamlRunner).GetMethods()
                .Where(m=>m.DeclaringType != typeof(object))
                    .Select(x=>x.AsRunnable(new YamlRunner(nuixProcessSettings)))
                    .Concat(rubyScriptProcessAssembly.GetTypes()
                        .Where(t=> typeof(RubyScriptProcess).IsAssignableFrom(t))
                        .Where(t=>!t.IsAbstract)
                        .Select(x=> new NuixProcessWrapper(x, nuixProcessSettings) )
                    ).ToList();

            ConsoleView.Run(args, methods);
        }
    }
}
