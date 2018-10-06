using Castle.MicroKernel.Registration;
using Castle.Windsor;
using CommandLine;
using DirectoryHelperLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DirectoryHelper
{
    class Program
    {
        public static class ActionType
        {
            public const string All = "all";
            public const string Cs = "cs";
            public const string Reversed1 = "reversed1";
            public const string Reversed2 = "reversed2";
        }
        public class Options
        {

            [Option('p', "path", Required = true, HelpText = "Path to start directory")]
            public string Path { get; set; }

            [Option('a', "action", Required = false, HelpText = "DEFAULT: all. Action type (all: all files | cs: *.cs files | reversed1: all files with reversed words | reversed1: all files with reversed chars).")]
            public string Action { get; set; } = ActionType.All;

            [Option('r', "results", Required = false, HelpText = "DEFAULT: results.txt. Path to result file.")]
            public string Results { get; set; } = "results.txt";
        }
        static void Main(string[] args)
        {
            CommandLine.Parser.Default.ParseArguments<Options>(args)
              .WithParsed<Options>(opts => RunWithOptions(opts));
        }

        private static void RunWithOptions(Options opts)
        {
            Task directoryProcessingTask = null;

            var container = new WindsorContainer();
            container.Register(Component.For<DirectoryProcessor>()
                .DependsOn(Dependency.OnValue("ReportFile", opts.Results))
                );

            if (string.Compare(opts.Action, ActionType.All, true) == 0)
                container.Register(Component.For<IFileProcessor>().ImplementedBy<SimpleFileProcessor>());
            else if (string.Compare(opts.Action, ActionType.Cs, true) == 0)
                container.Register(Component.For<IFileProcessor>().ImplementedBy<CsFileProcessor>());
            else if (string.Compare(opts.Action, ActionType.Reversed1, true) == 0)
                container.Register(Component.For<IFileProcessor>().ImplementedBy<ReverseFileProcessor1>());
            else if (string.Compare(opts.Action, ActionType.Reversed2, true) == 0)
                container.Register(Component.For<IFileProcessor>().ImplementedBy<ReverseFileProcessor2>());

            DirectoryProcessor client = null;
            try
            {
                client = container.Resolve<DirectoryProcessor>();
                CancellationTokenSource cts = new CancellationTokenSource();
                directoryProcessingTask = client.ProcessDirectoryAsync(opts.Path, cts.Token);

                if (directoryProcessingTask != null)
                {
                    Console.WriteLine("Press ESC to exit.");
                    CheckForCancelationAsync(cts);
                    try
                    {
                        directoryProcessingTask.Wait();
                        Console.WriteLine("Task completed successfully.");
                    }
                    catch (AggregateException ex)
                    {
                        foreach (var iex in ex.InnerExceptions)
                            Console.WriteLine(iex.Message);
                    }
                    finally
                    {
                        cts.Dispose();
                    }
                }
            }
            catch (Castle.MicroKernel.Handlers.HandlerException)
            {
                Console.WriteLine("Unable to handle request. Did you provide a valid action type?");
            }
            finally
            {
                container.Release(client);
            }
            //Console.ReadLine();
        }

        private static void CheckForCancelationAsync(CancellationTokenSource cts)
        {
            Task.Run(() =>
            {
                ConsoleKeyInfo key = new ConsoleKeyInfo();
                while (cts != null && !cts.IsCancellationRequested)
                {
                    while (!Console.KeyAvailable && cts != null && !cts.IsCancellationRequested)
                    {
                        Thread.Sleep(10);
                    }
                    if (Console.KeyAvailable)
                    {
                        key = Console.ReadKey(true);
                        if (key.Key == ConsoleKey.Escape)
                        {
                            cts.Cancel();
                            break;
                        }
                    }
                }

            }, cts.Token);
        }
        private static void SaveResult(string file)
        {
            Console.WriteLine(file);
        }
    }
}
