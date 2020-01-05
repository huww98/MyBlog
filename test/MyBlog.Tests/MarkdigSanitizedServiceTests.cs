using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyBlog.Services;
using Xunit;

namespace MyBlog.Tests
{
    public class MarkdigSanitizedServiceTests
    {
        [Fact]
        public void BasicTest()
        {
            var s = new MarkdigSanitizedService();
            var html = s.RenderHtml("# Test Markdown");
            Assert.Equal("<h1>Test Markdown</h1>", html.Trim());
        }

        [Fact]
        public void BootstrapClasses()
        {
            var s = new MarkdigSanitizedService();
            var content = "<p class=\"mx-1\">a</p>";
            var html = s.RenderHtml(content);
            Assert.Equal(content, html.Trim());
        }
    }
}
