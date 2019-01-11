using MyBlog.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using AspNet.Security.OAuth.QQ;
using AspNet.Security.OAuth.GitHub;
using System.Security.Claims;

namespace MyBlog.Helpers
{
    public static class UpdateExternalUserInfoHelper
    {
        internal static void Update(ApplicationUser user, ExternalLoginInfo info)
        {
            if (info.LoginProvider == QQAuthenticationDefaults.AuthenticationScheme)
            {
                user.NickName = info.Principal.Identity.Name;
                if (Uri.TryCreate(info.Principal.FindFirstValue("urn:qq:figureurl_qq_2"), UriKind.Absolute, out var uri))
                {
                    user.AvatarUrl = "//" + uri.GetComponents(UriComponents.AbsoluteUri & ~UriComponents.Scheme, UriFormat.UriEscaped);
                }

            }
            if (info.LoginProvider == GitHubAuthenticationDefaults.AuthenticationScheme)
            {
                user.NickName = info.Principal.Identity.Name;
            }
        }
    }
}
