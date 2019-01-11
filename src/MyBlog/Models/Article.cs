using AngleSharp.Dom.Html;
using Ganss.XSS;
using MyBlog.Controllers;
using MyBlog.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text.RegularExpressions;

namespace MyBlog.Models
{
    public enum ArticleStatus
    {
        Published, Draft
    }

    public class Article : IValidatableObject
    {
        public int ID { get; set; }

        [Display(Name = "标题")]
        [Required]
        [StringLength(255)]
        public string Title { get; set; }

        [StringLength(255)]
        public string Slug { get; set; }

        [DataType(DataType.Html)]
        [Display(Name = "内容")]
        [Required]
        public string Content { get; set; }

        [Display(Name = "作者")]
        public ApplicationUser Author { get; set; }

        [ForeignKey(nameof(Author))]
        public string AuthorID { get; set; }

        [Display(Name = "创建时间")]
        public DateTime CreatedTime { get; set; }

        [Display(Name = "编辑时间")]
        public DateTime EditedTime { get; set; }

        public ICollection<ArticleImage> Images { get; } = new List<ArticleImage>();

        [Display(Name = "分类")]
        public ICollection<ArticleCategory> Categories { get; } = new List<ArticleCategory>();

        public ICollection<Comment> Comments { get; } = new List<Comment>();

        public ArticleStatus Status { get; set; }

        [ForeignKey(nameof(ParentArticle))]
        public int? ParentArticleID { get; set; }

        public Article ParentArticle { get; set; }
        public Article DraftArticle { get; set; }

        [NotMapped]
        public bool CanEdit { get; set; }

        private IDictionary<string, Image> usedImages;//Key is the URL of the image.
        private ICollection<string> imgSrcs;

        public void FinishEdit(IQueryable<Image> imagesInDb, ICollection<int> newCategoryIDs, DateTime currentTime)
        {
            this.EditedTime = currentTime;
            this.Content = this.Content?.Trim();
            updateImageArticleLinks(imagesInDb);
            updateCategoryArticleLinks(newCategoryIDs);
        }

        private void updateImageArticleLinks(IQueryable<Image> imagesInDb)
        {
            imgSrcs = ArticleContentHelper.GetImageSrcs(Content).Where(src => src.StartsWith(ImagePath.UrlPath)).ToList();
            usedImages = imagesInDb.Where(i => imgSrcs.Distinct().Contains(i.Url)).ToDictionary(s => s.Url);
            CollectionUpdateHelper.updateCollection(Images, ai => ai.Image.Url, usedImages, i => new ArticleImage { Image = i });
        }

        private void updateCategoryArticleLinks(ICollection<int> categoryIDs)
        {
            CollectionUpdateHelper.updateCollection(
                Categories,
                ac => ac.CategoryID,
                categoryIDs.ToDictionary(id => id),
                id => new ArticleCategory { CategoryID = id });
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (usedImages != null && imgSrcs != null && usedImages.Count() != imgSrcs.Count)
            {
                foreach (var item in imgSrcs)
                {
                    if (!usedImages.ContainsKey(item))
                    {
                        yield return new ValidationResult($"图片{item}不存在或已被删除", new[] { nameof(Content) });
                    }
                }
            }
        }

        public Article CreateDraft()
        {
            this.DraftArticle = new Article
            {
                ParentArticle = this,
                Status = ArticleStatus.Draft,
                AuthorID = this.AuthorID
            };
            return this.DraftArticle;
        }
    }
}
