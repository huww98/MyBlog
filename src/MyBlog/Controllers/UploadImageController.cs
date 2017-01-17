using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace MyBlog.Controllers
{
    public class UploadImageController : Controller
    {
        public const string ImagePath = "wwwroot\\UploadedImages";
        public const string UrlPath = "/UploadedImages";
        public async Task<object> Index(IFormFile picture, int file_id)
        {
            Directory.CreateDirectory(ImagePath);
            long ticks = DateTime.Now.ToUniversalTime().Ticks;
            string pathToSave, url;
            while (true)
            {
                string fileName = ticks.ToString() + Path.GetExtension(picture.FileName);
                pathToSave = Path.Combine(ImagePath, fileName);
                if (!System.IO.File.Exists(pathToSave))
                {
                    url = Path.Combine(UrlPath, fileName);
                    url = url.Replace('\\', '/');
                    break;
                }
                ticks++;
            }

            using (FileStream stream = new FileStream(pathToSave, FileMode.CreateNew))
            {
                await picture.CopyToAsync(stream);
            }
            return new { src = url };
        }
    }
}
