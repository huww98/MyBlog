using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;

namespace MyBlog.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
            builder.Entity<Article>()
                .HasOne(a => a.ParentArticle)
                .WithOne(a => a.DraftArticle)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<ArticleImage>()
                .HasKey(t => new { t.ArticleID, t.ImageID });
            builder.Entity<ArticleCategory>()
                .HasKey(t => new { t.ArticleID, t.CategoryID });
            builder.Entity<Category>()
                .HasOne(c => c.ParentCategory)
                .WithMany(c => c.ChildCategories)
                .HasForeignKey(c => c.ParentCategoryID)
                .OnDelete(DeleteBehavior.Cascade);
            builder.Entity<Comment>()
                .HasOne(c => c.ParentComment)
                .WithMany(c => c.ChildrenComments)
                .HasForeignKey(c => c.ParentCommentID)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Image>()
                .HasIndex(i => i.Url)
                .IsUnique();
            builder.Entity<Article>()
                .HasIndex(a => a.Slug)
                .IsUnique();
        }

        public DbSet<Article> Articles { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<ArticleImage> ArticleImages { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Comment> Comments { get; set; }
    }
}