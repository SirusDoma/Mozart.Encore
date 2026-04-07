using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Mozart.Migrations.MySql.Migrations
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
                    INDEX_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    USER_INDEX_ID = table.Column<int>(type: "int", nullable: false),
                    ITEM_INDEX_ID = table.Column<int>(type: "int", nullable: false),
                    USED_COUNT = table.Column<int>(type: "int", nullable: false),
                    OLD_USED_COUNT = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    REG_DATE = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
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
                })
                .Annotation("MySQL:Charset", "utf8mb4");

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
