using MyBlog.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MyBlog.Services
{
    public interface IImageProcessor
    {
        Task SaveImageAsync(string fileName, Stream imageStream, Image image);

        Task<ImageDeleteResult> DeleteImageAsync(int id);
    }

    public enum ImageDeleteResult { Successed, NotFount, InUse }
}