namespace monowrap.commands
{
	using System;
	using System.Collections.Generic;
	using System.Diagnostics;
	using System.Linq;
	using OpenFileSystem.IO;
	using OpenFileSystem.IO.FileSystems.Local;
	using OpenWrap.Commands;
	using OpenWrap.Commands.Cli;
	using OpenWrap.Commands.Cli.Locators;
	using OpenWrap.Runtime;
	using OpenWrap.Services;

	[Command(Description = "Quickly publish to a specified remote", Noun = "wrap", Verb = "quickpublish", Visible = true)]
	public class QuickPublishCommand: AbstractCommand
	{
		private readonly CommandExecutor commandExecutor;

		public QuickPublishCommand()
		{
			commandExecutor = new CommandExecutor(ServiceLocator.GetService<IEnumerable<ICommandLocator>>(),
												  ServiceLocator.GetService<IEventHub>());
		}

		protected override IEnumerable<ICommandOutput> ExecuteCore()
		{
			yield return new Info("Building wrap");
			ExecuteCommand("wrap build");

			IDirectory directory = LocalFileSystem.Instance.GetCurrentDirectory();
			var remote = Remote;
			if (string.IsNullOrEmpty(remote))
			{
				remote = "dev-repo";
			}

			yield return new Info("publishing {0} to {1}", remote, directory.Name);
			ExecuteCommand(string.Format("wrap publish -remote {0} -name {1}", remote, directory.Name));
		}

		private void ExecuteCommand(string cmd)
		{
			var result = commandExecutor.Execute(cmd, Enumerable.Empty<string>());
			if (result != 0)
			{
				throw new CommandFailedException(cmd, result);
			}
		}

		[CommandInput(IsRequired = false, Position = 0 )]
		public string Remote { get; set; }
		
	}
}