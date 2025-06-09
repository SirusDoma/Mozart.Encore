using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mozart.Persistence.Migrations.Sqlite
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "t_o2jam_charinfo",
                columns: table => new
                {
                    USER_INDEX_ID = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    USER_ID = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    USER_NICKNAME = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Sex = table.Column<bool>(type: "INTEGER", nullable: false),
                    Level = table.Column<int>(type: "INTEGER", nullable: false),
                    Battle = table.Column<int>(type: "INTEGER", nullable: false),
                    Win = table.Column<int>(type: "INTEGER", nullable: false),
                    Lose = table.Column<int>(type: "INTEGER", nullable: false),
                    Draw = table.Column<int>(type: "INTEGER", nullable: false),
                    Experience = table.Column<int>(type: "INTEGER", nullable: false),
                    AdminLevel = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_o2jam_charinfo", x => x.USER_INDEX_ID);
                });

            migrationBuilder.CreateTable(
                name: "t_o2jam_credentials",
                columns: table => new
                {
                    Id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<int>(type: "INTEGER", nullable: false),
                    Username = table.Column<string>(type: "TEXT", maxLength: 12, nullable: false),
                    Password = table.Column<byte[]>(type: "BLOB", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_o2jam_credentials", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "t_o2jam_login",
                columns: table => new
                {
                    USER_INDEX_ID = table.Column<int>(type: "INTEGER", nullable: false),
                    GATEWAY_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    MAIN_CH = table.Column<int>(type: "INTEGER", nullable: false),
                    SUB_CH = table.Column<int>(type: "INTEGER", nullable: false),
                    USER_ID = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    TUSER_ID = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    ADDR_IP = table.Column<string>(type: "TEXT", nullable: false),
                    LOGIN_TIME = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_o2jam_login", x => x.USER_INDEX_ID);
                });

            migrationBuilder.CreateTable(
                name: "t_o2jam_charcash",
                columns: table => new
                {
                    USER_INDEX_ID = table.Column<int>(type: "INTEGER", nullable: false),
                    Gem = table.Column<int>(type: "INTEGER", nullable: false),
                    O2Cash = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_o2jam_charcash", x => x.USER_INDEX_ID);
                    table.ForeignKey(
                        name: "FK_t_o2jam_charcash_t_o2jam_charinfo_USER_INDEX_ID",
                        column: x => x.USER_INDEX_ID,
                        principalTable: "t_o2jam_charinfo",
                        principalColumn: "USER_INDEX_ID");
                });

            migrationBuilder.CreateTable(
                name: "t_o2jam_item",
                columns: table => new
                {
                    USER_INDEX_ID = table.Column<int>(type: "INTEGER", nullable: false),
                    Equip1 = table.Column<short>(type: "INTEGER", nullable: false),
                    Equip2 = table.Column<short>(type: "INTEGER", nullable: false),
                    Equip3 = table.Column<short>(type: "INTEGER", nullable: false),
                    Equip4 = table.Column<short>(type: "INTEGER", nullable: false),
                    Equip5 = table.Column<short>(type: "INTEGER", nullable: false),
                    Equip6 = table.Column<short>(type: "INTEGER", nullable: false),
                    Equip7 = table.Column<short>(type: "INTEGER", nullable: false),
                    Equip8 = table.Column<short>(type: "INTEGER", nullable: false),
                    Equip9 = table.Column<short>(type: "INTEGER", nullable: false),
                    Equip10 = table.Column<short>(type: "INTEGER", nullable: false),
                    Equip11 = table.Column<short>(type: "INTEGER", nullable: false),
                    Equip12 = table.Column<short>(type: "INTEGER", nullable: false),
                    Equip13 = table.Column<short>(type: "INTEGER", nullable: false),
                    Equip14 = table.Column<short>(type: "INTEGER", nullable: false),
                    Equip15 = table.Column<short>(type: "INTEGER", nullable: false),
                    Equip16 = table.Column<short>(type: "INTEGER", nullable: false),
                    Bag1 = table.Column<short>(type: "INTEGER", nullable: false),
                    Bag2 = table.Column<short>(type: "INTEGER", nullable: false),
                    Bag3 = table.Column<short>(type: "INTEGER", nullable: false),
                    Bag4 = table.Column<short>(type: "INTEGER", nullable: false),
                    Bag5 = table.Column<short>(type: "INTEGER", nullable: false),
                    Bag6 = table.Column<short>(type: "INTEGER", nullable: false),
                    Bag7 = table.Column<short>(type: "INTEGER", nullable: false),
                    Bag8 = table.Column<short>(type: "INTEGER", nullable: false),
                    Bag9 = table.Column<short>(type: "INTEGER", nullable: false),
                    Bag10 = table.Column<short>(type: "INTEGER", nullable: false),
                    Bag11 = table.Column<short>(type: "INTEGER", nullable: false),
                    Bag12 = table.Column<short>(type: "INTEGER", nullable: false),
                    Bag13 = table.Column<short>(type: "INTEGER", nullable: false),
                    Bag14 = table.Column<short>(type: "INTEGER", nullable: false),
                    Bag15 = table.Column<short>(type: "INTEGER", nullable: false),
                    Bag16 = table.Column<short>(type: "INTEGER", nullable: false),
                    Bag17 = table.Column<short>(type: "INTEGER", nullable: false),
                    Bag18 = table.Column<short>(type: "INTEGER", nullable: false),
                    Bag19 = table.Column<short>(type: "INTEGER", nullable: false),
                    Bag20 = table.Column<short>(type: "INTEGER", nullable: false),
                    Bag21 = table.Column<short>(type: "INTEGER", nullable: false),
                    Bag22 = table.Column<short>(type: "INTEGER", nullable: false),
                    Bag23 = table.Column<short>(type: "INTEGER", nullable: false),
                    Bag24 = table.Column<short>(type: "INTEGER", nullable: false),
                    Bag25 = table.Column<short>(type: "INTEGER", nullable: false),
                    Bag26 = table.Column<short>(type: "INTEGER", nullable: false),
                    Bag27 = table.Column<short>(type: "INTEGER", nullable: false),
                    Bag28 = table.Column<short>(type: "INTEGER", nullable: false),
                    Bag29 = table.Column<short>(type: "INTEGER", nullable: false),
                    Bag30 = table.Column<short>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_o2jam_item", x => x.USER_INDEX_ID);
                    table.ForeignKey(
                        name: "FK_t_o2jam_item_t_o2jam_charinfo_USER_INDEX_ID",
                        column: x => x.USER_INDEX_ID,
                        principalTable: "t_o2jam_charinfo",
                        principalColumn: "USER_INDEX_ID");
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "t_o2jam_charcash");

            migrationBuilder.DropTable(
                name: "t_o2jam_credentials");

            migrationBuilder.DropTable(
                name: "t_o2jam_item");

            migrationBuilder.DropTable(
                name: "t_o2jam_login");

            migrationBuilder.DropTable(
                name: "t_o2jam_charinfo");
        }
    }
}
