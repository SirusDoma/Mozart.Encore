using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mozart.Migrations.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class AddWalletCurrencies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CashPoint",
                table: "t_o2jam_charcash",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MusicCash",
                table: "t_o2jam_charcash",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ItemCash",
                table: "t_o2jam_charcash",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Point",
                table: "t_o2jam_charcash",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CashPoint",
                table: "t_o2jam_charcash");

            migrationBuilder.DropColumn(
                name: "MusicCash",
                table: "t_o2jam_charcash");

            migrationBuilder.DropColumn(
                name: "ItemCash",
                table: "t_o2jam_charcash");

            migrationBuilder.DropColumn(
                name: "Point",
                table: "t_o2jam_charcash");
        }
    }
}
