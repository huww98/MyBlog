using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyBlog.Data;
using MyBlog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using MyBlog.Authorization;
using System.Diagnostics;
using System.Security.Claims;
using Ganss.XSS;
using AngleSharp.Dom.Html;

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
        public async Task<IActionResult> Create([Bind("Content,Title")] Article article)
        {
            article.AuthorID = getCurrentUserID();
            article.CreatedTime = DateTime.Now;
            article.EditedTime = DateTime.Now;
            IDictionary<string, Image> imageUsed;
            preprocessContent(article, out imageUsed);
            if (TryValidateModel(article))
            {
                _context.Add(article);
                article.Images = new List<ArticleImage>();
                updateImageArticleLinks(article, imageUsed);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(article);
        }

        // GET: Articles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _context.Articles.AsNoTracking().SingleOrDefaultAsync(m => m.ID == id);
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

        // POST: Articles/Edit/5
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditArticle(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _context.Articles
                .Include(a => a.Images)
                    .ThenInclude(ai => ai.Image)
                .SingleOrDefaultAsync(a => a.ID == id);

            if (await TryUpdateModelAsync(article, string.Empty, a => a.ID, a => a.Title, a => a.Content))
            {
                if (id != article.ID)
                {
                    return NotFound();
                }
                article.EditedTime = DateTime.Now;
                IDictionary<string, Image> imageUsed;  //Key is the URL of the image.
                preprocessContent(article, out imageUsed);

                if (TryValidateModel(article))
                {
                    if (!await getCanEdit(article))
                    {
                        return Unauthorized();
                    }

                    updateImageArticleLinks(article, imageUsed);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
            }
            return View(article);
        }

        private void updateImageArticleLinks(Article article, IDictionary<string, Image> imageUsed)
        {
            List<ArticleImage> imageDeleted = new List<ArticleImage>();
            foreach (var i in article.Images)
            {
                if (imageUsed.ContainsKey(i.Image.Url))
                {
                    imageUsed.Remove(i.Image.Url);
                }
                else
                {
                    imageDeleted.Add(i);
                }
            }

            foreach (var i in imageDeleted)
            {
                article.Images.Remove(i);
            }
            foreach (var i in imageUsed.Values)
            {
                article.Images.Add(new ArticleImage { Image = i });
            }
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
        public async Task<IActionResult> DeleteConfirmed(int? id, bool? ifDeleteImages)
        {
            if (id == null)
            {
                return NotFound();
            }

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
