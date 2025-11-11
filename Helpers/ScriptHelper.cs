using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;

namespace ValiModern.Helpers
{
    public static class ScriptHelper
    {
        // Extension method for HtmlHelper
        public static IHtmlString RenderScriptWithAttribute(this HtmlHelper html, string bundleVirtualPath, string attribute = "defer")
        {
            // Scripts.Url(...) generates the bundle url (with cache busting query)
            var url = Scripts.Url(bundleVirtualPath).ToString();
            return new HtmlString($"<script src=\"{url}\" {attribute}></script>");
        }
    }
}
