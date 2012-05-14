namespace monorail.aspnet.host.Bootstrap
{
    using OpenWrap.Services;
    using OpenWrapIntegration;

    public static class OpenWrapBootstrapper
    {
        public static void Init()
        {
            new ServiceRegistry().MonorailOverride().Initialize();
        }
    }
}