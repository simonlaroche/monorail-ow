namespace monorail.aspnet.host.OpenWrapIntegration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using OpenFileSystem.IO;
    using OpenFileSystem.IO.FileSystems.Local;
    using OpenWrap.PackageManagement;
    using OpenWrap.Runtime;
    using OpenWrap.Services;
    using PackageManagement;

    public class ContentLocator
    {
        private readonly IList<IDirectory> contentPathSources;
        private readonly IDirectory contentRoot;

        //todo: Link content to a content cache similar to openwraps package cache. Use same mechanism?
        public ContentLocator()
        {
            var environment = ServiceLocator.GetService<IEnvironment>();
            var packageManager = ServiceLocator.GetService<IPackageManager>();

            var baseDirectory = LocalFileSystem.Instance.CreateDirectory(AppDomain.CurrentDomain.BaseDirectory);

            contentRoot = baseDirectory.GetDirectory("Content");
            contentRoot.MustExist();

            foreach (var scopedDescriptor in environment.ScopedDescriptors)
            {
                var files = packageManager.GetProjectExports<ContentFolderExport>(scopedDescriptor.Value.Value,
                                                                                  environment.ProjectRepository,
                                                                                  environment.ExecutionEnvironment);

                contentPathSources = files.SelectMany(x => x).Distinct().Select(x => x.Directory).ToList();
                foreach (
                    var pathSource in
                        contentPathSources.SelectMany(contentPathSource => contentPathSource.Directories()))
                {
                    pathSource.LinkTo(contentRoot.GetDirectory(pathSource.Name).MustExist().Path.FullPath);
                }
            }
        }

        public string GetContentPath(string content)
        {
            var file = CreateFileInfo(content);

            if (file != null && file.Exists)
            {
                return file.Path;
            }
            throw new ContentNotFoundException(content);
        }

        private IFile CreateFileInfo(string contentName)
        {
            return contentRoot.GetFile(contentName);
        }
    }

    [Serializable]
    public class ContentNotFoundException : Exception
    {
        public ContentNotFoundException(string contentName)
            : base(string.Format("The requested content {0} cannot be found", contentName))
        {
        }
    }
}