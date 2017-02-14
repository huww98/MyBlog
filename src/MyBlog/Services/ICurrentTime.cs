using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyBlog.Services
{
    public interface ICurrentTime
    {
        DateTime Time { get; }
    }
}
