using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MyBlog.Authorization;
using MyBlog.Models;
using MyBlog.Services;
using System;
using System.Text.Encodings.Web;
using System.Text.Unicode;
using Microsoft.AspNetCore.Mvc;
using Pomelo.EntityFrameworkCore.MySql.Storage;
using Pomelo.EntityFrameworkCore.MySql.Infrastructure;
using Microsoft.Extensions.Hosting;

namespace MyBlog
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseMySql(Configuration.GetConnectionString("DefaultConnection"), mySqlOptions => mySqlOptions
                    .ServerVersion(new ServerVersion(new Version(5, 7, 28), ServerType.MySql)))
            );

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddControllersWithViews().SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            services.AddAuthorization(options =>
            {
                options.AddPolicy("CanCreateArticle", builder => builder.RequireRole(RoleInfo.EditorRoleName));
            });

            // Add application services.
            services.AddTransient<IEmailSender, AuthMessageSender>();
            services.AddTransient<ISmsSender, AuthMessageSender>();
            services.AddSingleton<ICurrentTime, CurrentTimeService>();
            services.AddTransient<ISummaryGenerator, SanitizeSummaryGenerator>();
            services.AddTransient<IMarkdownRenderer, MarkdigSanitizedService>();
            services.AddScoped<IImageUploader, ImageUploader>();
            services.AddSingleton(HtmlEncoder.Create(allowedRanges: new[] { UnicodeRanges.BasicLatin, UnicodeRanges.CjkUnifiedIdeographs }));

            services.Configure<IdentityOptions>(o =>
            {
                o.Password.RequireDigit = false;
                o.Password.RequireLowercase = false;
                o.Password.RequireUppercase = false;
                o.Password.RequireNonAlphanumeric = false;

                //o.User.RequireUniqueEmail = true;
            });
            services.AddAuthentication()
                .AddQQ(options =>
                {
                    options.ClientId = Configuration["Authentication:QQ:AppId"];
                    options.ClientSecret = Configuration["Authentication:QQ:AppKey"];
                })
                .AddGitHub(options =>
                {
                    options.ClientId = Configuration["Authentication:GitHub:ClientID"];
                    options.ClientSecret = Configuration["Authentication:GitHub:ClientSecret"];
                    options.Scope.Add("user:email");
                });

            services.AddSingleton<IAuthorizationHandler, IsArticleAuthorAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, IsCommentAuthorAuthorizationHandler>();
            services.AddSingleton<IAuthorizationHandler, IsEditorAuthorizationHandler>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.All
            });
            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllerRoute(
                    name: "article",
                    pattern: "{*slug:required}",
                    defaults: new { controller = "Articles", action = "Slug" });
            });
        }
    }
}
