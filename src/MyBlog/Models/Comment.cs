using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MyBlog.Models
{
    public class Comment
    {
        public int ID { get; set; }

        public Article Article { get; set; }

        [ForeignKey(nameof(Article))]
        public int ArticleID { get; set; }

        public Comment ParentComment { get; set; }

        public int? ParentCommentID { get; set; }

        public ICollection<Comment> ChildrenComments { get; } = new List<Comment>();

        [Required]
        public string Content { get; set; }

        [ForeignKey(nameof(Author))]
        public string AuthorID { get; set; }

        public ApplicationUser Author { get; set; }

        public DateTime CreatedTime { get; set; }

        [NotMapped]
        public bool CanDelete { get; set; }
    }
}