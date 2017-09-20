using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MyBlog.Models;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

namespace MyBlog.Services
{
    public class ImageUploader : IImageUploader
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentTime _time;

        public ImageUploader(ApplicationDbContext context, ICurrentTime time)
        {
            _context = context;
            _time = time;
        }

        private void GeneratePath(string fileName, Image newImage)
        {
            Directory.CreateDirectory(ImagePath.StoragePath);
            long ticks = _time.CurrentTime.ToUniversalTime().Ticks;
            while (true)
            {
                string serverfileName = ticks.ToString() + Path.GetExtension(fileName).ToLower();
                newImage.Path = Path.Combine(ImagePath.StoragePath, serverfileName);
                if (!File.Exists(newImage.Path))
                {
                    newImage.Url = ImagePath.UrlPath + "/" + serverfileName;
                    break;
                }
                ticks++;
            }
        }

        public async Task UploadImageAsync(IFormFile file, Image newImage)
        {
            GeneratePath(file.FileName, newImage);
            newImage.UploadedTime = _time.CurrentTime;

            using (var imageStream = file.OpenReadStream())
            {
                byte[] data = new byte[imageStream.Length];
                await imageStream.ReadAsync(data, 0, (int)imageStream.Length);

                SHA1 sha1 = SHA1.Create();
                newImage.SHA1 = sha1.ComputeHash(data);
                using (FileStream fileStream = new FileStream(newImage.Path, FileMode.CreateNew))
                {
                    await fileStream.WriteAsync(data, 0, data.Length);
                }
            }
            _context.Add(newImage);
            await _context.SaveChangesAsync();
        }
    }
}