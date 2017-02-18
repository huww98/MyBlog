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
                addChildrenToList(cate, result);
            }
            return result;
        }

        private void addChildrenToList(Category category, IList<Category> list)
        {
            foreach (var child in category.ChildCategories)
            {
                child.IndentLevel = category.IndentLevel + 1;
                list.Add(child);
                addChildrenToList(child, list);
            }
        }

        // GET: Categories
        public async Task<IActionResult> Index()
        {
            var categories = _context.Category;
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

            var category = await _context.Category
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
            await _context.Category.LoadAsync();
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

            var category = await _context.Category.SingleOrDefaultAsync(m => m.ID == id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        // POST: Categories/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ID,ParentCategoryID,Name")] Category category)
        {
            if (id != category.ID)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(category);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CategoryExists(category.ID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index");
            }
            return View(category);
        }

        // GET: Categories/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var category = await _context.Category.SingleOrDefaultAsync(m => m.ID == id);
            _context.Category.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index");
        }

        private bool CategoryExists(int id)
        {
            return _context.Category.Any(e => e.ID == id);
        }

        private async Task<List<Category>> GetValidParentCategories(Category category)
        {
            var categories = await _context.Category.ToListAsync();
            categories.Remove(category);
            removeChildren(categories, category);
            return categories;
        }

        private void removeChildren(ICollection<Category> categories, Category toRemoveChildren)
        {
            var children = categories.Where(c => c.ParentCategory == toRemoveChildren).ToList();
            foreach (var child in children)
            {
                categories.Remove(child);
                removeChildren(categories, child);
            }
        }

        private async Task<bool> isParentCategoryValid(Category category)
        {
            return category != category.ParentCategory && !await isAncestorOf(category.ParentCategory, category);
        }

        private async Task<bool> isAncestorOf(Category category, Category ancestor)
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
            return await isAncestorOf(category.ParentCategory, ancestor);
        }
    }
}