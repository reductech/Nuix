﻿using System;
using System.Linq;
using Orchestration;

namespace NuixClientConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            //TODO add other methods from processes

            var methods = typeof(YamlRunner).GetMethods()
                    .Where(x=>x.IsStatic);

            var lines = ConsoleView.Run(args, methods);

            var enumerator = lines.GetAsyncEnumerator();

            try
            {
                while (true)
                {
                    var nextTask = enumerator.MoveNextAsync().AsTask();
                    var next = nextTask.Result;
                    if (!next)
                        break;
                    Console.WriteLine(enumerator.Current);
                }
            }
#pragma warning disable CA1031 // Do not catch general exception types
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
#pragma warning restore CA1031 // Do not catch general exception types
        }
    }
}
