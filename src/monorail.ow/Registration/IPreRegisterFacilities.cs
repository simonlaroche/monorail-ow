namespace monorail.ow.Registration
{
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;

    public interface IPreRegisterFacilities
    {
        void Install(IWindsorContainer container, IConfigurationStore store);
    }
}