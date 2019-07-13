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

## Development

* Install MySQL
* Open solution with Visual Studio
* Configure OAuth：
  * right click MyBlog project，click Manage User secrets，输入相关信息，如：
    ```json
    {
      "Authentication": {
        "QQ": {
          "AppId": "101xxxx9",
          "AppKey": "xxxxxxxxxxxxx"
        },
        "GitHub": {
          "ClientID": "3b0xxxxxxx66",
          "ClientSecret": "xxxxxxxxxxxxxxxxxxx"
        }
      }
    }
    ```
  * Or，delete `AddQQ`/`AddGitHub` in `Startup.cs` file.
* Config Database connection: edit `appsettings.json`，Update ConnectionStrings.DefaultConnection with your MySQL connection string.

## Deployment

OAuth configure and database connection string should be write to `appsettings.Production.json` file on server.

Build process I'm using:
```shell
dotnet publish --configuration Release --output ...
```

Run with:
```shell
dotnet MyBlog.dll
```

For more information, refer to
* Official documentation: https://docs.microsoft.com/zh-cn/aspnet/core/host-and-deploy/linux-nginx
* My blog article: https://www.huww98.cn/blog-construction-road-deployment
