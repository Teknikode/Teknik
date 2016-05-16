using CommandLine;
using CommandLine.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServerMaint
{
    public class ArgumentOptions
    {
        [ParserState]
        public IParserState LastParserState { get; set; }

        [Option('a', "all", DefaultValue = false, Required = false, HelpText = "Run All Processes")]
        public bool RunAll { get; set; }

        [Option('u', "clean-users", DefaultValue = false, Required = false, HelpText = "Clean all inactive users")]
        public bool CleanUsers { get; set; }

        [Option('s', "scan", DefaultValue = false, Required = false, HelpText = "Scan all uploads for viruses")]
        public bool ScanUploads { get; set; }

        [Option('c', "config", Required = false, HelpText = "The path to the teknik config file")]
        public string Config { get; set; }

        [Option('d', "days", DefaultValue = 90, Required = false, HelpText = "Days before the user is deleted")]
        public int DaysBeforeDeletion { get; set; }

        [Option('e', "emails", DefaultValue = 2, Required = false, HelpText = "Number of emails to send before deletion")]
        public int EmailsToSend { get; set; }

        // Omitting long name, default --verbose
        [Option(HelpText = "Prints all messages to standard output.")]
        public bool Verbose { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            var help = new HelpText();

            // ...
            if (this.LastParserState.Errors.Any())
            {
                var errors = help.RenderParsingErrorsText(this, 2); // indent with two spaces

                if (!string.IsNullOrEmpty(errors))
                {
                    help.AddPreOptionsLine(string.Concat(Environment.NewLine, "ERROR(S):"));
                    help.AddPreOptionsLine(errors);
                }
            }

            // ...
            return help;
        }

    }
}
