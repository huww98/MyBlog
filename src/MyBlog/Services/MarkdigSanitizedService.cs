using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ganss.XSS;

namespace MyBlog.Services
{
    public class MarkdigSanitizedService : IMarkdownRenderer
    {
        public string RenderHtml(string markdown)
        {
            var sanitizer = new HtmlSanitizer();
            sanitizer.AllowedTags.Add("video");
            sanitizer.AllowedTags.Add("source");
            sanitizer.AllowedAttributes.Add("preload");
            sanitizer.AllowedAttributes.Add("loop");
            sanitizer.AllowedAttributes.Add("autoplay");
            sanitizer.AllowedAttributes.Add("controls");
            var html = Markdig.Markdown.ToHtml(markdown);
            return sanitizer.Sanitize(html);
        }
    }
}