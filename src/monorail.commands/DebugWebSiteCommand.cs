namespace monorail.commands
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using OpenFileSystem.IO.FileSystems.Local;
    using OpenWrap.Commands;
    using OpenWrap.Runtime;
    using OpenWrap.Services;

    [Command(Description = "Debug web site", Noun = "website", Verb = "debug", Visible = true)]
    public class DebugWebSiteCommand : AbstractCommand

    {
        private readonly IEnvironment environment;

        public DebugWebSiteCommand() : this(ServiceLocator.GetService<IEnvironment>())
        {
        }

        private DebugWebSiteCommand(IEnvironment environment)
        {
            this.environment = environment;
        }

        [CommandInput(IsRequired = false, Position = 0)]
        public int Port { get; set; }

        private static string GetProgramFileVariable()
        {
            if (IntPtr.Size > 4)
            {
                return Environment.GetEnvironmentVariable("ProgramFiles(x86)");
            }
            return Environment.GetEnvironmentVariable("ProgramFiles");
        }

        protected override IEnumerable<ICommandOutput> ExecuteCore()
        {
            yield return new Info("Starting web site");

            var directory = LocalFileSystem.Instance.CreateDirectory(Environment.CurrentDirectory);
            var sitePath = directory.Path.Combine("debugsite/wraps/monorail/host");

            var port = 49001;
            if (Port != 0)
            {
                port = Port;
            }

            var webServer = string.Format(@"{0}\Common Files\microsoft shared\DevServer\10.0\WebDev.WebServer40.EXE",
                                          GetProgramFileVariable());
            yield return new Info("Starting process: {0}", webServer);

            var process = Process.Start(webServer, string.Format("/port:{0} /path:\"{1}", port, sitePath));

            VisualStudioAttacher.GetVisualStudio(environment.CurrentDirectory).AttachVisualStudioToProcess(process);

            yield return new Info("Web site started");
        }
    }
}