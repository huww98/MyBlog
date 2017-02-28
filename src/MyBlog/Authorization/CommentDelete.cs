using Microsoft.AspNetCore.Authorization;
using MyBlog.Models;
using System.Security.Claims;
using System.Threading.Tasks;

namespace MyBlog.Authorization
{
    public class CanDeleteCommentRequirement : IAuthorizationRequirement
    {
    }
}