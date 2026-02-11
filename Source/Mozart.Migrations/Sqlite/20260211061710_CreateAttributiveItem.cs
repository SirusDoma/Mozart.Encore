using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mozart.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class CreateAttributiveItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "t_o2jam_char_attr_item",
                columns: table => new
                {
                    INDEX_ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    USER_INDEX_ID = table.Column<int>(type: "INTEGER", nullable: false),
                    ITEM_INDEX_ID = table.Column<int>(type: "INTEGER", nullable: false),
                    USED_COUNT = table.Column<int>(type: "INTEGER", nullable: false),
                    OLD_USED_COUNT = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    REG_DATE = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_o2jam_char_attr_item", x => x.INDEX_ID);
                    table.ForeignKey(
                        name: "FK_t_o2jam_char_attr_item_t_o2jam_charinfo_USER_INDEX_ID",
                        column: x => x.USER_INDEX_ID,
                        principalTable: "t_o2jam_charinfo",
                        principalColumn: "USER_INDEX_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_t_o2jam_char_attr_item_USER_INDEX_ID",
                table: "t_o2jam_char_attr_item",
                column: "USER_INDEX_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "t_o2jam_char_attr_item");
        }
    }
}
