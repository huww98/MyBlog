using System;
using Xunit;
using MyBlog.Services;

namespace MyBlog.Tests
{
    public class SanitizeSummaryGeneratorTests
    {
        SanitizeSummaryGenerator generator = new SanitizeSummaryGenerator();

        [Fact]
        public void HtmlTagsStripped()
        {
            var content = "<p>123456</p>7890";
            var expected = "1234567890";
            Assert.Equal(expected, generator.GenerateSummary(content, 100));
        }

        [Fact]
        public void SpecialHtmlTagsNotStripped()
        {
            var content = "<p>1<strong>234</strong>5<b>67</b>8</p>9<i>0</i>";
            var expected = "1<strong>234</strong>5<b>67</b>89<i>0</i>";
            Assert.Equal(expected, generator.GenerateSummary(content, 1000));
        }

        [Fact]
        public void HeadingHtmlTagsReplaced()
        {
            var content = 
                "<h1>Heading</h1>some text<h2>Heading</h2>some text<h3>Heading</h3>some text"+
                "<h4>Heading</h4>some text<h5>Heading</h5>some text<h6>Heading</h6>some text";
            var expected =
                "<strong>Heading</strong>some text<strong>Heading</strong>some text<strong>Heading</strong>some text" +
                "<strong>Heading</strong>some text<strong>Heading</strong>some text<strong>Heading</strong>some text";
            Assert.Equal(expected, generator.GenerateSummary(content, 1000));
        }

        [Theory]
        [InlineData("12345",5)]
        [InlineData("12345",10)]
        [InlineData("测试的中文",5)]
        public void PlainTextNotCut(string content,int summaryLength) 
        {
            var summary = generator.GenerateSummary(content, summaryLength);
            Assert.Equal(content, summary);
        }


        [Theory]
        [InlineData("1234567", 5)]
        [InlineData("测试的比较长的中文", 6)]
        public void PlainTextCut(string content, int summaryLength)
        {
            var summary = generator.GenerateSummary(content, summaryLength);
            var expected = content.Substring(0, summaryLength) + "…";
            Assert.Equal(expected, summary);
        }

        [Theory]
        //Not cut data
        [InlineData("1<p>23</p>45", 5, "12345")]
        [InlineData("1<strong>234</strong>5", 5, "1<strong>234</strong>5")]
        [InlineData("1<strong>2345</strong>", 5, "1<strong>2345</strong>")]
        [InlineData("1<strong>2345</strong><b></b>", 5, "1<strong>2345</strong>")]
        //Cut data
        [InlineData("1<p>2345678</p>90", 5, "12345…")]
        [InlineData("1<strong>234</strong>5678", 5, "1<strong>234</strong>5…")]
        [InlineData("1<strong>2345</strong>haha", 5, "1<strong>2345</strong>…")]
        [InlineData("1<strong>2345</strong><b>haha</b>", 5, "1<strong>2345</strong>…")]
        [InlineData("1<strong>23456789</strong>0<b>haha</b>", 5, "1<strong>2345</strong>…")]
        public void HtmlText(string content, int summaryLength, string result)
        {
            var summary = generator.GenerateSummary(content, summaryLength);
            Assert.Equal(result, summary);
        }
    }
}
