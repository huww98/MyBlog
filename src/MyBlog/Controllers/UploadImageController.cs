using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.IO;
using System.Security.Cryptography;
using MyBlog.Models;
using MyBlog.Data;

namespace MyBlog.Controllers
{
    public class UploadImageController : Controller
    {
        public const string ImagePath = "wwwroot\\UploadedImages";
        public const string UrlPath = "/UploadedImages";
        private readonly ApplicationDbContext _context;
        public UploadImageController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<object> Index(IFormFile imageFile, int file_id)
        {
            string pathToSave, url;
            generatePath(imageFile, out pathToSave, out url);
            Image image = new Image { Path = pathToSave, Url = url };
            byte[] data;
            using (Stream stream = imageFile.OpenReadStream())
            {
                data = new byte[stream.Length];
                await stream.ReadAsync(data, 0, (int)stream.Length);
            }

            SHA1 sha1 = SHA1.Create();
            image.SHA1 = sha1.ComputeHash(data);
            using (FileStream fileStream = new FileStream(pathToSave, FileMode.CreateNew))
            {
                await fileStream.WriteAsync(data, 0, data.Length);
            }

            _context.Add(image);
            await _context.SaveChangesAsync();
            return new { src = url };
        }

        private static void generatePath(IFormFile picture, out string pathToSave, out string url)
        {
            Directory.CreateDirectory(ImagePath);
            long ticks = DateTime.Now.ToUniversalTime().Ticks;
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
        }
    }
}
