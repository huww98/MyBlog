using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace MyBlog.Migrations
{
    public partial class CascadeDeleteChildrenCategory : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Category_Category_ParentCategoryID",
                table: "Category");

            migrationBuilder.AddForeignKey(
                name: "FK_Category_Category_ParentCategoryID",
                table: "Category",
                column: "ParentCategoryID",
                principalTable: "Category",
                principalColumn: "ID",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Category_Category_ParentCategoryID",
                table: "Category");

            migrationBuilder.AddForeignKey(
                name: "FK_Category_Category_ParentCategoryID",
                table: "Category",
                column: "ParentCategoryID",
                principalTable: "Category",
                principalColumn: "ID",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
