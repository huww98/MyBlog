using Microsoft.AspNetCore.Http;
using MyBlog.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace MyBlog.Services
{
    /// <remarks>This is defined as a separate interface because it is used by multiple controllers</remarks>
    public interface IImageUploader
    {
        /// <summary>
        /// Save the uploaded file to disk
        /// </summary>
        /// <param name="file">The file uploaded</param>
        /// <param name="newImage">The related property of this object will be populated.</param>
        /// <returns></returns>
        Task UploadImageAsync(IFormFile file, Image newImage);
    }
}
