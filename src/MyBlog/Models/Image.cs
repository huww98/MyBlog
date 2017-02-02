using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MyBlog.Models
{
    public class Image
    {
        public int ID { get; set; }

        public string Alt { get; set; }

        public string Discription { get; set; }

        [MaxLength(20)]
        public byte[] SHA1 { get; set; }

        [Required]
        public string Path { get; set; }

        [Required]
        public string Url { get; set; }

        public ICollection<ArticleImage> Articles { get; set; }
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