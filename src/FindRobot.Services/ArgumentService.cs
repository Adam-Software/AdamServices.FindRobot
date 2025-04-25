using CommandLine;

namespace FindRobot.Services
{
    [Verb("arguments", isDefault: true)]
    public class ArgumentService
    {
        [Option(shortName: 'p', longName: "port", Required = false, HelpText = "The local port that the service listens on")]
        public int Port { get; set; }
    }
}
