namespace monorail.aspnet.host.OpenWrapIntegration.Runtime
{
    using System.Web;
    using OpenFileSystem.IO.FileSystems.Local;
    using OpenWrap.Runtime;

    public class WebAppDirectoryEnvironment : CurrentDirectoryEnvironment
    {
        /// <summary>
        ///   Assume that the site is started from an anchored package in the wrap directory.
        /// </summary>
        public WebAppDirectoryEnvironment()
            : base(
                LocalFileSystem.Instance.GetDirectory(HttpContext.Current.Request.PhysicalApplicationPath).Parent.Parent
                )
        {
        }
    }
}