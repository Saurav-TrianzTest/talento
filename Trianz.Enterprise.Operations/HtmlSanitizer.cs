using System.Web.Mvc;
using Ganss.Xss;

namespace Trianz.Enterprise.Operations
{
    public static class HtmlSanitizerHelper
    {
        private static readonly HtmlSanitizer _sanitizer = new HtmlSanitizer();

        public static string Sanitize(string html)
        {
            if (html == null) return null;
            var config = System.Configuration.ConfigurationManager.AppSettings["HtmlSanitizerConfig"];
            if (!string.IsNullOrEmpty(config))
            {
                // Load allowed tags/attributes from config
                _sanitizer.AllowedTags.Clear();
                foreach (var tag in config.Split(','))
                {
                    _sanitizer.AllowedTags.Add(tag.Trim());
                }
            }
            return _sanitizer.Sanitize(html);
        }
    }
}
