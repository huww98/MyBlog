using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBlog.Models
{
    public class Category
    {
        public int ID { get; set; }

        public int? ParentCategoryID { get; set; }

        [Display(Name = "父分类")]
        public Category ParentCategory { get; set; }

        public ICollection<Category> ChildCategories { get; } = new List<Category>();

        [Display(Name = "名称")]
        [StringLength(256)]
        [Required]
        public string Name { get; set; }

        [Display(Name = "别名")]
        [StringLength(256)]
        public string Slot { get; set; }

        [NotMapped]
        public int IndentLevel { get; set; }
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
