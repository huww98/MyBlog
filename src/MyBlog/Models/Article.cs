using AngleSharp.Dom.Html;
using Ganss.XSS;
using MyBlog.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace MyBlog.Models
{
    public class Article
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

        [Display(Name = "创建日期")]
        public DateTime CreatedTime { get; set; }

        [Display(Name = "编辑日期")]
        public DateTime EditedTime { get; set; }

        public ICollection<ArticleImage> Images { get; set; }

        [Display(Name = "分类")]
        public ICollection<ArticleCategory> Categories { get; set; }

        [NotMapped]
        public bool CanEdit { get; set; }

        public ValidationResult FinishCreate(ApplicationDbContext context, ICollection<int> newCategoryIDs, DateTime currentTime)
        {
            this.CreatedTime = currentTime;
            return FinishEdit(context, newCategoryIDs, currentTime);
        }
        public ValidationResult FinishEdit(ApplicationDbContext context, ICollection<int> newCategoryIDs, DateTime currentTime)
        {
            this.EditedTime = currentTime;
            IDictionary<string, Image> imageUsed;  //Key is the URL of the image.
            var result = preprocessContent(context, out imageUsed);
            updateImageArticleLinks(imageUsed);
            updateCategoryArticleLinks(newCategoryIDs);

            return result;
        }

        private ValidationResult preprocessContent(ApplicationDbContext context, out IDictionary<string, Image> usedImages)
        {
            this.Content = this.Content.Trim();
            List<string> imgSrcs = new List<string>();
            var sanitizer = new HtmlSanitizer();
            sanitizer.PostProcessNode += (s, e) =>
            {
                IHtmlImageElement ele = e.Node as IHtmlImageElement;
                if (ele != null)
                {
                    Uri uri = new Uri(ele.GetAttribute("src"), UriKind.RelativeOrAbsolute);
                    if (!uri.IsAbsoluteUri)
                    {
                        imgSrcs.Add(uri.OriginalString);
                    }
                }
            };
            this.Content = sanitizer.Sanitize(this.Content);
            usedImages = context.Images.Where(i => imgSrcs.Distinct().Contains(i.Url)).ToDictionary(s => s.Url);

            ValidationResult result = new ValidationResult();
            result.IsSucceeded = true;
            if (usedImages.Count() != imgSrcs.Count)
            {
                foreach (var item in imgSrcs)
                {
                    if (!usedImages.ContainsKey(item))
                    {
                        result.IsSucceeded = false;
                        result.Errors.Add(nameof(this.Content), $"图片{item}不存在或已被删除");
                    }
                }
            }
            return result;
        }

        private void updateImageArticleLinks(IDictionary<string, Image> imageUsed)
        {
            if (Images == null)
            {
                Images = new List<ArticleImage>();
            }
            CollectionUpdateHelper.updateCollection(Images, ai => ai.Image.Url, imageUsed, i => new ArticleImage { Image = i });
        }

        private void updateCategoryArticleLinks(ICollection<int> categoryIDs)
        {
            if (Categories == null)
            {
                Categories = new List<ArticleCategory>();
            }
            CollectionUpdateHelper.updateCollection(
                Categories,
                ac => ac.CategoryID,
                categoryIDs.ToDictionary(id => id),
                id => new ArticleCategory { CategoryID = id });
        }
    }

    public class ValidationResult
    {
        public bool IsSucceeded { get; set; }

        public Dictionary<string, string> Errors { get; set; } = new Dictionary<string, string>();
    }
}