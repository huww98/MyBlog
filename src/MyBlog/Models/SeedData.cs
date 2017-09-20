using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyBlog.Models;
using System;
using System.Threading.Tasks;

namespace MyBlog.Models
{
    public static class SeedData
    {
        private const string administratorDefaultPassword = "a123456";

        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();
            await context.Database.MigrateAsync();

            UserManager<ApplicationUser> userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var admin = await userManager.FindByNameAsync(RoleInfo.AdministratorUserName);
            if (admin != null)
            {
                return;
            }

            admin = new ApplicationUser { UserName = RoleInfo.AdministratorUserName };
            var result = await userManager.CreateAsync(admin, administratorDefaultPassword);
            if (result != IdentityResult.Success)
            {
                throw new Exception($"创建{RoleInfo.AdministratorUserName}用户失败。{result.ToString()}");
            }

            RoleManager<IdentityRole> roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            await createRole(roleManager, RoleInfo.AdministratorRoleName);
            await createRole(roleManager, RoleInfo.EditorRoleName);
            await createRole(roleManager, RoleInfo.AuthorRoleName);

            await addUserToRole(userManager, admin, RoleInfo.AdministratorRoleName);
            await addUserToRole(userManager, admin, RoleInfo.EditorRoleName);
            await addUserToRole(userManager, admin, RoleInfo.AuthorRoleName);

            context.Articles.AddRange(
            new Article
            {
                Title = "Test Article",
                CreatedTime = new DateTime(2017, 1, 10, 16, 13, 21),
                EditedTime = new DateTime(2017, 1, 10, 16, 13, 21),
                Content = "## Test Article\nThis is the first test article. support **Markdown**!",
                Author = admin
            },
            new Article
            {
                Title = "测试文章",
                CreatedTime = new DateTime(2017, 1, 11, 19, 24, 38),
                EditedTime = new DateTime(2017, 1, 12, 9, 14, 26),
                Content = "### 测试的文章标题-H3\n这是一篇中文测试文章。\n\n这篇文章在创建的第二天被修改过。",
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