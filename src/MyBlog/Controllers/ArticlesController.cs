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

namespace MyBlog.Controllers
{
    public class ArticlesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IAuthorizationService _authorizationService;

        public ArticlesController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, IAuthorizationService authorizationService)
        {
            _userManager = userManager;
            _context = context;
            _authorizationService = authorizationService;
        }

        // GET: Articles
        public async Task<IActionResult> Index()
        {
            var list = await _context.Article
                .Include(a => a.Author)
                .AsNoTracking()
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

            var article = await _context.Article.Include(a => a.Author).SingleOrDefaultAsync(m => m.ID == id);
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
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [Authorize(Roles = SeedData.AuthorRoleName)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Content,Title")] Article article)
        {
            article.Author = await GetCurrentUserAsync();
            article.CreatedTime = DateTime.Now;
            article.EditedTime = DateTime.Now;
            if (ModelState.IsValid)
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

            var article = await _context.Article.Include(a => a.Author).SingleOrDefaultAsync(m => m.ID == id);
            if (article == null)
            {
                return NotFound();
            }
            if (!await getCanEdit(article))
            {
                return Challenge();
            }

            return View(article);
        }

        // POST: Articles/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            int id,
            [Bind(nameof(Article.ID), nameof(Article.Title), nameof(Article.Content))] Article article)
        {
            if (id != article.ID)
            {
                return NotFound();
            }
            article.EditedTime = DateTime.Now;

            if (ModelState.IsValid)
            {

                Article articleFromDB = getArticle(article.ID);
                if (articleFromDB == null)
                {
                    return NotFound();
                }
                if (!await getCanEdit(articleFromDB))
                {
                    return Challenge();
                }

                var entity = _context.Attach(article);
                entity.Property(a => a.Title).IsModified = true;
                entity.Property(a => a.Content).IsModified = true;
                entity.Property(a => a.EditedTime).IsModified = true;
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }
            return View(article);
        }

        private Article getArticle(int articleID)
        {
            return _context.Article.Include(a => a.Author).AsNoTracking().SingleOrDefault(a => a.ID == articleID);
        }

        // GET: Articles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await _context.Article.Include(a=>a.Author).SingleOrDefaultAsync(m => m.ID == id);

            if (article == null)
            {
                return NotFound();
            }
            if (!await getCanEdit(article))
            {
                return Challenge();
            }

            return View(article);
        }

        // POST: Articles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var article = await _context.Article.Include(a=>a.Author).SingleOrDefaultAsync(m => m.ID == id);

            if (article==null)
            {
                return NotFound();
            }
            if (!await getCanEdit(article))
            {
                return Challenge();
            }

            _context.Article.Remove(article);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool ArticleExists(int id)
        {
            return _context.Article.Any(e => e.ID == id);
        }

        private Task<ApplicationUser> GetCurrentUserAsync()
        {
            return _userManager.GetUserAsync(User);
        }
    }
}
