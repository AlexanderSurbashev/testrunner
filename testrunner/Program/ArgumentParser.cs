﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using TestRunner.Infrastructure;

namespace TestRunner.Program
{

    /// <summary>
    /// Command line argument parser
    /// </summary>
    ///
    static class ArgumentParser
    {
        
        /// <summary>
        /// Were the command line arguments valid?
        /// </summary>
        ///
        static public bool Success
        {
            get;
            private set;
        }


        /// <summary>
        /// User-facing error message if not <see cref="Success"/>
        /// </summary>
        ///
        static public string ErrorMessage
        {
            get;
            private set;
        }


        static public bool InProc
        {
            get;
            private set;
        }


        /// <summary>
        /// Path(s) to test assemblies listed on the command line
        /// </summary>
        ///
        static public IList<string> TestFiles
        {
            get
            {
                return new ReadOnlyCollection<string>(_testFiles);
            }
        }

        static List<string> _testFiles;


        /// <summary>
        /// Produce user-facing command line usage information
        /// </summary>
        ///
        static public string[] GetUsage()
        {
            var fileName = Path.GetFileName(Assembly.GetExecutingAssembly().Location);
            bool isUnix = new[] { PlatformID.Unix, PlatformID.MacOSX }.Contains(Environment.OSVersion.Platform);

            var shellPrefix =
                isUnix
                    ? "$"
                    : "C:\\>";

            var examplePath =
                isUnix
                    ? "/path/to/"
                    : "C:\\path\\to\\";

            return
                new[] {
                    $"SYNOPSIS",
                    $"",
                    $"    {fileName} <testfile>...",
                    $"",
                    $"OPTIONS",
                    $"",
                    $"    <testfile>",
                    $"        Path(s) to one or more .NET assembly file(s) containg tests",
                    $"",
                    $"EXAMPLES",
                    $"",
                    $"    {shellPrefix} {fileName} TestAssembly.dll AnotherTestAssembly.dll",
                    $"",
                    $"    {shellPrefix} {fileName} {examplePath}TestAssembly.dll {examplePath}AnotherTestAssembly.dll",
                    };
        }


        /// <summary>
        /// Parse command line arguments
        /// </summary>
        ///
        static public void Parse(string[] args)
        {
            Success = false;
            ErrorMessage = "";
            InProc = false;
            _testFiles = new List<string>();
            Parse(new Queue<string>(args));
        }


        static void Parse(Queue<string> args)
        {
            for (;;)
            {
                if (args.Count == 0) break;
                if (!args.Peek().StartsWith("--", StringComparison.Ordinal)) break;
                var s = args.Dequeue();
                switch (s)
                {
                    case "--inproc":
                        InProc = true;
                        break;
                    default:
                        ErrorMessage = "Unrecognised switch " + s;
                        return;
                }
            }

            while (args.Count > 0)
            {
                _testFiles.Add(args.Dequeue());
            }

            if (TestFiles.Count == 0)
            {
                ErrorMessage = "No <testfile>s specified";
                return;
            }

            if (InProc && TestFiles.Count > 1)
            {
                ErrorMessage = "Only one <testfile> allowed when --inproc";
                return;
            }

            Success = true;
        }

    }
}
