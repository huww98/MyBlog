using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MyBlog.Migrations
{
    public partial class AddImageUploadedTime : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UploadedTime",
                table: "Images",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<byte[]>(
                name: "SHA1",
                table: "Images",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(byte[]),
                oldMaxLength: 20,
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UploadedTime",
                table: "Images");

            migrationBuilder.AlterColumn<byte[]>(
                name: "SHA1",
                table: "Images",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(byte[]),
                oldMaxLength: 20);
        }
    }
}
