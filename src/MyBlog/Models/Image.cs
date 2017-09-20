using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBlog.Models
{
    public class Image
    {
        public int ID { get; set; }

        [Display(Name = "替代文本")]
        public string Alt { get; set; }

        [Display(Name = "描述")]
        public string Description { get; set; }

        [MaxLength(20)]
        [Required]
        public byte[] SHA1 { get; set; }

        [Required]
        public string Path { get; set; }

        [Required]
        [DataType(DataType.ImageUrl)]
        public string Url { get; set; }

        [Required]
        [Display(Name = "上传时间")]
        public DateTime UploadedTime { get; set; }

        public ICollection<ArticleImage> Articles { get; } = new List<ArticleImage>();
    }

    public class ArticleImage
    {
        [ForeignKey(nameof(Article))]
        public int ArticleID { get; set; }

        public Article Article { get; set; }

        [ForeignKey(nameof(Image))]
        public int ImageID { get; set; }

        public Image Image { get; set; }
    }
}