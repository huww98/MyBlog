﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyBlog.Services
{
    public interface IMarkdownRenderer
    {
        string RenderHtml(string markdown);
    }
}