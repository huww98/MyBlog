using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MyBlog.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using MyBlog.Services;

namespace MyBlog.Controllers
{
    [Authorize(Roles = RoleInfo.EditorRoleName)]
    public class ImagesController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ImagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Images
        public async Task<IActionResult> Index()
        {
            return View(await _context.Images.Include(i => i.Articles).ThenInclude(ai => ai.Article).ToListAsync());
        }

        // GET: Images/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Images/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Alt,Discription")] Image image, IFormFile file, [FromServices]IImageProcessor processor)
        {
            if (file == null)
            {
                ModelState.AddModelError(string.Empty, "必须选择文件");
                return View(image);
            }
            using (var stream = file.OpenReadStream())
            {
                await processor.SaveImageAsync(file.FileName, stream, image);
            }

            return RedirectToAction("Index");
        }

        // GET: Images/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var image = await _context.Images.SingleOrDefaultAsync(m => m.ID == id);
            if (image == null)
            {
                return NotFound();
            }
            return View(image);
        }

        // POST: Images/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id)
        {
            var image = _context.Images.SingleOrDefault(i => i.ID == id);
            if (image == null)
            {
                return NotFound();
            }

            if (await TryUpdateModelAsync(image, string.Empty, i => i.ID, i => i.Alt, i => i.Discription))
            {
                if (id != image.ID)
                {
                    return NotFound();
                }

                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(image);
        }

        // POST: Images/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, [FromServices]IImageProcessor processor)
        {
            var result = await processor.DeleteImageAsync(id);
            switch (result)
            {
                case ImageDeleteResult.NotFount:
                    return NotFound();

                case ImageDeleteResult.InUse:
                    return BadRequest();

                case ImageDeleteResult.Successed:
                    return RedirectToAction("Index");
            }

            return StatusCode(500);
        }
    }
}