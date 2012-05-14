namespace monorail.commands
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;
    using EnvDTE;
    using OpenFileSystem.IO;
    using OpenFileSystem.IO.FileSystems.Local;
    using Process = System.Diagnostics.Process;

    #region Classes

    public static class VisualStudioAttacher
    {
        #region Public Methods

        [DllImport("User32")]
        private static extern int ShowWindow(int hwnd, int nCmdShow);

        [DllImport("ole32.dll")]
        public static extern int CreateBindCtx(int reserved, out IBindCtx ppbc);

        [DllImport("ole32.dll")]
        public static extern int GetRunningObjectTable(int reserved, out IRunningObjectTable prot);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        public static extern IntPtr SetFocus(IntPtr hWnd);

        public static string GetSolutionForVisualStudio(Process visualStudioProcess)
        {
            _DTE visualStudioInstance;
            if (TryGetVsInstance(visualStudioProcess.Id, out visualStudioInstance))
            {
                try
                {
                    return visualStudioInstance.Solution.FullName;
                }
                catch (Exception)
                {
                }
            }
            return null;
        }

        public static Process GetAttachedVisualStudio(Process applicationProcess)
        {
            var visualStudios = GetVisualStudioProcesses();

            foreach (var visualStudio in visualStudios)
            {
                _DTE visualStudioInstance;
                if (TryGetVsInstance(visualStudio.Id, out visualStudioInstance))
                {
                    try
                    {
                        foreach (Process debuggedProcess in visualStudioInstance.Debugger.DebuggedProcesses)
                        {
                            if (debuggedProcess.Id == applicationProcess.Id)
                            {
                                return debuggedProcess;
                            }
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            return null;
        }

        public static void AttachVisualStudioToProcess(this Process visualStudioProcess, Process applicationProcess)
        {
            _DTE visualStudioInstance;

            if (TryGetVsInstance(visualStudioProcess.Id, out visualStudioInstance))
            {
                //Find the process you want the VS instance to attach to...
                var processToAttachTo =
                    visualStudioInstance.Debugger.LocalProcesses.Cast<EnvDTE.Process>().FirstOrDefault(
                        process => process.ProcessID == applicationProcess.Id);

                //Attach to the process.
                if (processToAttachTo != null)
                {
                    processToAttachTo.Attach();

                    ShowWindow((int) visualStudioProcess.MainWindowHandle, 3);
                    SetForegroundWindow(visualStudioProcess.MainWindowHandle);
                }
                else
                {
                    throw new InvalidOperationException("Visual Studio process cannot find specified application '" +
                                                        applicationProcess.Id + "'");
                }
            }
        }

        public static Process GetVisualStudioForSolutions(List<string> solutionNames)
        {
            foreach (var solution in solutionNames)
            {
                var visualStudioForSolution = GetVisualStudioForSolution(solution);
                if (visualStudioForSolution != null)
                {
                    return visualStudioForSolution;
                    ;
                }
            }
            return null;
        }

        public static Process GetVisualStudio(IDirectory directory)
        {
            var visualStudios = GetVisualStudioProcesses();

            foreach (var visualStudio in visualStudios)
            {
                _DTE visualStudioInstance;
                if (TryGetVsInstance(visualStudio.Id, out visualStudioInstance))
                {
                    try
                    {
                        var path = new Path(visualStudioInstance.Solution.FullName);
                        var solutionDir = LocalFileSystem.Instance.GetDirectory(path.DirectoryName);

                        var isSolutionUnderOWProject = solutionDir.AncestorsAndSelf().Any(x => x.Equals(directory));
                        if (isSolutionUnderOWProject)
                        {
                            return visualStudio;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            return null;
        }

        public static Process GetVisualStudioForSolution(string solutionName)
        {
            var visualStudios = GetVisualStudioProcesses();

            foreach (var visualStudio in visualStudios)
            {
                _DTE visualStudioInstance;
                if (TryGetVsInstance(visualStudio.Id, out visualStudioInstance))
                {
                    try
                    {
                        var actualSolutionName = System.IO.Path.GetFileName(visualStudioInstance.Solution.FullName);

                        if (
                            string.Compare(actualSolutionName, solutionName, StringComparison.InvariantCultureIgnoreCase) ==
                            0)
                        {
                            return visualStudio;
                        }
                    }
                    catch (Exception)
                    {
                    }
                }
            }
            return null;
        }

        #endregion

        #region Private Methods

        private static IEnumerable<Process> GetVisualStudioProcesses()
        {
            var processes = Process.GetProcesses();
            return processes.Where(o => o.ProcessName.Contains("devenv"));
        }

        private static bool TryGetVsInstance(int processId, out _DTE instance)
        {
            var numFetched = IntPtr.Zero;
            IRunningObjectTable runningObjectTable;
            IEnumMoniker monikerEnumerator;
            var monikers = new IMoniker[1];

            GetRunningObjectTable(0, out runningObjectTable);
            runningObjectTable.EnumRunning(out monikerEnumerator);
            monikerEnumerator.Reset();

            while (monikerEnumerator.Next(1, monikers, numFetched) == 0)
            {
                IBindCtx ctx;
                CreateBindCtx(0, out ctx);

                string runningObjectName;
                monikers[0].GetDisplayName(ctx, null, out runningObjectName);

                object runningObjectVal;
                runningObjectTable.GetObject(monikers[0], out runningObjectVal);

                if (runningObjectVal is _DTE && runningObjectName.StartsWith("!VisualStudio"))
                {
                    var currentProcessId = int.Parse(runningObjectName.Split(':')[1]);

                    if (currentProcessId == processId)
                    {
                        instance = (_DTE) runningObjectVal;
                        return true;
                    }
                }
            }

            instance = null;
            return false;
        }

        #endregion
    }

    #endregion
}