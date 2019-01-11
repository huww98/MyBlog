using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace MyBlog.Models.ArticleViewModels
{
    public class ArticleFilterViewModel:IValidatableObject
    {
        public int? CategoryID { get; set; }

        private DateTime? _fromDate;
        [DataType(DataType.Date)]
        public DateTime? FromDate
        {
            get { return _fromDate; }
            set
            {
                _fromDate = value?.Date;
            }
        }

        private DateTime? _toDate;
        [DataType(DataType.Date)]
        public DateTime? ToDate
        {
            get { return _toDate; }
            set
            {
                _toDate = value?.Date;
            }
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (ToDate != null && FromDate != null && ToDate < FromDate)
            {
                yield return new System.ComponentModel.DataAnnotations.ValidationResult("筛选结束时间必须大于开始时间",
                    new[] { nameof(FromDate), nameof(ToDate) });
            }
        }
    }

    public static class ArticleFilterExtensions
    {
        public static IQueryable<Article> ApplyArticleFilter(this IQueryable<Article> query, ArticleFilterViewModel filter)
        {
            if (filter.CategoryID != null)
            {
                query = query.Where(a => a.Categories.Any(ac => ac.CategoryID == filter.CategoryID));
            }
            if (filter.FromDate!=null)
            {
                query = query.Where(a => a.CreatedTime >= filter.FromDate);
            }
            if (filter.ToDate!=null)
            {
                query = query.Where(a => a.CreatedTime < filter.ToDate.Value.AddDays(1));
            }

            return query;
        }
    }
}
