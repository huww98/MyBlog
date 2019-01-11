using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Parser.Html;

namespace MyBlog.Helpers
{
    public static class ArticleContentHelper
    {
        public static ICollection<string> GetImageSrcs(string content)
        {
            if (string.IsNullOrEmpty(content))
            {
                return new List<string>();
            }
            var parser = new HtmlParser();
            var document = parser.Parse(Markdig.Markdown.ToHtml(content));
            return document.QuerySelectorAll("img,video,source")
                .Select(e => e.GetAttribute("src"))
                .Where(s => s != null)
                .ToList();
        }
    }
}
