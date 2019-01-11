using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyBlog.Helpers;
using MyBlog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MyBlog.Tests
{
    public class ArticleContentHelperTests
    {
        [Fact]
        public void GetImageSrcs()
        {
            string content = "<div><p>some text</p><img src='1.jpg'/><p>some text</p></div>";
            var result = ArticleContentHelper.GetImageSrcs(content);
            Assert.Equal("1.jpg", result.Single());
        }
    }
}
