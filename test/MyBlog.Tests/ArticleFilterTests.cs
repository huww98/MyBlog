using MyBlog.Models;
using MyBlog.Models.ArticleViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MyBlog.Tests
{
    public class ArticleFilterTests
    {
        [Fact]
        public void NotFilterWithNullProperties()
        {
            var articles = new[]
            {
                new Article { Title="Test 1"},
                new Article { Title="Test 2"}
            };

            var filter = new ArticleFilterViewModel();

            var result = articles.AsQueryable().ApplyArticleFilter(filter);

            Assert.Equal(articles.Count(), result.Count());
            foreach (var item in articles)
            {
                Assert.Contains(item, result);
            }
        }

        [Fact]
        public void Filter()
        {
            var articles = new[]
            {
                new Article { Title="Test 1", CreatedTime=new DateTime(3687,8,7,16,35,26)},
                new Article { Title="Test 2", CreatedTime=new DateTime(3687,8,24,16,35,26)},
                new Article { Title="Test 3", CreatedTime=new DateTime(3687,8,1,16,35,26)},
                new Article { Title="Test 4", CreatedTime=new DateTime(3687,8,16,16,35,26)}
            };
            articles.Where(a => a.Title == "Test 2").Single().Categories.Add(new ArticleCategory { CategoryID = 64 });
            articles.Where(a => a.Title == "Test 3").Single().Categories.Add(new ArticleCategory { CategoryID = 64 });
            articles.Where(a => a.Title == "Test 4").Single().Categories.Add(new ArticleCategory { CategoryID = 64 });

            var filter = new ArticleFilterViewModel
            {
                CategoryID = 64,
                FromDate = new DateTime(3687, 8, 7),
                ToDate = new DateTime(3687, 8, 16)
            };

            var result = articles.AsQueryable().ApplyArticleFilter(filter).ToList();


            Assert.Equal(1, result.Count());
            Assert.Contains(articles.Where(a => a.Title == "Test 4").Single(), result);
        }

        [Fact]
        public void UsuallyValid()
        {
            var filter = new ArticleFilterViewModel
            {
                CategoryID = 64,
                FromDate = new DateTime(3687, 8, 7),
                ToDate = new DateTime(3687, 8, 7)
            };
            var result = filter.Validate(new System.ComponentModel.DataAnnotations.ValidationContext(filter)).ToList();

            Assert.Empty(result);
        }
        [Fact]
        public void ToDateEarlierThanFromDateCauseInvalid()
        {
            var filter = new ArticleFilterViewModel
            {
                FromDate = new DateTime(3687, 8, 7),
                ToDate = new DateTime(3687, 8, 6)
            };
            var result = filter.Validate(new System.ComponentModel.DataAnnotations.ValidationContext(filter)).ToList();

            Assert.Contains(result, r => r.MemberNames.Contains(nameof(filter.FromDate)));
            Assert.Contains(result, r => r.MemberNames.Contains(nameof(filter.ToDate)));
        }
    }
}
