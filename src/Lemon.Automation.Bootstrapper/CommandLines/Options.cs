using CommandLine;

namespace Lemon.Automation.Bootstrapper.CommandLines
{
    public class Options
    {
        [Option('a', "app", Required = true, HelpText = "Apps to start.")]
        public required IEnumerable<string> Apps { get; set; }

        [Option('b', "isboot", Default = false, Required = false)]
        public bool? IsBootstrapper { get; set; }
    }
}
