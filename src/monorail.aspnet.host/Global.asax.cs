namespace monorail.aspnet.host
{
    using System;
    using System.IO;
    using System.Web;
    using Bootstrap;
    using Castle.Core;
    using Castle.MonoRail.Framework;
    using Castle.MonoRail.Framework.Configuration;
    using Castle.MonoRail.Framework.Container;
    using Castle.MonoRail.Framework.Internal;
    using Castle.MonoRail.Views.Brail;
    using Castle.Windsor;

    public class Global : HttpApplication, IServiceProviderEx, IContainerAccessor, IMonoRailConfigurationEvents,
                          IMonoRailContainerEvents
    {
        private static IWindsorContainer container;

        public IWindsorContainer Container
        {
            get { return container; }
        }

        public void Configure(IMonoRailConfiguration configuration)
        {
            configuration.ViewEngineConfig.ViewPathRoot = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Views");
            var brail = new ViewEngineInfo(typeof (BooViewEngine), false);
            configuration.ViewEngineConfig.ViewEngines.Add(brail);
        }

        public void Created(IMonoRailContainer container)
        {
        }

        public void Initialized(IMonoRailContainer container)
        {
        }

        public object GetService(Type serviceType)
        {
            return container.GetService(serviceType);
        }

        public T GetService<T>() where T : class
        {
            return container.Resolve<T>();
        }

        protected void Application_Start(object sender, EventArgs e)
        {
            InitializeOpenWrap();
            BootstrapWindsor();
        }

        private void InitializeOpenWrap()
        {
            OpenWrapBootstrapper.Init();
        }

        private static void BootstrapWindsor()
        {
            //look for installers in wrap dirs.
            container = WindsorBootstrapper.Install();
        }

        protected void Session_Start(object sender, EventArgs e)
        {
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
        }

        protected void Application_Error(object sender, EventArgs e)
        {
        }

        protected void Session_End(object sender, EventArgs e)
        {
        }

        protected void Application_End(object sender, EventArgs e)
        {
        }
    }
}