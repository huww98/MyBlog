using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MyBlog.Models;
using Microsoft.EntityFrameworkCore;
using MyBlog.Controllers;
using MyBlog.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Xunit;
using MyBlog.Models.ArticleViewModels;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using MyBlog.Authorization;

namespace MyBlog.Tests
{
    public class ArticlesControllerTests
    {
        private Mock<ICurrentTime> currentTimeMock;
        private Mock<IAuthorizationService> authMock;
        private DateTime mockedCurrentTime = new DateTime(3079, 3, 9, 8, 7, 34, 687);

        private ArticlesController buildController(Action<IServiceCollection> buildServices = null)
        {
            var services = new ServiceCollection();
            buildServices?.Invoke(services);

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("test"));
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            currentTimeMock = new Mock<ICurrentTime>();
            currentTimeMock.Setup(t => t.CurrentTime).Returns(mockedCurrentTime);
            services.AddSingleton(currentTimeMock.Object);

            if (!services.Any(service => service.ServiceType == typeof(IAuthorizationService)))
            {
                authMock = new Mock<IAuthorizationService>();
                authMock.Setup(i => i.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.Is<Article>(a => a.Title == "Test Article"), It.Is<IEnumerable<IAuthorizationRequirement>>(arg => arg.Single() is CanEditArticleRequirement)))
                    .Returns(Task.FromResult(AuthorizationResult.Success()));
                authMock.Setup(i => i.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.Is<Article>(a => a.Title == "测试文章"), It.Is<IEnumerable<IAuthorizationRequirement>>(arg => arg.Single() is CanEditArticleRequirement)))
                    .Returns(Task.FromResult(AuthorizationResult.Failed()));
                authMock.Setup(i => i.AuthorizeAsync(It.IsAny<ClaimsPrincipal>(), It.IsAny<object>(), "CanCreateArticle"))
                    .Returns(Task.FromResult(AuthorizationResult.Success()));
                services.AddSingleton(authMock.Object);
            }
            var serviceProvider = services.BuildServiceProvider();

            addTestArticles(serviceProvider.GetRequiredService<ApplicationDbContext>());

            return ActivatorUtilities.CreateInstance<ArticlesController>(serviceProvider);
        }

        private void addTestArticles(ApplicationDbContext dbContext)
        {
            dbContext.Database.EnsureDeleted();
            dbContext.Articles.AddRange(
            new Article
            {
                Title = "Test Article",
                CreatedTime = new DateTime(2017, 1, 10, 16, 13, 21),
                EditedTime = new DateTime(2017, 1, 10, 16, 13, 21),
                Content = "<h4>Test Article</h4>\n<p>This is the first test article. This is within a 'p' tag.</p>",
                Status = ArticleStatus.Published,
            },
            new Article
            {
                Title = "测试文章",
                CreatedTime = new DateTime(2017, 1, 11, 19, 24, 38),
                EditedTime = new DateTime(2017, 1, 12, 9, 14, 26),
                Content = "<h3>测试的文章标题-H3</h3>\n<p>这是一篇中文测试文章。</p>\n<p>这篇文章在创建的第二天被修改过。</p>",
                Status = ArticleStatus.Published,
            },
            new Article
            {
                Title = "Test Article",
                CreatedTime = new DateTime(2017, 1, 15, 16, 13, 29),
                EditedTime = new DateTime(2017, 1, 15, 16, 13, 29),
                Content = "<h4>Test Article</h4>\n<p>Modified Draft</p>",
                Status = ArticleStatus.Draft,
            },
            new Article
            {
                Title = "测试文章",
                CreatedTime = new DateTime(2017, 1, 17, 19, 24, 38),
                EditedTime = new DateTime(2017, 1, 23, 9, 14, 26),
                Content = "<h3>测试的文章标题-H3</h3>\n<p>这是一篇中文测试文章。</p>\n<p>这是草稿。</p>",
                Status = ArticleStatus.Draft,
            }
            );
            dbContext.SaveChanges();
        }

        [Fact]
        public async Task Index_WithoutDraft_PreservesParameters_CheckPermitions()
        {
            var controller = buildController();
            var filter = new ArticleFilterViewModel();
            var result = await controller.Index(filter, ArticleViewMode.List);

            var viewResult = Assert.IsType<ViewResult>(result);
            //ReturnsAllArticles
            var articles = Assert.IsAssignableFrom<IEnumerable<Article>>(viewResult.ViewData["Articles"]);
            Assert.Equal(2, articles.Count());
            Assert.Contains(articles, m => m.Title == "Test Article");
            Assert.Contains(articles, m => m.Title == "测试文章");
            //PreservesParameters
            Assert.Equal(filter, viewResult.Model);
            Assert.Equal(ArticleViewMode.List, viewResult.ViewData["ViewMode"]);
            //CheckPermitions
            authMock.VerifyAll();
            Assert.Equal(true, articles.Where(a => a.Title == "Test Article").Single().CanEdit);
            Assert.Equal(false, articles.Where(a => a.Title == "测试文章").Single().CanEdit);
            Assert.Equal(true, viewResult.ViewData["CanCreate"]);
        }

        [Fact]
        public async Task IndexReturnsAllArticlesWhenFilterIsInvalid()
        {
            var controller = buildController();
            controller.ModelState.AddModelError("ToDate", "Some error");
            var result = await controller.Index(new ArticleFilterViewModel { FromDate = new DateTime(2017, 1, 11) });

            var viewResult = Assert.IsType<ViewResult>(result);
            var articles = Assert.IsAssignableFrom<IEnumerable<Article>>(viewResult.ViewData["Articles"]);
            Assert.Equal(2, articles.Count());
            Assert.Contains(articles, m => m.Title == "Test Article");
            Assert.Contains(articles, m => m.Title == "测试文章");
        }

        [Fact]
        public async Task IndexAppliesVaildFilter()
        {
            var controller = buildController();
            var result = await controller.Index(new ArticleFilterViewModel { FromDate = new DateTime(2017, 1, 11) });

            var viewResult = Assert.IsType<ViewResult>(result);
            var articles = Assert.IsAssignableFrom<IEnumerable<Article>>(viewResult.ViewData["Articles"]);
            Assert.Equal(1, articles.Count());
            Assert.Contains(articles, m => m.Title == "测试文章");
        }

        [Fact]
        public async Task Draft_ReturnEditableDrafts()
        {
            var controller = buildController();
            var filter = new ArticleFilterViewModel();
            var result = await controller.Drafts(filter);

            var viewResult = Assert.IsType<ViewResult>(result);
            var articles = Assert.IsAssignableFrom<IEnumerable<Article>>(viewResult.ViewData["Articles"]);

            Assert.Equal(1, articles.Count());
            Assert.True(articles.All(a => a.Status == ArticleStatus.Draft));
            Assert.Contains(articles, m => m.Title == "Test Article");
        }
    }
}