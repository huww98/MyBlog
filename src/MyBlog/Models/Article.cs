using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MyBlog.Models
{
    public class Article
    {
        public int ID { get; set; }

        [Display(Name ="标题")]
        public string Title { get; set; }

        [DataType(DataType.Html)]
        [Display(Name = "内容")]
        public string Content { get; set; }

        [Display(Name = "作者")]
        public ApplicationUser Author { get; set; }

        [Display(Name ="创建日期")]
        public DateTime CreatedTime { get; set; }

        [Display(Name = "编辑日期")]
        public DateTime EditedTime { get; set; }

        [NotMapped]
        public bool CanEdit { get; set; }
    }
}
