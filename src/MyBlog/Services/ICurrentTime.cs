using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyBlog.Services
{
    ///<remarks>Defined for unit test purpose.</remarks>
    public interface ICurrentTime
    {
        DateTime CurrentTime { get; }
    }
}
