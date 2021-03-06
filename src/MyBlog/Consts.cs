using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyBlog
{
    public static class ImagePath
    {
        public static string StoragePath { get; } = System.IO.Path.Combine("wwwroot", "UploadedImages");

        public const string UrlPath = "/UploadedImages";
    }

    public static class RoleInfo
    {
        public const string AdministratorUserName = "Administrator";
        public const string AdministratorRoleName = "Administrator";
        public const string EditorRoleName = "Editor";
        public const string AuthorRoleName = "Author";
    }
}
