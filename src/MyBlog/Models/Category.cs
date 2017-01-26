using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MyBlog.Models
{
    public class Category
    {
        public int ID { get; set; }

        [ForeignKey(nameof(ParentCategory))]
        public int? ParentCategoryID { get; set; }

        [Display(Name = "父分类")]
        public Category ParentCategory { get; set; }

        [Display(Name = "名称")]
        [StringLength(256)]
        [Required]
        public string Name { get; set; }
    }

    public class ArticleCategory
    {
        [ForeignKey(nameof(Article))]
        public int ArticleID { get; set; }
        public Article Article { get; set; }

        [ForeignKey(nameof(Category))]
        public int CategoryID { get; set; }
        public Category Category { get; set; }

    }
}
