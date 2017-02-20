using Ganss.XSS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp.Dom.Html;
using AngleSharp.Dom;
using Markdig;

namespace MyBlog.Services
{
    //This class is NOT thread safe!
    public class SanitizeSummaryGenerator : ISummaryGenerator
    {
        HtmlSanitizer sanitizer;
        public SanitizeSummaryGenerator()
        {
            sanitizer = new HtmlSanitizer(
                new[] { "b", "strong", "i", "em", "h1", "h2", "h3", "h4", "h5", "h6" },
                new string[0], new string[0], new string[0], new string[0]);
            sanitizer.KeepChildNodes = true;
            sanitizer.PostProcessNode += replaceHeading;
            sanitizer.PostProcessNode += calcLength;
        }

        int summaryLength;
        int currentLength = 0;
        bool summaryLengthReached = false;
        bool summaryLengthExeceeded = false;
        private void calcLength(object sender, PostProcessNodeEventArgs e)
        {
            if (summaryLengthReached)
            {
                if (!summaryLengthExeceeded && e.Node.TextContent.Length > 0)
                {
                    summaryLengthExeceeded = true;
                }
                e.Node.Parent.RemoveChild(e.Node);
                return;
            }

            var textNode = e.Node as IText;
            if (textNode != null)
            {
                int nextLength = currentLength + textNode.Text.Length;
                if (nextLength < summaryLength)
                {
                    currentLength += textNode.Text.Length;
                }
                else if (nextLength == summaryLength)
                {
                    summaryLengthReached = true;
                }
                else
                {
                    var newTextNode = e.Document.CreateTextNode(textNode.Text.Substring(0, summaryLength - currentLength));
                    e.ReplacementNodes.Add(newTextNode);
                    summaryLengthReached = summaryLengthExeceeded = true;
                }
            }
        }

        private void replaceHeading(object sender, PostProcessNodeEventArgs e)
        {
            var headingElement = e.Node as IHtmlHeadingElement;
            if (headingElement != null)
            {
                var newElement = e.Document.CreateElement("strong");
                newElement.InnerHtml = headingElement.InnerHtml;
                e.ReplacementNodes.Add(newElement);
                return;
            }
        }

        public string GenerateSummary(string htmlContent, int summaryLength)
        {
            this.summaryLength = summaryLength;
            currentLength = 0;
            summaryLengthReached = summaryLengthExeceeded = false;

            string summary = sanitizer.Sanitize(htmlContent);
            if (summaryLengthExeceeded)
            {
                summary += "…";
            }
            return summary;
        }
    }
}
