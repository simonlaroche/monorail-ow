namespace monorail.commands
{
    using System.Collections.Generic;
    using System.Linq;
    using OpenWrap.Commands;
    using OpenWrap.Commands.Cli;
    using OpenWrap.Commands.Cli.Locators;

    /// <summary>
    ///   Shameless copy of the console command executor in openwrap but without the subscription to event hub to avoid doubled console output
    /// </summary>
    public class CommandExecutor
    {
        private readonly IEventHub _eventHub;
        private readonly IEnumerable<ICommandLocator> _handlers;

        public CommandExecutor(IEnumerable<ICommandLocator> handlers, IEventHub eventHub)
        {
            _handlers = handlers;
            _eventHub = eventHub;
        }

        public int Execute(string commandLine, IEnumerable<string> optionalInputs)
        {
            commandLine = commandLine.Trim();
            if (commandLine == string.Empty)
            {
                return 0;
            }
            var commandParameters = commandLine;
            var command = _handlers.Select(x => x.Execute(ref commandParameters)).Where(x => x != null).FirstOrDefault();
            if (command == null)
            {
                var sp = commandLine.IndexOf(" ");

                _eventHub.Publish(
                    new Error(
                        "The term '{0}' is not a recognized command or alias. Check the spelling or enter 'get-help' to get a list of available commands.",
                        sp != -1 ? commandLine.Substring(0, sp) : commandLine));
                return -10;
            }
            var returnCode = 0;
            var commandLineRunner = new CommandLineRunner {OptionalInputs = optionalInputs};
            foreach (var output in commandLineRunner.Run(command, commandParameters))
            {
                _eventHub.Publish(output);
                if (output.Type == CommandResultType.Error)
                {
                    returnCode = -50;
                }
            }
            return returnCode;
        }
    }
}