using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mozart.Migrations.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class CreateUserPenalty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "t_o2jam_penalty",
                columns: table => new
                {
                    USER_INDEX_ID = table.Column<int>(type: "integer", nullable: false),
                    LEVEL = table.Column<int>(type: "integer", nullable: false),
                    COUNT = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_o2jam_penalty", x => x.USER_INDEX_ID);
                    table.ForeignKey(
                        name: "FK_t_o2jam_penalty_t_o2jam_charinfo_USER_INDEX_ID",
                        column: x => x.USER_INDEX_ID,
                        principalTable: "t_o2jam_charinfo",
                        principalColumn: "USER_INDEX_ID");
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "t_o2jam_penalty");
        }
    }
}
