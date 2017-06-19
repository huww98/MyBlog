using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MyBlog.Migrations
{
    public partial class AddNickNameAndAvatar : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarUrl",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NickName",
                table: "AspNetUsers",
                nullable: true);

            migrationBuilder.Sql(
@"UPDATE AspNetUsers 
SET NickName=UserName;

UPDATE AspNetUsers 
SET UserName=Email,
    NormalizedUserName=NormalizedEmail
WHERE UserName!='Administrator';
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
@"UPDATE AspNetUsers 
SET UserName=NickName,
    NormalizedUserName=upper(NickName)
WHERE UserName!='Administrator';
");

            migrationBuilder.DropColumn(
                name: "AvatarUrl",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "NickName",
                table: "AspNetUsers");
        }
    }
}
