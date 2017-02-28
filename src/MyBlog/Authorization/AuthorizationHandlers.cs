using Microsoft.AspNetCore.Authorization;
using MyBlog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MyBlog.Authorization
{
    internal class IsArticleAuthorAuthorizationHandler : IAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            if (context.User != null && context.User.IsInRole(RoleInfo.AuthorRoleName))
            {
                var currentUserID = context.User.FindFirst(c => c.Type == ClaimTypes.NameIdentifier).Value;
                if (context.Resource is Article && currentUserID == ((Article)context.Resource).AuthorID)
                {
                    var requirement = context.PendingRequirements.SingleOrDefault(r => r is CanEditArticleRequirement);
                    if (requirement != null)
                    {
                        context.Succeed(requirement);
                    }
                }
                else if (context.Resource is Comment)
                {
                    var comment = (Comment)context.Resource;
                    if (comment.AuthorID == currentUserID || comment.Article.AuthorID == currentUserID)
                    {
                        var requirement = context.PendingRequirements.SingleOrDefault(r => r is CanDeleteCommentRequirement);
                        if (requirement != null)
                        {
                            context.Succeed(requirement);
                        }
                    }
                }
            }
            return Task.CompletedTask;
        }
    }

    internal class IsEditorAuthorizationHandler : IAuthorizationHandler
    {
        public Task HandleAsync(AuthorizationHandlerContext context)
        {
            if (context.User != null && context.User.IsInRole(RoleInfo.EditorRoleName))
            {
                foreach (var r in context.PendingRequirements)
                {
                    if (r is CanEditArticleRequirement || r is CanDeleteCommentRequirement)
                    {
                        context.Succeed(r);
                    }
                }
            }
            return Task.CompletedTask;
        }
    }
}