namespace monorail.aspnet.host.Bootstrap
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Castle.Facilities.Startable;
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.Resolvers.SpecializedResolvers;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.MonoRail.Framework;
    using Castle.MonoRail.WindsorExtension;
    using Castle.Windsor;
    using OpenWrap.PackageManagement;
    using OpenWrap.Runtime;
    using OpenWrap.Services;
    using OpenWrapIntegration;
    using OpenWrapIntegration.PackageManagement;
    using ow.Helpers;
    using ow.Registration;
    using Assembly = System.Reflection.Assembly;

    /// <summary>
    ///   Method to install all the facilities, then all components. No facility should be registered in WindsorInstallers
    /// </summary>
    public static class WindsorBootstrapper
    {
        public static IWindsorContainer Install()
        {
            var container = new WindsorContainer();
            var environment = ServiceLocator.GetService<IEnvironment>();
            var packageManager = ServiceLocator.GetService<IPackageManager>();

            container.Kernel.Resolver.AddSubResolver(new ArrayResolver(container.Kernel));

            Install<IPreRegisterFacilities>(environment, packageManager, x => x.Install(container, null));
            Install<IWindsorInstaller>(environment, packageManager, x => x.Install(container, null));

            return container;
        }

        private static void Install<T>(IEnvironment environment, IPackageManager packageManager,
                                       Action<T> installDelegate)
        {
            var exportedTypes = LoadAssemblies(environment, packageManager).GetExportedTypes<T>();

            foreach (var exportedType in exportedTypes)
            {
                var installer = (T) Activator.CreateInstance(exportedType);
                installDelegate.Invoke(installer);
            }
        }

        private static IEnumerable<Assembly> LoadAssemblies(IEnvironment environment, IPackageManager packageManager)
        {
            var assemblies = Enumerable.Empty<Assembly>();
            foreach (var scopedDescriptor in environment.ScopedDescriptors)
            {
                assemblies =
                    assemblies.Concat(
                        packageManager.GetProjectExports<IPlugInAssemblyExport>(scopedDescriptor.Value.Value,
                                                                                environment.ProjectRepository,
                                                                                environment.ExecutionEnvironment).
                            SelectMany(x => x).Select(item => Assembly.LoadFrom(item.File.Path)));
            }
            return assemblies;
        }

        private static IEnumerable<Type> GetExportedTypes<T>(this IEnumerable<Assembly> assemblies)
        {
            return assemblies.SelectMany(x => x.GetTypes().Where(t => typeof (T).IsAssignableFrom(t)));
        }
    }

    /// <summary>
    ///   Register facilities required for all plugins
    /// </summary>
    public class FacilityInstaller : IPreRegisterFacilities
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.AddFacility<StartableFacility>();
            container.AddFacility<MonoRailFacility>();
        }
    }

    /// <summary>
    ///   Register components for all plugins
    /// </summary>
    public class Installer : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<IViewSourceLoader>().ImplementedBy<OpenWrapViewSourceLoader>());

            var contentLocator = new ContentLocator();
            container.Register(
                Component.For<ContentHelper>().UsingFactoryMethod(
                    () => new ContentHelper(x => contentLocator.GetContentPath(x))));
        }
    }
}