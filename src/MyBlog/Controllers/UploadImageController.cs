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
        private readonly IImageUploader _uploader;

        public UploadImageController(IImageUploader uploader)
        {
            _uploader = uploader;
        }

        public async Task<IActionResult> Index(IFormFile imageFile, int file_id)
        {
            Image img = new Image();
            await _uploader.UploadImageAsync(imageFile, img);
            return Json(new { src = img.Url });
        }
    }
}
