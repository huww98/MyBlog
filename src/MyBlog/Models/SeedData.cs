using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MyBlog.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyBlog.Models
{
    public static class SeedData
    {
        public const string AdministratorUserName = "Administrator";
        const string administratorDefaultPassword = "a123456";
        public const string AdministratorRoleName = "Administrator";
        public const string EditorRoleName = "Editor";
        public const string AuthorRoleName = "Author";
        public static async Task Initialize(IServiceProvider serviceProvider)
        {

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

            result = await userManager.AddToRoleAsync(admin, AdministratorRoleName);
            if (result != IdentityResult.Success)
            {
                throw new Exception($"添加{AdministratorUserName}用户至组{AdministratorRoleName}失败。{result.ToString()}");
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
