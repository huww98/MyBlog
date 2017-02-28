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
            var html = Markdig.Markdown.ToHtml(markdown);
            return sanitizer.Sanitize(html);
        }
    }
}