using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBlog.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyBlog.Controllers
{
    [Authorize(Roles = RoleInfo.EditorRoleName)]
    public class CategoriesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public CategoriesController(ApplicationDbContext context)
        {
            _context = context;
        }

        private List<Category> SettleCategories(IEnumerable<Category> categories)
        {
            var roots = categories.Where(c => c.ParentCategory == null);
            var result = new List<Category>();
            foreach (var cate in roots)
            {
                cate.IndentLevel = 0;
                result.Add(cate);
                AddChildrenToList(cate, result);
            }
            return result;
        }

        private void AddChildrenToList(Category category, IList<Category> list)
        {
            foreach (var child in category.ChildCategories)
            {
                child.IndentLevel = category.IndentLevel + 1;
                list.Add(child);
                AddChildrenToList(child, list);
            }
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            var categories = _context.Categories;
            await categories.LoadAsync();
            return View(SettleCategories(categories));
        }

        // GET: Categories/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .Include(c => c.ParentCategory)
                .SingleOrDefaultAsync(m => m.ID == id);
            if (category == null)
            {
                return NotFound();
            }

            return View(category);
        }

        // GET: Categories/Create
        public async Task<IActionResult> Create()
        {
            await _context.Categories.LoadAsync();
            return View();
        }

        // POST: Categories/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ID,ParentCategoryID,Name")] Category category)
        {
            if (ModelState.IsValid)
            {
                _context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(category);
        }

        // GET: Categories/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Categories.SingleOrDefaultAsync(m => m.ID == id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var category = _context.Categories.SingleOrDefault(c => c.ID == id);
            if (category == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync(category, string.Empty, c => c.ParentCategoryID, c => c.Name))
            {
                if (!await IsParentCategoryValid(category))
                {
                    return BadRequest("父分类无效");
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(category);
        }

        // POST: Categories/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var category = await _context.Categories.SingleOrDefaultAsync(m => m.ID == id);
            if (category == null)
            {
                return NotFound();
            }
            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private async Task<bool> IsParentCategoryValid(Category category)
        {
            return !await IsAncestorOf(category.ParentCategory, category);
        }

        private async Task<bool> IsAncestorOf(Category category, Category ancestor)
        {
            await _context.Entry(category).Reference(c => c.ParentCategory).LoadAsync();
            if (category.ParentCategory == null)
            {
                return false;
            }
            if (category.ParentCategory == ancestor)
            {
                return true;
            }
            return await IsAncestorOf(category.ParentCategory, ancestor);
        }
    }
}
