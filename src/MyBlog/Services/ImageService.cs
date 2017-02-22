using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using MyBlog.Models;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;

namespace MyBlog.Services
{
    public class ImageService : IImageProcessor
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentTime _time;

        public ImageService(ApplicationDbContext context, ICurrentTime time)
        {
            _context = context;
            _time = time;
        }

        private static void generatePath(string fileName, out string pathToSave, out string url)
        {
            Directory.CreateDirectory(ImagePath.StoragePath);
            long ticks = DateTime.Now.ToUniversalTime().Ticks;
            while (true)
            {
                string serverfileName = ticks.ToString() + Path.GetExtension(fileName).ToLower();
                pathToSave = Path.Combine(ImagePath.StoragePath, serverfileName);
                if (!System.IO.File.Exists(pathToSave))
                {
                    url = ImagePath.UrlPath + "/" + serverfileName;
                    break;
                }
                ticks++;
            }
        }

        public async Task<ImageDeleteResult> DeleteImageAsync(int id)
        {
            var image = await _context.Images.Include(i => i.Articles).SingleOrDefaultAsync(m => m.ID == id);
            if (image == null)
            {
                return ImageDeleteResult.NotFount;
            }
            if (image.Articles.Count > 0)
            {
                return ImageDeleteResult.InUse;
            }
            _context.Images.Remove(image);
            await _context.SaveChangesAsync();
            if (File.Exists(image.Path))
            {
                File.Delete(image.Path);
            }
            return ImageDeleteResult.Successed;
        }

        public async Task SaveImageAsync(string fileName, Stream imageStream, Image image)
        {
            string pathToSave, url;
            generatePath(fileName, out pathToSave, out url);
            image.Path = pathToSave;
            image.Url = url;
            image.UploadedTime = _time.Time;

            byte[] data = new byte[imageStream.Length];
            await imageStream.ReadAsync(data, 0, (int)imageStream.Length);

            SHA1 sha1 = SHA1.Create();
            image.SHA1 = sha1.ComputeHash(data);
            using (FileStream fileStream = new FileStream(pathToSave, FileMode.CreateNew))
            {
                await fileStream.WriteAsync(data, 0, data.Length);
            }

            _context.Add(image);
            await _context.SaveChangesAsync();
        }
    }
}