using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mozart.Migrations.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class CreateGiftMusic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "t_o2jam_gift_music",
                columns: table => new
                {
                    Seq = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    User_Index_ID = table.Column<int>(type: "int", nullable: false),
                    Music_ID = table.Column<int>(type: "int", nullable: false),
                    Sender_Index_ID = table.Column<int>(type: "int", nullable: false),
                    SenderNickname = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SendDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_o2jam_gift_music", x => x.Seq);
                    table.ForeignKey(
                        name: "FK_t_o2jam_gift_music_t_o2jam_charinfo_User_Index_ID",
                        column: x => x.User_Index_ID,
                        principalTable: "t_o2jam_charinfo",
                        principalColumn: "USER_INDEX_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_t_o2jam_gift_music_User_Index_ID",
                table: "t_o2jam_gift_music",
                column: "User_Index_ID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "t_o2jam_gift_music");
        }
    }
}
