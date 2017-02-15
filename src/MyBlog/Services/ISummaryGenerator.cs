using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyBlog.Services
{
    public interface ISummaryGenerator
    {
        string GenerateSummary(string content, int summaryLength);
    }
}
