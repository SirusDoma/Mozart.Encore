using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mozart.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class AddMemberVip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "vip",
                table: "member",
                type: "INTEGER",
                nullable: false,
                defaultValue: (short)0);

            migrationBuilder.AddColumn<DateTime>(
                name: "vipdate",
                table: "member",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "vip",
                table: "member");

            migrationBuilder.DropColumn(
                name: "vipdate",
                table: "member");
        }
    }
}
