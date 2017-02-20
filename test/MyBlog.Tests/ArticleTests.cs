using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyBlog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace MyBlog.Tests
{
    public class ArticleTests
    {
        [Fact]
        public void FinishArticleEdit_UpdateEditedTime_UpdateImageAndCategory_valid()
        {
            var article = new Article { Content = $"<img src='{ImagePath.UrlPath}/2.jpg'/><img src='http://somehost/3.jpg'/>" };
            article.Categories.Add(new ArticleCategory { CategoryID = 1 });
            article.Images.Add(new ArticleImage { ImageID = 1, Image = new Image { ID = 1, Url = $"{ImagePath.UrlPath}/1.jpg" } });
            var images = new List<Image> { new Image { ID = 2, Url = $"{ImagePath.UrlPath}/2.jpg" } };
            var fakeCurrentTime = new DateTime(2598, 8, 28, 16, 8, 7, 988);

            var validateResult1 = article.Validate(new System.ComponentModel.DataAnnotations.ValidationContext(article));
            article.FinishEdit(images.AsQueryable(), new List<int> { 2 }, fakeCurrentTime);
            var validateResult2 = article.Validate(new System.ComponentModel.DataAnnotations.ValidationContext(article));

            Assert.Empty(validateResult1);
            Assert.Equal(fakeCurrentTime, article.EditedTime);
            Assert.Equal(1, article.Images.Count);
            Assert.Contains(article.Images, i => i.Image.ID == 2);
            Assert.Equal(1, article.Categories.Count);
            Assert.Contains(article.Categories, c => c.CategoryID == 2);
            Assert.Empty(validateResult2);
        }

        [Fact]
        public void FinishArticleCreate_UpdateCreatedTime()
        {
            var article = new Article();
            var fakeCurrentTime = new DateTime(2598, 8, 28, 16, 8, 7, 988);

            article.FinishCreate((new Image[0]).AsQueryable(), new int[0], fakeCurrentTime);

            Assert.Equal(fakeCurrentTime, article.CreatedTime);
        }

        [Fact]
        public void FinishArticleEdit_NonExistentImageCauseInvalid()
        {
            var article = new Article { Content = $"<img src='{ImagePath.UrlPath}/1.jpg'/><img src='{ImagePath.UrlPath}/2.jpg'/>" };
            var images = new List<Image> { new Image { ID = 1, Url = $"{ImagePath.UrlPath}/1.jpg" } };
            var fakeCurrentTime = new DateTime(2598, 8, 28, 16, 8, 7, 988);

            article.FinishEdit(images.AsQueryable(), new List<int>(), fakeCurrentTime);
            var validateResult = article.Validate(new System.ComponentModel.DataAnnotations.ValidationContext(article));

            Assert.Equal(fakeCurrentTime, article.EditedTime);
            Assert.Equal(1, article.Images.Count);
            Assert.Contains(article.Images, i => i.Image.ID == 1);
            Assert.Contains(validateResult,r=>r.MemberNames.Contains(nameof(article.Content)));
        }
    }
}
