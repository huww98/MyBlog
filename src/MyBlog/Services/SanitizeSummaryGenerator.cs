using Ganss.XSS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom.Html;

namespace MyBlog.Services
{
    public class SanitizeSummaryGenerator : ISummaryGenerator
    {
        HtmlSanitizer sanitizer;
        public SanitizeSummaryGenerator()
        {
            sanitizer = new HtmlSanitizer(
                new[] { "b", "strong", "i" },
                new string[0], new string[0], new string[0], new string[0]);
            sanitizer.KeepChildNodes = true;
            sanitizer.RemovingTag += Sanitizer_RemovingTag;
        }

        private void Sanitizer_RemovingTag(object sender, RemovingTagEventArgs e)
        {
            if (e.Tag is IHtmlHeadingElement)
            {
                e.Cancel = true;
                var newElement = e.Tag.Owner.CreateElement("strong");
                newElement.InnerHtml = e.Tag.InnerHtml;
                e.Tag.Replace(newElement);
            }
        }

        public string GenerateSummary(string content)
        {
            string summary = sanitizer.Sanitize(content);
            if (summary.Length > 300)
            {
                summary = summary.Substring(0, 300);
                summary += "…";
            }
            return summary;
        }
    }
}
