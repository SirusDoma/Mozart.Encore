using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Mozart.Migrations.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class CreateUserRankingExtended : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "t_o2jam_user_ranking",
                columns: table => new
                {
                    Seq = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    User_Index_ID = table.Column<int>(type: "integer", nullable: false),
                    User_ID = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    User_NickName = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: false),
                    Sex = table.Column<bool>(type: "boolean", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Battle = table.Column<int>(type: "integer", nullable: false),
                    Win = table.Column<int>(type: "integer", nullable: false),
                    Draw = table.Column<int>(type: "integer", nullable: false),
                    Lose = table.Column<int>(type: "integer", nullable: false),
                    Experience = table.Column<int>(type: "integer", nullable: false),
                    WriteDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Ranking = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    ChangeType = table.Column<int>(type: "integer", nullable: false),
                    ChangeRanking = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_o2jam_user_ranking", x => x.Seq);
                    table.ForeignKey(
                        name: "FK_t_o2jam_user_ranking_t_o2jam_charinfo_User_Index_ID",
                        column: x => x.User_Index_ID,
                        principalTable: "t_o2jam_charinfo",
                        principalColumn: "USER_INDEX_ID");
                });

            migrationBuilder.CreateIndex(
                name: "IX_t_o2jam_user_ranking_User_Index_ID",
                table: "t_o2jam_user_ranking",
                column: "User_Index_ID",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "t_o2jam_user_ranking");
        }
    }
}
