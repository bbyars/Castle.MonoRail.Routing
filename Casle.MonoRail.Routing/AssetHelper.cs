using System.Collections;
using System.Configuration;
using System.IO;
using System.Text;
using System.Web;

namespace Castle.MonoRail.Routing
{
    /// <summary>
    /// By adding a querystring timestamp to your assets, you can allow browsers and proxy
    /// caches to cache the asset based on the URL.  As soon as the timestamp changes, a different
    /// querystring will be appended, which will bypass all caches.  So you get the best of both
    /// worlds -- maximum caching, plus easy refreshing.
    /// </summary>
    public class AssetHelper
    {
        public AssetHelper()
        {
            MissingImage = GetConfig("missingImage", "missing.jpg");
            var virtualDirectory = HttpRuntime.AppDomainAppVirtualPath;
            if (virtualDirectory == "/")
                virtualDirectory = "";
            ImagesRoot = GetConfig("imagesRoot", virtualDirectory + "/Content/i");
            CssRoot = GetConfig("cssRoot", virtualDirectory + "/Content/css");
            JavaScriptRoot = GetConfig("javascriptRoot", virtualDirectory + "/Content/js");
        }

        private string GetConfig(string key, string defaultValue)
        {
            return ConfigurationManager.AppSettings[key] ?? defaultValue;
        }

        public string MissingImage { get; set; }
        public string ImagesRoot { get; set; }
        public string CssRoot { get; set; }
        public string JavaScriptRoot { get; set; }

        public string Image(string filename)
        {
            return Image(filename, new Hashtable());
        }

        public string Image(string filename, IDictionary attributes)
        {
            var builder = new StringBuilder();
            builder.AppendFormat("<img src=\"{0}\"", Src(filename));

            foreach (DictionaryEntry entry in attributes)
            {
                builder.AppendFormat(" {0}=\"{1}\"", entry.Key, entry.Value);
            }
            builder.Append(" />");

            return builder.ToString().Trim();
        }

        public string Src(string filename)
        {
            return GetAssetNameFor(ImagesRoot, filename, MissingImage);
        }

        public string Css(string filename)
        {
            return Css(filename, null);
        }

        public string Css(string filename, string media)
        {
            var mediaAttribute = string.Empty;
            if (!string.IsNullOrEmpty(media))
                mediaAttribute = string.Format("media=\"{0}\" ", media);

            return string.Format("<link href=\"{0}\" rel=\"Stylesheet\" type=\"text/css\" {1}/>",
                GetAssetNameFor(CssRoot, filename), mediaAttribute);
        }

        public string JavaScript(string filename)
        {
            return JavaScript(filename, null);
        }

        public string JavaScript(string filename, string type)
        {
            var typeAttribute = "text/javascript";
            if (!string.IsNullOrEmpty(type))
                typeAttribute = string.Format("{0}", type);

            return string.Format("<script src=\"{0}\" type=\"{1}\"></script>",
                GetAssetNameFor(JavaScriptRoot, filename), typeAttribute);
        }

        private string GetAssetNameFor(string root, string filename)
        {
            return GetAssetNameFor(root, filename, filename);
        }

        private string GetAssetNameFor(string root, string filename, string substituteIfAssetIsMissing)
        {
            var fileParts = filename.Split('?');
            var fileNameWithPath = root + "/" + fileParts[0];

            var serverPath = GetServerPath(fileNameWithPath);
            if (!File.Exists(serverPath))
                return root + "/" + substituteIfAssetIsMissing;

            var result = fileNameWithPath + "?" + GetAssetIdFor(fileNameWithPath);
            if (fileParts.Length > 1)
                result = result + "&" + fileParts[1];
            return result;
        }

        private string GetAssetIdFor(string filename)
        {
            return File.GetLastWriteTime(GetServerPath(filename)).Ticks.ToString();
        }

        private string GetServerPath(string virtualPath)
        {
            // This is a bit of a hack.  HttpContext.Current is null during testing,
            // so our tests don't actually exercise the production code...
            return HttpContext.Current == null ? virtualPath : HttpContext.Current.Server.MapPath(virtualPath);
        }
    }
}