using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyBlog.Data;
using System;
using System.Threading.Tasks;

namespace MyBlog.Models
{
    public static class SeedData
    {
        public const string AdministratorUserName = "Administrator";
        private const string administratorDefaultPassword = "a123456";
        public const string AdministratorRoleName = "Administrator";
        public const string EditorRoleName = "Editor";
        public const string AuthorRoleName = "Author";

        public static async Task Initialize(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            await context.Database.EnsureCreatedAsync();

            UserManager<ApplicationUser> userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var admin = await userManager.FindByNameAsync(AdministratorUserName);
            if (admin != null)
            {
                return;
            }

            admin = new ApplicationUser { UserName = AdministratorUserName };
            var result = await userManager.CreateAsync(admin, administratorDefaultPassword);
            if (result != IdentityResult.Success)
            {
                throw new Exception($"创建{AdministratorUserName}用户失败。{result.ToString()}");
            }

            RoleManager<IdentityRole> roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            await createRole(roleManager, AdministratorRoleName);
            await createRole(roleManager, EditorRoleName);
            await createRole(roleManager, AuthorRoleName);

            await addUserToRole(userManager, admin, AdministratorRoleName);
            await addUserToRole(userManager, admin, EditorRoleName);
            await addUserToRole(userManager, admin, AuthorRoleName);

            context.Articles.AddRange(
            new Article
            {
                Title = "Test Article",
                CreatedTime = new DateTime(2017, 1, 10, 16, 13, 21),
                EditedTime = new DateTime(2017, 1, 10, 16, 13, 21),
                Content = "<h4>Test Article</h4>\n<p>This is the first test article. This is within a 'p' tag.</p>",
                Author = admin
            },
            new Article
            {
                Title = "测试文章",
                CreatedTime = new DateTime(2017, 1, 11, 19, 24, 38),
                EditedTime = new DateTime(2017, 1, 12, 9, 14, 26),
                Content = "<h3>测试的文章标题-H3</h3>\n<p>这是一篇中文测试文章。</p>\n<p>这篇文章在创建的第二天被修改过。</p>",
                Author = admin
            }
            );
            context.SaveChanges();
        }

        private static async Task addUserToRole(UserManager<ApplicationUser> userManager, ApplicationUser user, string roleName)
        {
            var result = await userManager.AddToRoleAsync(user, roleName);
            if (result != IdentityResult.Success)
            {
                throw new Exception($"添加{user.UserName}用户至组{roleName}失败。{result.ToString()}");
            }
        }

        private static async Task createRole(RoleManager<IdentityRole> roleManager, string roleName)
        {
            var adminRole = new IdentityRole(roleName);
            var result = await roleManager.CreateAsync(adminRole);
            if (result != IdentityResult.Success)
            {
                throw new Exception($"创建组{roleName}失败。{result.ToString()}");
            }
        }
    }
}