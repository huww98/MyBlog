using AngleSharp.Dom.Html;
using Ganss.XSS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBlog.Authorization;
using MyBlog.Data;
using MyBlog.Helpers;
using MyBlog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MyBlog.Controllers
{
    public class ArticlesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly HtmlSanitizer _santitizer;

        public ArticlesController(UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IAuthorizationService authorizationService,
            HtmlSanitizer santitizer)
        {
            _userManager = userManager;
            _context = context;
            _authorizationService = authorizationService;
            _santitizer = santitizer;
        }

        // GET: Articles
        public async Task<IActionResult> Index()
        {
            var list = await _context.Articles
                .AsNoTracking()
                .Include(a => a.Author)
                .ToListAsync();

            foreach (var a in list)
            {
                a.CanEdit = await getCanEdit(a);
            }
            ViewData["CanCreate"] = User.IsInRole(SeedData.AuthorRoleName);
            return base.View(list);
        }

        // GET: Articles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _context.Articles
                .AsNoTracking()
                .Include(a => a.Author)
                .SingleOrDefaultAsync(a => a.ID == id);
            if (article == null)
            {
                return NotFound();
            }
            article.CanEdit = await getCanEdit(article);

            return View(article);
        }

        private async Task<bool> getCanEdit(Article article)
        {
            return await _authorizationService.AuthorizeAsync(User, article, new CanEditArticleRequirement());
        }

        // GET: Articles/Create
        [Authorize(Roles = SeedData.AuthorRoleName)]
        public IActionResult Create()
        {
            return View();
        }

        // POST: Articles/Create
        [HttpPost]
        [Authorize(Roles = SeedData.AuthorRoleName)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Content,Title")] Article article, ICollection<int> categoryIDs)
        {
            article.AuthorID = getCurrentUserID();
            article.CreatedTime = DateTime.Now;
            preprocessArticle(article, categoryIDs);
            _context.Add(article);
            if (TryValidateModel(article))
            {
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(article);
        }

        private void preprocessArticle(Article article, ICollection<int> categoryIDs)
        {
            article.EditedTime = DateTime.Now;
            IDictionary<string, Image> imageUsed;  //Key is the URL of the image.
            preprocessContent(article, out imageUsed);
            updateImageArticleLinks(article, imageUsed);
            updateCategoryArticleLinks(article, categoryIDs);
        }

        // GET: Articles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _context.Articles.AsNoTracking().Include(a => a.Categories).SingleOrDefaultAsync(m => m.ID == id);
            if (article == null)
            {
                return NotFound();
            }
            if (!await getCanEdit(article))
            {
                return Unauthorized();
            }
            ViewData["CategoryIDs"] = article.Categories.Select(c => c.CategoryID).ToList();
            return View(article);
        }

        // POST: Articles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ICollection<int> categoryIDs)
        {
            var article = await _context.Articles
                .Include(a => a.Categories)
                .Include(a => a.Images)
                    .ThenInclude(ai => ai.Image)
                .SingleOrDefaultAsync(a => a.ID == id);

            if (await TryUpdateModelAsync(article, string.Empty, a => a.ID, a => a.Title, a => a.Content))
            {
                if (id != article.ID)
                {
                    return NotFound();
                }
                if (!await getCanEdit(article))
                {
                    return Unauthorized();
                }
                preprocessArticle(article, categoryIDs);

                if (TryValidateModel(article))
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
            }
            return View(article);
        }

        private void updateCategoryArticleLinks(Article article, ICollection<int> categoryIDs)
        {
            if (article.Categories == null)
            {
                article.Categories = new List<ArticleCategory>();
            }
            CollectionUpdateHelper.updateCollection(
                article.Categories,
                ac => ac.CategoryID,
                categoryIDs.ToDictionary(id => id),
                id => new ArticleCategory { CategoryID = id });
        }

        private void updateImageArticleLinks(Article article, IDictionary<string, Image> imageUsed)
        {
            if (article.Images == null)
            {
                article.Images = new List<ArticleImage>();
            }
            CollectionUpdateHelper.updateCollection(article.Images, ai => ai.Image.Url, imageUsed, i => new ArticleImage { Image = i });
        }

        private void preprocessContent(Article article, out IDictionary<string, Image> usedImages)
        {
            article.Content = article.Content.Trim();
            List<string> imgSrcs = new List<string>();
            _santitizer.PostProcessNode += (s, e) =>
            {
                IHtmlImageElement ele = e.Node as IHtmlImageElement;
                if (ele != null)
                {
                    Uri uri = new Uri(ele.GetAttribute("src"), UriKind.RelativeOrAbsolute);
                    if (!uri.IsAbsoluteUri)
                    {
                        imgSrcs.Add(uri.OriginalString);
                    }
                }
            };
            article.Content = _santitizer.Sanitize(article.Content);
            usedImages = _context.Images.Where(i => imgSrcs.Distinct().Contains(i.Url)).ToDictionary(s => s.Url);

            if (usedImages.Count() == imgSrcs.Count)
            {
                return;
            }
            else
            {
                foreach (var item in imgSrcs)
                {
                    if (!usedImages.ContainsKey(item))
                    {
                        ModelState.AddModelError(string.Empty, $"图片{item}不存在或已被删除");
                    }
                }
            }
        }

        // GET: Articles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _context.Articles
                .Include(a => a.Author)
                .SingleOrDefaultAsync(m => m.ID == id);

            if (article == null)
            {
                return NotFound();
            }
            if (!await getCanEdit(article))
            {
                return Unauthorized();
            }

            return View(article);
        }

        // POST: Articles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, bool? ifDeleteImages)
        {
            var query = _context.Articles.Include(a => a.Images);
            if (ifDeleteImages == true)
            {
                query = query
                    .ThenInclude(ai => ai.Image)
                    .ThenInclude(i => i.Articles);
            }
            var article = await query.SingleOrDefaultAsync(a => a.ID == id);

            if (article == null)
            {
                return NotFound();
            }
            if (!await getCanEdit(article))
            {
                return Unauthorized();
            }

            if (ifDeleteImages == true)
            {
                foreach (var i in article.Images)
                {
                    if (i.Image.Articles.Count <= 1)
                    {
                        System.IO.File.Delete(i.Image.Path);
                        _context.Remove(i.Image);
                    }
                }
            }
            _context.Remove(article);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private string getCurrentUserID()
        {
            return User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier).Value;
        }
    }
}