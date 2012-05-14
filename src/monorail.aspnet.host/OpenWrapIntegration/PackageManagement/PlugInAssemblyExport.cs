namespace monorail.aspnet.host.OpenWrapIntegration.PackageManagement
{
    using System.Reflection;
    using OpenFileSystem.IO;
    using OpenWrap.PackageManagement;
    using OpenWrap.PackageModel;

    public interface IPlugInAssemblyExport : Exports.IAssembly
    {
    }

    public class PlugInAssemblyExport : IPlugInAssemblyExport
    {
        private readonly Exports.IAssembly assembly;

        public PlugInAssemblyExport(Exports.IAssembly assembly)
        {
            this.assembly = assembly;
        }

        public string Path
        {
            get { return assembly.Path; }
        }

        public IPackage Package
        {
            get { return assembly.Package; }
        }

        public IFile File
        {
            get { return assembly.File; }
        }

        public string Platform
        {
            get { return assembly.Platform; }
        }

        public string Profile
        {
            get { return assembly.Profile; }
        }

        public AssemblyName AssemblyName
        {
            get { return assembly.AssemblyName; }
        }
    }
}