using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyBlog.Services
{
    public class CurrentTimeService : ICurrentTime
    {
        public DateTime CurrentTime => DateTime.Now;
    }
}
