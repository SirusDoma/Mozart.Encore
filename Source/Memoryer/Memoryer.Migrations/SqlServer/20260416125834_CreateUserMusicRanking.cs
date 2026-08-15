using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mozart.Migrations.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class CreateUserMusicRanking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "t_o2jam_user_music_ranking",
                columns: table => new
                {
                    USER_INDEX_ID = table.Column<int>(type: "int", nullable: false),
                    MUSIC_INDEX_ID = table.Column<int>(type: "int", nullable: false),
                    DIFFICULTY = table.Column<byte>(type: "tinyint", nullable: false),
                    USER_SCORE = table.Column<long>(type: "bigint", nullable: false),
                    FLAG = table.Column<byte>(type: "tinyint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_o2jam_user_music_ranking", x => new { x.USER_INDEX_ID, x.MUSIC_INDEX_ID, x.DIFFICULTY });
                    table.ForeignKey(
                        name: "FK_t_o2jam_user_music_ranking_t_o2jam_charinfo_USER_INDEX_ID",
                        column: x => x.USER_INDEX_ID,
                        principalTable: "t_o2jam_charinfo",
                        principalColumn: "USER_INDEX_ID",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "t_o2jam_user_music_ranking");
        }
    }
}
