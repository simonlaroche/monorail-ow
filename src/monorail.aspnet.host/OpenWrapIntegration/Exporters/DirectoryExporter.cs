namespace monorail.aspnet.host.OpenWrapIntegration.Exporters
{
    using System.Collections.Generic;
    using System.Linq;
    using Iesi.Collections.Generic;
    using OpenFileSystem.IO;
    using OpenWrap.PackageManagement;
    using OpenWrap.PackageModel;
    using OpenWrap.Runtime;
    using PackageManagement;

    public class ViewFolderExporter : DirectoryExporter
    {
        public ViewFolderExporter() : base("Views")
        {
        }

        public override IEnumerable<IGrouping<string, TItem>> Items<TItem>(IPackage package,
                                                                           ExecutionEnvironment environment)
        {
            if (typeof (TItem) != typeof (ViewFolderExport))
            {
                return Enumerable.Empty<IGrouping<string, TItem>>();
            }

            var enumerable = GetFolders<IDirectoryExport>(package);

            var selectMany =
                enumerable.SelectMany(x => x).Select(x => new ViewFolderExport(x)).Cast<TItem>().GroupBy(x => x.Path);

            return selectMany;
        }
    }

    public class ContentFolderExporter : DirectoryExporter
    {
        public ContentFolderExporter() : base("Content")
        {
        }

        public override IEnumerable<IGrouping<string, TItem>> Items<TItem>(IPackage package,
                                                                           ExecutionEnvironment environment)
        {
            if (typeof (TItem) != typeof (ContentFolderExport))
            {
                return Enumerable.Empty<IGrouping<string, TItem>>();
            }

            var enumerable = GetFolders<IDirectoryExport>(package);

            var selectMany =
                enumerable.SelectMany(x => x).Select(x => new ContentFolderExport(x)).Cast<TItem>().GroupBy(x => x.Path);

            return selectMany;
        }
    }

    public abstract class DirectoryExporter : IExportProvider
    {
        private const string exportName = "monorail";
        private readonly string directory;

        protected DirectoryExporter(string directory)
        {
            this.directory = directory;
        }

        public abstract IEnumerable<IGrouping<string, TItem>> Items<TItem>(IPackage package,
                                                                           ExecutionEnvironment environment)
            where TItem : IExportItem;

        protected IEnumerable<IGrouping<string, TItems>> GetFolders<TItems>(IPackage package) where TItems : IExportItem
        {
            var packagesContent = package.Content.SelectMany(x => x);

            var viewFolderFinder = new FolderFinder(directory);
            var result =
                packagesContent.Select(x => viewFolderFinder.Find(x.File)).Where(
                    y => y != null && y.Parent.Name == exportName).Select(y => new DirectoryExport(y, package)).Cast
                    <TItems>().GroupBy(z => z.Path);

            return result;
        }
    }

    public class FolderFinder
    {
        private readonly string folderName;
        private readonly Iesi.Collections.Generic.ISet<IDirectory> nonViewFolders = new HashedSet<IDirectory>();

        public FolderFinder(string folderName)
        {
            this.folderName = folderName;
        }

        public IDirectory Find(IFile file)
        {
            var parent = file.Parent;

            if (nonViewFolders.Contains(parent))
            {
                return null;
            }

            while (parent != null && parent.Name != folderName)
            {
                parent = parent.Parent;
            }

            if (parent == null)
            {
                nonViewFolders.Add(file.Parent);
            }

            return parent;
        }
    }
}