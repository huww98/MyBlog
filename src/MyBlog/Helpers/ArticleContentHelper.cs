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
            var parser = new HtmlParser();
            var document = parser.Parse(Markdig.Markdown.ToHtml(content));
            return document.Images.Select(img => img.GetAttribute("src")).ToList();
        }
    }
}