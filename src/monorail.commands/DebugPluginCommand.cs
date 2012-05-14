namespace monorail.commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using OpenFileSystem.IO;
    using OpenFileSystem.IO.FileSystems.Local;
    using OpenWrap.Commands;
    using OpenWrap.Commands.Cli;
    using OpenWrap.Commands.Cli.Locators;
    using OpenWrap.Runtime;
    using OpenWrap.Services;

    [Command(Description = "Debug plugin", Noun = "plugin", Verb = "debug", Visible = true)]
    public class DebugPluginCommand : AbstractCommand
    {
        private readonly CommandExecutor commandExecutor;

        public DebugPluginCommand()
        {
            commandExecutor = new CommandExecutor(ServiceLocator.GetService<IEnumerable<ICommandLocator>>(),
                                                  ServiceLocator.GetService<IEventHub>());
        }

        [CommandInput(IsRequired = true, Position = 0)]
        public string Package { get; set; }

        [CommandInput(IsRequired = false, Position = 1)]
        public string MetaPackage { get; set; }

        protected override IEnumerable<ICommandOutput> ExecuteCore()
        {
            //Build wrap
            ExecuteCommand("build-wrap -path ./temp -quiet");

            //Create debugsite dir 
            var directory = LocalFileSystem.Instance.CreateDirectory("./debugsite");
            if (directory.Exists)
            {
                directory.Delete();
            }
            directory.MustExist();

            ExecuteCommand("init-wrap -target ./debugsite -name debugsite");

            //change environment to debug site dir
            new ServiceRegistry().Override<IEnvironment>(() => new CurrentDirectoryEnvironment(directory)).Initialize();

            yield return new Info("Changed current directory to " + Environment.CurrentDirectory);

            ExecuteCommand("add-wrap host -anchored true -content true");
            if (!string.IsNullOrEmpty(MetaPackage))
            {
                ExecuteCommand(string.Format("add-wrap {0}", MetaPackage));
            }

            ExecuteCommand(string.Format("add-wrap {0} -from ./temp", Package));

            //Reset environnement to project dir
            new ServiceRegistry().Override<IEnvironment>(() => new CurrentDirectoryEnvironment()).Initialize();

            ExecuteCommand("debug-website");

            yield return new Info("Started debug site using meta package {0}", MetaPackage);
        }

        private void ExecuteCommand(string cmd)
        {
            var result = commandExecutor.Execute(cmd, Enumerable.Empty<string>());
            if (result != 0)
            {
                throw new CommandFailedException(cmd, result);
            }
        }
    }

    [Serializable]
    public class CommandFailedException : Exception
    {
        public CommandFailedException(string cmd, int result)
            : base(string.Format("The command \"{0}\" failed with result {1}", cmd, result))
        {
        }
    }
}