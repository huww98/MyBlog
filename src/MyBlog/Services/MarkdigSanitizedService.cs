using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ganss.XSS;

namespace MyBlog.Services
{
    public class MarkdigSanitizedService : IMarkdownRenderer
    {
        private readonly HtmlSanitizer sanitizer;

        public MarkdigSanitizedService()
        {
            var allowedClasses = new List<string>();
            // Bootstrap classes
            for (var i = 0; i < 6; i++)
            {
                foreach (var a in new[] { 'm', 'p' })
                {
                    foreach (var b in new[] { "", "x", "y", "l", "r", "t", "b" })
                    {
                        allowedClasses.Add($"{a}{b}-{i}");
                    }
                }
            }
            foreach (var target in new[] { "text", "bg" })
            {
                foreach (var color in new[] { "primary", "secondary", "success", "danger", "warning", "info", "light", "dark", "white", "transparent" })
                {
                    allowedClasses.Add($"{target}-{color}");
                }
            }

            sanitizer = new HtmlSanitizer(allowedCssClasses: allowedClasses);
            sanitizer.AllowedTags.Add("video");
            sanitizer.AllowedTags.Add("source");
            sanitizer.AllowedAttributes.Add("preload");
            sanitizer.AllowedAttributes.Add("loop");
            sanitizer.AllowedAttributes.Add("autoplay");
            sanitizer.AllowedAttributes.Add("controls");
            sanitizer.AllowedAttributes.Add("class");
        }

        public string RenderHtml(string markdown)
        {
            var html = Markdig.Markdown.ToHtml(markdown);
            return sanitizer.Sanitize(html);
        }
    }
}
