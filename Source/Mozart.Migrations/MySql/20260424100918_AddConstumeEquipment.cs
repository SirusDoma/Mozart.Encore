using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mozart.Migrations.MySql.Migrations
{
    /// <inheritdoc />
    public partial class AddConstumeEquipment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<short>(
                name: "Equip17",
                table: "t_o2jam_item",
                type: "smallint",
                nullable: false,
                defaultValue: (short)0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Equip17",
                table: "t_o2jam_item");
        }
    }
}
