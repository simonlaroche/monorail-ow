namespace monorail.aspnet.host.OpenWrapIntegration
{
    using Exporters;
    using OpenWrap.PackageManagement;
    using OpenWrap.PackageManagement.Exporters;
    using OpenWrap.PackageManagement.Exporters.Assemblies;
    using OpenWrap.Runtime;
    using OpenWrap.Services;
    using Runtime;

    public static class ServiceRegistryOverrides
    {
        public static ServiceRegistry MonorailOverride(this ServiceRegistry registry)
        {
            return
                registry.Override<IEnvironment>(() => new WebAppDirectoryEnvironment()).Override<IPackageExporter>(
                    () =>
                    new DefaultPackageExporter(new IExportProvider[]
                                                   {
                                                       new PlugInAssemblyExporter(), new DefaultAssemblyExporter(),
                                                       new ViewFolderExporter(), new ContentFolderExporter()
                                                   }));
        }
    }
}