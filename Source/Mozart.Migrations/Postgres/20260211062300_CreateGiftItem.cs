using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Mozart.Migrations.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class CreateGiftItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "t_o2jam_gift_item",
                columns: table => new
                {
                    Seq = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    User_Index_ID = table.Column<int>(type: "integer", nullable: false),
                    ItemID = table.Column<int>(type: "integer", nullable: false),
                    Sender_Index_ID = table.Column<int>(type: "integer", nullable: false),
                    SenderNickname = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    SendDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_o2jam_gift_item", x => x.Seq);
                    table.ForeignKey(
                        name: "FK_t_o2jam_gift_item_t_o2jam_charinfo_User_Index_ID",
                        column: x => x.User_Index_ID,
                        principalTable: "t_o2jam_charinfo",
                        principalColumn: "USER_INDEX_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_t_o2jam_gift_item_User_Index_ID",
                table: "t_o2jam_gift_item",
                column: "User_Index_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "t_o2jam_gift_item");
        }
    }
}
