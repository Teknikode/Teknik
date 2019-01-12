using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace Teknik.ServiceWorker
{
    public class ArgumentOptions
    {
        [Option('a', "all", Default = false, Required = false, HelpText = "Run All Processes")]
        public bool RunAll { get; set; }

        [Option('c', "config", Required = false, HelpText = "The path to the teknik config file")]
        public string Config { get; set; }

        [Option('s', "scan", Default = false, Required = false, HelpText = "Scan all uploads for viruses")]
        public bool ScanUploads { get; set; }

        [Option('m', "migrate", Default = false, Required = false, HelpText = "Migrate everything")]
        public bool Migrate { get; set; }

        // Omitting long name, default --verbose
        [Option(HelpText = "Prints all messages to standard output.")]
        public bool Verbose { get; set; }
    }
}
