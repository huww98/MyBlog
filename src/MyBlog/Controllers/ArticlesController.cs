using AngleSharp.Dom.Html;
using Ganss.XSS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBlog.Authorization;
using MyBlog.Helpers;
using MyBlog.Models;
using MyBlog.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MyBlog.Controllers
{
    public enum ArticleViewMode { Summary, List }

    public class ArticlesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuthorizationService _authorizationService;
        private readonly ICurrentTime _currentTime;

        public ArticlesController(UserManager<ApplicationUser> userManager,
            ApplicationDbContext context,
            IAuthorizationService authorizationService,
            ICurrentTime currentTime)
        {
            _userManager = userManager;
            _context = context;
            _authorizationService = authorizationService;
            _currentTime = currentTime;
        }

        // GET: Articles
        public async Task<IActionResult> Index(ArticleViewMode viewMode = ArticleViewMode.Summary)
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
            ViewData["ViewMode"] = viewMode;
            return base.View(list);
        }

        // GET: Articles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await getArticleToShow(q => q.Where(a => a.ID == id));
            if (article == null)
            {
                return NotFound();
            }

            return View(article);
        }
        public async Task<IActionResult> Slug(string slug)
        {
            if (string.IsNullOrEmpty(slug))
            {
                return NotFound();
            }

            var article = await getArticleToShow(q => q.Where(a => a.Slug == slug));
            if (article == null)
            {
                return NotFound();
            }

            return View("Details", article);
        }

        private async Task<Article> getArticleToShow(Func<IQueryable<Article>, IQueryable<Article>> additionQuery)
        {
            var query = _context.Articles.AsNoTracking();
            query = additionQuery(query);
            var article = await query
               .Include(a => a.Categories).ThenInclude(ac => ac.Category)
               .Include(a => a.Author)
               .SingleOrDefaultAsync();
            if (article != null)
            {
                article.CanEdit = await getCanEdit(article);
            }
            return article;
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
        public async Task<IActionResult> Create([Bind("Content,Title,Slug")] Article article, ICollection<int> categoryIDs)
        {
            article.AuthorID = getCurrentUserID();
            var result = article.FinishCreate(_context, categoryIDs, _currentTime.Time);
            processValidationResult(result);
            if (TryValidateModel(article))
            {
                _context.Add(article);
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

            if (await TryUpdateModelAsync(article, string.Empty, a => a.ID, a => a.Title, a => a.Slug, a => a.Content))
            {
                if (id != article.ID)
                {
                    return NotFound();
                }
                if (!await getCanEdit(article))
                {
                    return Unauthorized();
                }
                var result = article.FinishEdit(_context, categoryIDs, _currentTime.Time);
                processValidationResult(result);
                if (TryValidateModel(article))
                {
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Index");
                }
            }
            return View(article);
        }

        private void processValidationResult(ValidationResult result)
        {
            if (!result.IsSucceeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(error.Key, error.Value);
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