using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MyBlog.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyBlog.Controllers
{
    [Authorize(Roles = RoleInfo.AdministratorRoleName)]
    public class RoleManageController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RoleManageController(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> Index()
        {
            var authorRole = await _roleManager.FindByNameAsync(RoleInfo.AuthorRoleName);
            var editorRole = await _roleManager.FindByNameAsync(RoleInfo.EditorRoleName);
            var users = await _userManager.Users.Select(
                u => new RoleManageViewModel
                {
                    UserId = u.Id,
                    NickName = u.NickName,
                    Email = u.Email,
                    IsAuthor = u.Roles.Any(r => r.RoleId == authorRole.Id),
                    IsEditor = u.Roles.Any(r => r.RoleId == editorRole.Id)
                }).ToListAsync();
            return View(users);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRoles(RoleManageViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.FindByIdAsync(model.UserId);
                if (user == null)
                {
                    return NotFound();
                }
                var rawRoles = await _userManager.GetRolesAsync(user);
                await updateRole(user, rawRoles, RoleInfo.AuthorRoleName, model.IsAuthor);
                await updateRole(user, rawRoles, RoleInfo.EditorRoleName, model.IsEditor);
            }
            return RedirectToAction("Index");
        }

        private async Task updateRole(ApplicationUser user, IList<string> rawRoles, string roleName, bool isInRole)
        {
            bool rawIsInRole = rawRoles.Contains(roleName);
            if (rawIsInRole == isInRole)
            {
                return;
            }

            if (rawIsInRole)
            {
                await _userManager.RemoveFromRoleAsync(user, roleName);
            }
            else
            {
                await _userManager.AddToRoleAsync(user, roleName);
            }
        }
    }
}
