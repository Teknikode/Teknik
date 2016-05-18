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

        [Option('e', "clean-emails", DefaultValue = false, Required = false, HelpText = "Clean all unused email accounts")]
        public bool CleanEmails { get; set; }

        [Option('g', "clean-git", DefaultValue = false, Required = false, HelpText = "Clean all unused git accounts")]
        public bool CleanGit { get; set; }

        [Option('s', "scan", DefaultValue = false, Required = false, HelpText = "Scan all uploads for viruses")]
        public bool ScanUploads { get; set; }

        [Option('c', "config", Required = false, HelpText = "The path to the teknik config file")]
        public string Config { get; set; }

        [Option('d', "days", DefaultValue = 90, Required = false, HelpText = "Days before the user is deleted")]
        public int DaysBeforeDeletion { get; set; }

        [Option('l', "last-seen", DefaultValue = false, Required = false, HelpText = "Generate a list of user's last seen stats")]
        public bool GenerateLastSeen { get; set; }

        [Option('f', "last-seen-file", Required = false, HelpText = "The file in which you want the last seen stats to be saved to")]
        public string LastSeenFile { get; set; }

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
