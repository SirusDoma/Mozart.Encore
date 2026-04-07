using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mozart.Migrations.MySql.Migrations
{
    /// <inheritdoc />
    public partial class AddMembership : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "vip",
                table: "member",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "vipdate",
                table: "member",
                type: "datetime(6)",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_t_o2jam_charinfo_USER_ID",
                table: "t_o2jam_charinfo",
                column: "USER_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_member_t_o2jam_charinfo_userid",
                table: "member",
                column: "userid",
                principalTable: "t_o2jam_charinfo",
                principalColumn: "USER_ID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_member_t_o2jam_charinfo_userid",
                table: "member");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_t_o2jam_charinfo_USER_ID",
                table: "t_o2jam_charinfo");

            migrationBuilder.DropColumn(
                name: "vip",
                table: "member");

            migrationBuilder.DropColumn(
                name: "vipdate",
                table: "member");
        }
    }
}
