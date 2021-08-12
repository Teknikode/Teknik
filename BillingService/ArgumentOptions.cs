using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace Teknik.BillingService
{
    public class ArgumentOptions
    {
        [Option('s', "sync", Default = false, Required = false, HelpText = "Syncs the current subscriptions with the invoice system")]
        public bool SyncSubscriptions { get; set; }

        [Option('c', "config", Required = false, HelpText = "The path to the teknik config file")]
        public string Config { get; set; }

        // Omitting long name, default --verbose
        [Option(HelpText = "Prints all messages to standard output.")]
        public bool Verbose { get; set; }
    }
}
