namespace monorail.aspnet.host.OpenWrapIntegration.Exporters
{
    using System.Collections.Generic;
    using System.Linq;
    using OpenWrap.PackageManagement;
    using OpenWrap.PackageManagement.Exporters.Assemblies;
    using OpenWrap.PackageModel;
    using OpenWrap.Runtime;
    using PackageManagement;

    public class PlugInAssemblyExporter : AbstractAssemblyExporter
    {
        public PlugInAssemblyExporter() : base("monorail")
        {
        }

        public override IEnumerable<IGrouping<string, TItem>> Items<TItem>(IPackage package,
                                                                           ExecutionEnvironment environment)
        {
            if (typeof (TItem) != typeof (IPlugInAssemblyExport))
            {
                return Enumerable.Empty<IGrouping<string, TItem>>();
            }

            var assemblies = GetAssemblies<Exports.IAssembly>(package, environment);

            var selectMany =
                assemblies.SelectMany(x => x).Select(x => new PlugInAssemblyExport(x)).Cast<TItem>().GroupBy(x => x.Path);

            return selectMany;
        }
    }
}