using System.ComponentModel.DataAnnotations;

namespace MyBlog.Models
{
    public class RoleManageViewModel
    {
        public string UserId { get; set; }

        [Display(Name = "用户")]
        public string UserDisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(Email))
                {
                    return UserName;
                }
                else
                {
                    return $"{UserName} ({Email})";
                }
            }
        }

        public string UserName { get; set; }

        public string Email { get; set; }

        [Display(Name = "作者")]
        public bool IsAuthor { get; set; }

        [Display(Name = "编辑")]
        public bool IsEditor { get; set; }
    }
}