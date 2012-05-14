namespace monorail.aspnet.host.OpenWrapIntegration.PackageManagement
{
    using OpenFileSystem.IO;
    using OpenWrap.PackageManagement;
    using OpenWrap.PackageModel;

    //todo: change directory export to content export.
    //todo: add additional .data file for content, for ex javascript file hosted on CDN
    public class DirectoryExport : IDirectoryExport
    {
        private readonly IDirectory directory;

        public DirectoryExport(IDirectory directory, IPackage package)
        {
            this.directory = directory;
            Path = directory.Path;
            Package = package;
        }

        public IDirectory Directory
        {
            get { return directory; }
        }

        public string Path { get; private set; }

        public IPackage Package { get; private set; }
    }

    public class ViewFolderExport : IDirectoryExport
    {
        private readonly IDirectoryExport directory;

        public ViewFolderExport(IDirectoryExport directory)
        {
            this.directory = directory;
        }

        public string Path
        {
            get { return directory.Path; }
        }

        public IPackage Package
        {
            get { return directory.Package; }
        }

        public IDirectory Directory
        {
            get { return directory.Directory; }
        }
    }

    public class ContentFolderExport : IDirectoryExport
    {
        private readonly IDirectoryExport directory;

        public ContentFolderExport(IDirectoryExport directory)
        {
            this.directory = directory;
        }

        public string Path
        {
            get { return directory.Path; }
        }

        public IPackage Package
        {
            get { return directory.Package; }
        }

        public IDirectory Directory
        {
            get { return directory.Directory; }
        }
    }

    public interface IDirectoryExport : IExportItem
    {
        IDirectory Directory { get; }
    }
}