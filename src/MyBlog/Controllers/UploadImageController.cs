using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using MyBlog.Models;
using MyBlog.Services;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace MyBlog.Controllers
{
    public class UploadImageController : Controller
    {
        private readonly IImageProcessor _processor;

        public UploadImageController(IImageProcessor processor)
        {
            _processor = processor;
        }

        public async Task<IActionResult> Index(IFormFile imageFile, int file_id)
        {
            Image img = new Image();
            using (var stream = imageFile.OpenReadStream())
            {
                await _processor.SaveImageAsync(imageFile.FileName, stream, img);
            }
            return Json(new { src = img.Url });
        }
    }
}