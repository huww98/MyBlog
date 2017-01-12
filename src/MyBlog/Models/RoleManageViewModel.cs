using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyBlog.Models
{
    public class RoleManageViewModel
    {
        public string UserId { get; set; }

        [Display(Name ="用户")]
        public string UserDisplayName { get; set; }

        [Display(Name = "作者")]
        public bool IsAuthor { get; set; }

        [Display(Name = "编辑")]
        public bool IsEditor { get; set; }
    }
}
