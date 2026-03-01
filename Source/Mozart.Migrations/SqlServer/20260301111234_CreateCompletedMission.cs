using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mozart.Migrations.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class CreateCompletedMission : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "t_o2jam_user_mission",
                columns: table => new
                {
                    USER_INDEX_ID = table.Column<int>(type: "int", nullable: false),
                    GatewayID = table.Column<int>(type: "int", nullable: false),
                    SetID = table.Column<int>(type: "int", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Rank = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_o2jam_user_mission", x => new { x.USER_INDEX_ID, x.GatewayID, x.SetID, x.Level });
                    table.ForeignKey(
                        name: "FK_t_o2jam_user_mission_t_o2jam_charinfo_USER_INDEX_ID",
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
                name: "t_o2jam_user_mission");
        }
    }
}
