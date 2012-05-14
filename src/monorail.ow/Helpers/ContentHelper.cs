namespace monorail.ow.Helpers
{
    using System;
    using Castle.MonoRail.Framework.Helpers;

    public class ContentHelper : AbstractHelper
    {
        private readonly Func<string, string> contentPath;

        public ContentHelper(Func<string, string> contentPath)
        {
            this.contentPath = contentPath;
        }

        public string CssLink(string name)
        {
            return BuildLink(name, "css", "<link type=\"text/css\" rel=\"stylesheet\" href=\"{0}\" />");
        }

        public string JsScript(string name)
        {
            return BuildLink(name, "js", "<script src=\"{0}\" type=\"text/javascript\">");
        }

        private string BuildLink(string name, string ext, string format)
        {
            if (!name.EndsWith("." + ext))
            {
                name = name + "." + ext;
            }

            var contentFile = contentPath.Invoke(name);
            var virtualPath = contentFile.Replace(Context.ApplicationPhysicalPath, "/").Replace(@"\", "/");

            return string.Format(format, virtualPath);
        }
    }
}