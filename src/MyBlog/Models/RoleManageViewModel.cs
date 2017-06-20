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
                return $"{NickName} ({Email})";
            }
        }

        public string NickName { get; set; }

        public string Email { get; set; }

        [Display(Name = "作者")]
        public bool IsAuthor { get; set; }

        [Display(Name = "编辑")]
        public bool IsEditor { get; set; }
    }
}