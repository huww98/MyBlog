using Microsoft.AspNetCore.Authorization;
using MyBlog.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MyBlog.Authorization
{
    public class CanEditArticleRequirement : IAuthorizationRequirement
    {
    }

    internal class IsArticleAuthorAuthorizationHandler : AuthorizationHandler<CanEditArticleRequirement, Article>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CanEditArticleRequirement requirement, Article article)
        {
            if (context.User.IsInRole(SeedData.AuthorRoleName) && context.User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier).Value == article.AuthorID)
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }

    internal class IsEditorAuthorizationHandler : AuthorizationHandler<CanEditArticleRequirement, Article>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, CanEditArticleRequirement requirement, Article article)
        {
            if (context.User.IsInRole(SeedData.EditorRoleName))
            {
                context.Succeed(requirement);
            }
            return Task.CompletedTask;
        }
    }
}