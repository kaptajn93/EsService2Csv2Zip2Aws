using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommandLine;
using CommandLine.Text;

namespace Es2Csv
{
    // Define a class to receive parsed values
    public class Options
    {
        [Option('c', "configurationfile", Required = true, HelpText = "Input file to be processed.")]
        public string ConfigurationFile { get; set; }

        [Option('t', "when to run service", Required = true, HelpText = "At what time the service should run")]
        public string WhenToRunService { get; set; }

        //"you must execute with an attatched filepath for your configurationfile:\nEs2Csv -c \"your-config-file-path\"")]

        [ParserState]
        public IParserState LastParserState { get; set; }

        [HelpOption]
        public string GetUsage()
        {
            return HelpText.AutoBuild(this,
              (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
        }
    }

}
