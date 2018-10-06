﻿using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using TestRunner.Infrastructure;
using TestRunner.Runners;
using TestRunner.Events;
using EventHandler = TestRunner.Events.EventHandler;

namespace TestRunner.Program
{
    static class Program
    {

        static readonly string ProgramPath = Assembly.GetExecutingAssembly().Location;
        static readonly string ProgramName = Path.GetFileName(ProgramPath);


        [STAThread]
        static void Main(string[] args)
        {
            //
            // Exit the program immediately, killing off any background threads
            //
            Environment.Exit(Main2(args));
        }


        [System.Diagnostics.CodeAnalysis.SuppressMessage(
            "Microsoft.Design",
            "CA1031:DoNotCatchGeneralExceptionTypes",
            Justification = "Required to handle unexpected exceptions")]
        static int Main2(string[] args)
        {
            try
            {
                return Main3(args);
            }

            //
            // Handle user-facing errors
            //
            catch (UserException ue)
            {
                EventHandlers.First.Handle(new ProgramUserErrorEvent() { Exception = ue });
                return 1;
            }

            //
            // Handle internal errors
            //
            catch (Exception e)
            {
                EventHandlers.First.Handle(new ProgramInternalErrorEvent() { Exception = e });
                return 1;
            }
        }


        static int Main3(string[] args)
        {
            EventHandlers.Append(new TimingEventHandler());
            EventHandlers.Append(new TestContextEventHandler());

            //
            // Parse arguments
            //
            ArgumentParser.Parse(args);

            switch(ArgumentParser.OutputFormat)
            {
                case OutputFormat.Human:
                    EventHandlers.Append(new HumanOutputEventHandler());
                    break;
                default:
                    throw new Exception($"Unrecognised <outputformat> from parser {ArgumentParser.OutputFormat}");
            }

            if (!ArgumentParser.Success)
            {
                EventHandlers.First.Handle(new ProgramUsageEvent() { Lines = ArgumentParser.GetUsage() });
                throw new UserException(ArgumentParser.ErrorMessage);
            }

            if (ArgumentParser.Help)
            {
                EventHandlers.First.Handle(new ProgramUsageEvent() { Lines = ArgumentParser.GetUsage() });
                return 0;
            }

            //
            // Parent process: Print the program banner and invoke TestRunner --inproc child processes for each
            // <testfile> specified on the command line
            //
            if (!ArgumentParser.InProc)
            {
                Banner();
                bool success = true;
                foreach (var testFile in ArgumentParser.TestFiles)
                {
                    var exitCode = 
                        ProcessExtensions.ExecuteDotnet(
                            ProgramPath,
                            "--inproc \"" + testFile + "\"")
                        .ExitCode;

                    if (exitCode != 0) success = false;
                }
                return success ? 0 : 1;
            }

            //
            // Child process: Run the tests in the specified <testfile>
            //
            else
            {
                return TestAssemblyRunner.Run(ArgumentParser.TestFiles[0]) ? 0 : 1;
            }
        }


        /// <summary>
        /// Print program information
        /// </summary>
        ///
        static void Banner()
        {
            var name = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductName;
            var version = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).ProductVersion;
            var copyright = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).LegalCopyright;

            EventHandlers.First.Handle(
                new ProgramBannerEvent() {
                    Lines = new[]{
                        $"{name} v{version}",
                        copyright,
                    }
                });
        }

    }
}
