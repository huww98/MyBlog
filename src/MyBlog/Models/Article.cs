using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBlog.Models
{
    public class Article
    {
        public int ID { get; set; }

        [Display(Name = "标题")]
        [Required]
        public string Title { get; set; }

        [DataType(DataType.Html)]
        [Display(Name = "内容")]
        [StringLength(Microsoft.AspNetCore.WebUtilities.FormReader.DefaultValueLengthLimit)]
        [Required]
        public string Content { get; set; }

        [Display(Name = "作者")]
        public ApplicationUser Author { get; set; }

        [ForeignKey(nameof(Author))]
        public string AuthorID { get; set; }

        [Display(Name = "创建日期")]
        public DateTime CreatedTime { get; set; }

        [Display(Name = "编辑日期")]
        public DateTime EditedTime { get; set; }

        public ICollection<ArticleImage> Images { get; set; }

        [Display(Name = "分类")]
        public ICollection<ArticleCategory> Categories { get; set; }

        [NotMapped]
        public bool CanEdit { get; set; }
    }
}