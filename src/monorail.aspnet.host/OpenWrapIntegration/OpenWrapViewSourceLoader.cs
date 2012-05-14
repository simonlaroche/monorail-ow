namespace monorail.aspnet.host.OpenWrapIntegration
{
    using System.Collections;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using Castle.MonoRail.Framework;
    using Castle.MonoRail.Framework.Configuration;
    using Castle.MonoRail.Framework.Views;
    using OpenFileSystem.IO;
    using OpenWrap.PackageManagement;
    using OpenWrap.Runtime;
    using OpenWrap.Services;
    using PackageManagement;
    using Path = System.IO.Path;

    public class OpenWrapViewSourceLoader : IViewSourceLoader, IMRServiceEnabled
    {
        private readonly List<IDirectory> additionnalPathSources;
        private readonly object locker = new object();
        private readonly List<FileSystemWatcher> viewFolderWatchers = new List<FileSystemWatcher>();
        private bool enableCache = true;
        private FileSystemEventHandler viewChangedImpl = delegate { };
        private string viewRootDir = "Views";
        private string virtualViewDir;

        public OpenWrapViewSourceLoader()
        {
            var environment = ServiceLocator.GetService<IEnvironment>();
            var packageManager = ServiceLocator.GetService<IPackageManager>();

            foreach (var scopedDescriptor in environment.ScopedDescriptors)
            {
                var files = packageManager.GetProjectExports<ViewFolderExport>(scopedDescriptor.Value.Value,
                                                                               environment.ProjectRepository,
                                                                               environment.ExecutionEnvironment);

                additionnalPathSources = files.SelectMany(x => x).Distinct().Select(x => x.Directory).ToList();
            }
        }

        public virtual void Service(IMonoRailServices provider)
        {
            var railConfiguration = (IMonoRailConfiguration) provider.GetService(typeof (IMonoRailConfiguration));
            if (railConfiguration == null)
            {
                return;
            }
            viewRootDir = railConfiguration.ViewEngineConfig.ViewPathRoot;
            virtualViewDir = railConfiguration.ViewEngineConfig.VirtualPathRoot;
        }

        ///<summary>
        ///  Useless (I think) because we use OW
        ///</summary>
        public string VirtualViewDir
        {
            get { return virtualViewDir; }
            set { virtualViewDir = value; }
        }

        /// <summary>
        ///   Useless because we use OW
        /// </summary>
        public string ViewRootDir
        {
            get { return viewRootDir; }
            set { viewRootDir = value; }
        }

        public bool EnableCache
        {
            get { return enableCache; }
            set { enableCache = value; }
        }

        /// <summary>
        ///   We don't support assembly views, stick your views in a wrap
        /// </summary>
        public IList AssemblySources
        {
            get { return new ArrayList(); }
        }

        public IList PathSources
        {
            get { return additionnalPathSources.Select(x => x.Path).ToArray(); }
        }

        public event FileSystemEventHandler ViewChanged
        {
            add
            {
                lock (locker)
                {
                    if (viewFolderWatchers.Count == 0)
                    {
                        InitViewFolderWatch();
                    }
                    viewChangedImpl += value;
                }
            }
            remove
            {
                lock (locker)
                {
                    viewChangedImpl -= value;
                    if (viewChangedImpl != null)
                    {
                        return;
                    }
                    DisposeViewFolderWatch();
                }
            }
        }

        public bool HasSource(string sourceName)
        {
            return HasTemplateOnFileSystem(sourceName);
        }

        public IViewSource GetViewSource(string templateName)
        {
            var fileInfo = CreateFileInfo(templateName);
            if (fileInfo != null && fileInfo.Exists)
            {
                return new FileViewSource(fileInfo, enableCache);
            }
            return null;
        }

        public string[] ListViews(string dirName, params string[] fileExtensionsToInclude)
        {
            var views = new ArrayList();
            CollectViewsOnFileSystem(dirName, views, fileExtensionsToInclude);
            return (string[]) views.ToArray(typeof (string));
        }

        public void AddPathSource(string pathSource)
        {
        }

        public void AddAssemblySource(AssemblySourceInfo assemblySourceInfo)
        {
        }

        private void DisposeViewFolderWatch()
        {
        }

        /// <summary>
        ///   Since we are using openwrap to resolve views, if the views they will be in another directory, no need to watch then
        /// </summary>
        private void InitViewFolderWatch()
        {
        }

        private bool HasTemplateOnFileSystem(string templateName)
        {
            return additionnalPathSources.Any(viewRoot => CreateFileInfo(viewRoot.Path, templateName).Exists);
        }

        private FileInfo CreateFileInfo(string templateName)
        {
            return
                additionnalPathSources.Select(viewRoot => CreateFileInfo(viewRoot.Path, templateName)).FirstOrDefault(
                    fileInfo2 => fileInfo2.Exists);
        }

        private static FileInfo CreateFileInfo(string viewRoot, string templateName)
        {
            if (Path.IsPathRooted(templateName))
            {
                templateName = templateName.Substring(Path.GetPathRoot(templateName).Length);
            }
            return new FileInfo(Path.Combine(viewRoot, templateName));
        }

        private void CollectViewsOnFileSystem(string dirName, ArrayList views, params string[] fileExtensionsToInclude)
        {
            var directoryInfo = new DirectoryInfo(Path.Combine(ViewRootDir, dirName));
            if (!directoryInfo.Exists)
            {
                return;
            }
            if (fileExtensionsToInclude == null || fileExtensionsToInclude.Length == 0)
            {
                fileExtensionsToInclude = new[] {".*"};
            }
            foreach (var str in fileExtensionsToInclude)
            {
                foreach (var fileInfo in directoryInfo.GetFiles("*" + str))
                {
                    views.Add(Path.Combine(dirName, fileInfo.Name));
                }
            }
        }
    }
}