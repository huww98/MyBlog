using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyBlog.Models;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace MyBlog.Controllers
{
    public class UploadImageController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UploadImageController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(IFormFile imageFile, int file_id)
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
            return Json(new { src = url });
        }

        private static void generatePath(IFormFile picture, out string pathToSave, out string url)
        {
            Directory.CreateDirectory(ImagePath.StoragePath);
            long ticks = DateTime.Now.ToUniversalTime().Ticks;
            while (true)
            {
                string fileName = ticks.ToString() + Path.GetExtension(picture.FileName);
                pathToSave = Path.Combine(ImagePath.StoragePath, fileName);
                if (!System.IO.File.Exists(pathToSave))
                {
                    url = ImagePath.UrlPath + "/" + fileName;
                    break;
                }
                ticks++;
            }
        }
    }
}