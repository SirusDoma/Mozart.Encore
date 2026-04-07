using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Mozart.Migrations.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class CreateUserRanking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "t_o2jam_dumpranking",
                columns: table => new
                {
                    Ranking = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    User_Index_ID = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_o2jam_dumpranking", x => x.Ranking);
                    table.ForeignKey(
                        name: "FK_t_o2jam_dumpranking_t_o2jam_charinfo_User_Index_ID",
                        column: x => x.User_Index_ID,
                        principalTable: "t_o2jam_charinfo",
                        principalColumn: "USER_INDEX_ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_t_o2jam_dumpranking_User_Index_ID",
                table: "t_o2jam_dumpranking",
                column: "User_Index_ID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "t_o2jam_dumpranking");
        }
    }
}
