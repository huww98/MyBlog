using AngleSharp.Dom.Html;
using Ganss.XSS;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBlog.Authorization;
using MyBlog.Helpers;
using MyBlog.Models;
using MyBlog.Models.ArticleViewModels;
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

        public async Task<IActionResult> GetArticleListResult(
            IQueryable<Article> query,
            ArticleFilterViewModel filter,
            ArticleViewMode viewMode,
            bool showOnlyCanEdit = false)
        {
            query = query.OrderByDescending(a => a.CreatedTime);

            if (ModelState.IsValid)
            {
                query = query.ApplyArticleFilter(filter);
            }

            var list = query.ToList();

            foreach (var a in list)
            {
                a.CanEdit = await GetCanEdit(a);
            }
            if (showOnlyCanEdit)
            {
                list = list.Where(a => a.CanEdit).ToList();
            }
            ViewData["CanCreate"] = (await _authorizationService.AuthorizeAsync(User, "CanCreateArticle")).Succeeded;
            ViewData["ViewMode"] = viewMode;
            ViewData["Articles"] = list;
            return View("Index", filter);
        }

        // GET: Articles
        public async Task<IActionResult> Index(ArticleFilterViewModel filter, ArticleViewMode viewMode = ArticleViewMode.Summary)
        {
            IQueryable<Article> query = _context.Articles
                .AsNoTracking()
                .Include(a => a.Author).Where(a => a.Status == ArticleStatus.Published);
            return await GetArticleListResult(query, filter, viewMode);
        }

        public async Task<IActionResult> Drafts(ArticleFilterViewModel filter, ArticleViewMode viewMode = ArticleViewMode.Summary)
        {
            IQueryable<Article> query = _context.Articles
                .AsNoTracking()
                .Include(a => a.Author).Where(a => a.Status == ArticleStatus.Draft);
            return await GetArticleListResult(query, filter, viewMode, true);
        }

        // GET: Articles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = await GetArticleToShow(q => q.Where(a => a.ID == id));
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

            var article = await GetArticleToShow(q => q.Where(a => a.Slug == slug));
            if (article == null)
            {
                return NotFound();
            }

            return View("Details", article);
        }

        private async Task<Article> GetArticleToShow(Func<IQueryable<Article>, IQueryable<Article>> additionQuery)
        {
            IQueryable<Article> query = _context.Articles;
            query = additionQuery(query);
            var article = await query
               .Include(a => a.Categories).ThenInclude(ac => ac.Category)
               .Include(a => a.Author)
               .Include(a => a.Comments).ThenInclude(c => c.Author)
               .SingleOrDefaultAsync();
            if (article != null)
            {
                article.CanEdit = await GetCanEdit(article);
                if (article.Status == ArticleStatus.Draft && !article.CanEdit)
                {
                    return null;
                }
                foreach (var c in article.Comments)
                {
                    c.CanDelete = await GetCanDeleteComment(c);
                }
            }
            return article;
        }

        private async Task<bool> GetCanEdit(Article article)
            => (await _authorizationService.AuthorizeAsync(User, article, new CanEditArticleRequirement())).Succeeded;

        private async Task<bool> GetCanDeleteComment(Comment comment)
            => (await _authorizationService.AuthorizeAsync(User, comment, new CanDeleteCommentRequirement())).Succeeded;

        // GET: Articles/Create
        [Authorize(Roles = RoleInfo.AuthorRoleName)]
        public IActionResult Create()
        {
            ViewData["Images"] = _context.Images.ToList();
            return View();
        }

        // POST: Articles/Create
        [HttpPost]
        [Authorize(Roles = RoleInfo.AuthorRoleName)]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ICollection<int> categoryIDs, int draftId)
        {
            Article article;
            if (draftId != 0)
            {
                article = await _context.Articles
                    .Include(a => a.Categories)
                    .Include(a => a.Images)
                        .ThenInclude(ai => ai.Image)
                    .SingleOrDefaultAsync(a => a.ID == draftId);
            }
            else
            {
                article = new Article
                {
                    AuthorID = GetCurrentUserID(),
                };
                _context.Add(article);
            }
            article.CreatedTime = _currentTime.CurrentTime;
            article.Status = ArticleStatus.Published;
            var result = await UpdateArticle(article, categoryIDs);

            switch (result)
            {
                case OkResult r:
                    return RedirectToAction("Details", new { id = article.ID });

                case BadRequestResult r:
                    return View(article);

                default:
                    return result;
            }
        }

        // GET: Articles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var article = _context.Articles.AsNoTracking()
                .Include(a => a.Categories)
                .Include(a => a.DraftArticle).ThenInclude(a => a.Categories)
                .SingleOrDefault(m => m.ID == id); // use sync method as a workaround of https://github.com/aspnet/EntityFrameworkCore/issues/9038
            if (article == null)
            {
                return NotFound();
            }
            if (!await GetCanEdit(article))
            {
                return Unauthorized();
            }
            if (article.DraftArticle != null)
            {
                article = article.DraftArticle;
            }
            ViewData["CategoryIDs"] = article.Categories.Select(c => c.CategoryID).ToList();
            ViewData["Images"] = _context.Images.ToList();
            return View(article);
        }

        private async Task<IActionResult> UpdateArticle(Article articleToUpdate, ICollection<int> categoryIDs)
        {
            if (articleToUpdate == null)
            {
                return NotFound();
            }
            if (!await GetCanEdit(articleToUpdate))
            {
                return Unauthorized();
            }

            if (await TryUpdateModelAsync(articleToUpdate, string.Empty, a => a.Title, a => a.Slug, a => a.Content))
            {
                articleToUpdate.FinishEdit(_context.Images, categoryIDs, _currentTime.CurrentTime);
                if (TryValidateModel(articleToUpdate))
                {
                    await _context.SaveChangesAsync();
                    return Ok();
                }
            }
            return BadRequest(ModelState.Select(s => s.Value));
        }

        // POST: Articles/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ICollection<int> categoryIDs, ArticleStatus status)
        {
            var article = await _context.Articles
                    .Include(a => a.DraftArticle)
                    .Include(a => a.Categories)
                    .Include(a => a.Images)
                        .ThenInclude(ai => ai.Image)
                    .SingleOrDefaultAsync(a => a.ID == id);

            if (article?.DraftArticle != null)
            {
                _context.Remove(article.DraftArticle);
            }
            var result = await UpdateArticle(article, categoryIDs);

            switch (result)
            {
                case OkResult _:
                    return RedirectToAction("Details", new { id = article.ID });

                case BadRequestObjectResult _:
                    return View(article);

                default:
                    return result;
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveDraft(int draftID, ICollection<int> categoryIDs)
        {
            var article = await _context.Articles
                   .Include(a => a.Categories)
                   .Include(a => a.Images)
                       .ThenInclude(ai => ai.Image)
                   .SingleOrDefaultAsync(a => a.ID == draftID);

            var result = await UpdateArticle(article, categoryIDs);
            return GenerateJsonResult(article, result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDraft(int id, ICollection<int> categoryIDs)
        {
            Article draft;
            if (id == 0)
            {
                draft = new Article { Status = ArticleStatus.Draft, AuthorID = GetCurrentUserID() };
                _context.Add(draft);
            }
            else
            {
                var rawArticle = await _context.Articles
                    .Include(a => a.DraftArticle)
                        .ThenInclude(a => a.Categories)
                    .Include(a => a.DraftArticle)
                        .ThenInclude(a => a.Images)
                            .ThenInclude(ai => ai.Image)
                    .Where(a => a.ID == id).FirstOrDefaultAsync();
                if (rawArticle == null)
                {
                    return NotFound();
                }
                if (rawArticle.Status == ArticleStatus.Draft)
                {
                    return BadRequest("不能创建草稿的草稿。");
                }

                draft = rawArticle.DraftArticle ?? rawArticle.CreateDraft();
            }
            draft.CreatedTime = _currentTime.CurrentTime;
            var result = await UpdateArticle(draft, categoryIDs);
            return GenerateJsonResult(draft, result);
        }

        private IActionResult GenerateJsonResult(Article draft, IActionResult result)
        {
            switch (result)
            {
                case OkResult r:
                    return Json(new { IsSuccess = true, DraftId = draft.ID, Message = $"于{_currentTime.CurrentTime}保存草稿成功" });

                default:
                    return result;
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
            if (!await GetCanEdit(article))
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
            if (!await GetCanEdit(article))
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

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddComment(Comment comment)
        {
            if (ModelState.IsValid)
            {
                comment.CreatedTime = _currentTime.CurrentTime;
                comment.AuthorID = GetCurrentUserID();
                _context.Add(comment);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Details", new { id = comment.ArticleID });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteComment(int id)
        {
            var comment = await _context.Comments.Include(c => c.Article).SingleOrDefaultAsync(c => c.ID == id);
            if (comment == null)
            {
                return NotFound();
            }
            if (!await GetCanDeleteComment(comment))
            {
                return Unauthorized();
            }
            _context.Remove(comment);
            _context.SaveChanges();
            return RedirectToAction("Details", new { id = comment.ArticleID });
        }

        private string GetCurrentUserID()
            => User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier).Value;
    }
}
