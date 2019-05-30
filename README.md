# MyBlog

Developed for my personal blog (and as a practice). Deployed at https://www.huww98.cn

Based on ASP.NET Core, MySQL, Bootstrap

## Features

* Markdown based article editing and comments. Frontend render using [remarkable](https://github.com/jonschlinkert/remarkable) at editing time, Backend render using [markdig](https://github.com/lunet-io/markdig) after published
* Use [HtmlSanitizer](https://github.com/mganss/HtmlSanitizer). Cleans HTML to avoid XSS attacks.
* Role based authorization, based on [ASP.NET Core Identity](https://docs.microsoft.com/en-us/aspnet/core/security/authentication/identity)
* Login using QQ and GitHub OAuth.
* Tree based article category
* Code highlight with [highlight.js](https://highlightjs.org/)
* Ability to find uploaded but unused image
* Mobile frendly (responsive)

Currently, it is lack of paging. I may add this once I got more article.
