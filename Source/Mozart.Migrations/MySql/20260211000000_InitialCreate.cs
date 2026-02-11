using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace Mozart.Migrations.MySql.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "member",
                columns: table => new
                {
                    id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    userid = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    passwd = table.Column<byte[]>(type: "longblob", nullable: false),
                    registdate = table.Column<DateTime>(type: "datetime(6)", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_member", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "t_o2jam_charinfo",
                columns: table => new
                {
                    USER_INDEX_ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    USER_ID = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    USER_NICKNAME = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    Sex = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Level = table.Column<int>(type: "int", nullable: false),
                    Battle = table.Column<int>(type: "int", nullable: false),
                    Win = table.Column<int>(type: "int", nullable: false),
                    Lose = table.Column<int>(type: "int", nullable: false),
                    Draw = table.Column<int>(type: "int", nullable: false),
                    Experience = table.Column<int>(type: "int", nullable: false),
                    AdminLevel = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_o2jam_charinfo", x => x.USER_INDEX_ID);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "t_o2jam_login",
                columns: table => new
                {
                    USER_INDEX_ID = table.Column<int>(type: "int", nullable: false),
                    GATEWAY_ID = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    MAIN_CH = table.Column<int>(type: "int", nullable: false),
                    SUB_CH = table.Column<int>(type: "int", nullable: false),
                    USER_ID = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    TUSER_ID = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false),
                    ADDR_IP = table.Column<string>(type: "longtext", nullable: false),
                    LOGIN_TIME = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_o2jam_login", x => x.USER_INDEX_ID);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "t_o2jam_charcash",
                columns: table => new
                {
                    USER_INDEX_ID = table.Column<int>(type: "int", nullable: false),
                    Gem = table.Column<int>(type: "int", nullable: false),
                    O2Cash = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_o2jam_charcash", x => x.USER_INDEX_ID);
                    table.ForeignKey(
                        name: "FK_t_o2jam_charcash_t_o2jam_charinfo_USER_INDEX_ID",
                        column: x => x.USER_INDEX_ID,
                        principalTable: "t_o2jam_charinfo",
                        principalColumn: "USER_INDEX_ID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "t_o2jam_item",
                columns: table => new
                {
                    USER_INDEX_ID = table.Column<int>(type: "int", nullable: false),
                    Equip1 = table.Column<short>(type: "smallint", nullable: false),
                    Equip2 = table.Column<short>(type: "smallint", nullable: false),
                    Equip3 = table.Column<short>(type: "smallint", nullable: false),
                    Equip4 = table.Column<short>(type: "smallint", nullable: false),
                    Equip5 = table.Column<short>(type: "smallint", nullable: false),
                    Equip6 = table.Column<short>(type: "smallint", nullable: false),
                    Equip7 = table.Column<short>(type: "smallint", nullable: false),
                    Equip8 = table.Column<short>(type: "smallint", nullable: false),
                    Equip9 = table.Column<short>(type: "smallint", nullable: false),
                    Equip10 = table.Column<short>(type: "smallint", nullable: false),
                    Equip11 = table.Column<short>(type: "smallint", nullable: false),
                    Equip12 = table.Column<short>(type: "smallint", nullable: false),
                    Equip13 = table.Column<short>(type: "smallint", nullable: false),
                    Equip14 = table.Column<short>(type: "smallint", nullable: false),
                    Equip15 = table.Column<short>(type: "smallint", nullable: false),
                    Equip16 = table.Column<short>(type: "smallint", nullable: false),
                    Bag1 = table.Column<short>(type: "smallint", nullable: false),
                    Bag2 = table.Column<short>(type: "smallint", nullable: false),
                    Bag3 = table.Column<short>(type: "smallint", nullable: false),
                    Bag4 = table.Column<short>(type: "smallint", nullable: false),
                    Bag5 = table.Column<short>(type: "smallint", nullable: false),
                    Bag6 = table.Column<short>(type: "smallint", nullable: false),
                    Bag7 = table.Column<short>(type: "smallint", nullable: false),
                    Bag8 = table.Column<short>(type: "smallint", nullable: false),
                    Bag9 = table.Column<short>(type: "smallint", nullable: false),
                    Bag10 = table.Column<short>(type: "smallint", nullable: false),
                    Bag11 = table.Column<short>(type: "smallint", nullable: false),
                    Bag12 = table.Column<short>(type: "smallint", nullable: false),
                    Bag13 = table.Column<short>(type: "smallint", nullable: false),
                    Bag14 = table.Column<short>(type: "smallint", nullable: false),
                    Bag15 = table.Column<short>(type: "smallint", nullable: false),
                    Bag16 = table.Column<short>(type: "smallint", nullable: false),
                    Bag17 = table.Column<short>(type: "smallint", nullable: false),
                    Bag18 = table.Column<short>(type: "smallint", nullable: false),
                    Bag19 = table.Column<short>(type: "smallint", nullable: false),
                    Bag20 = table.Column<short>(type: "smallint", nullable: false),
                    Bag21 = table.Column<short>(type: "smallint", nullable: false),
                    Bag22 = table.Column<short>(type: "smallint", nullable: false),
                    Bag23 = table.Column<short>(type: "smallint", nullable: false),
                    Bag24 = table.Column<short>(type: "smallint", nullable: false),
                    Bag25 = table.Column<short>(type: "smallint", nullable: false),
                    Bag26 = table.Column<short>(type: "smallint", nullable: false),
                    Bag27 = table.Column<short>(type: "smallint", nullable: false),
                    Bag28 = table.Column<short>(type: "smallint", nullable: false),
                    Bag29 = table.Column<short>(type: "smallint", nullable: false),
                    Bag30 = table.Column<short>(type: "smallint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_o2jam_item", x => x.USER_INDEX_ID);
                    table.ForeignKey(
                        name: "FK_t_o2jam_item_t_o2jam_charinfo_USER_INDEX_ID",
                        column: x => x.USER_INDEX_ID,
                        principalTable: "t_o2jam_charinfo",
                        principalColumn: "USER_INDEX_ID");
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_member_userid",
                table: "member",
                column: "userid",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_t_o2jam_charinfo_USER_ID",
                table: "t_o2jam_charinfo",
                columns: new[] { "USER_ID" },
                unique: true);
            
            migrationBuilder.CreateIndex(
                name: "IX_t_o2jam_charinfo_USER_NICKNAME",
                table: "t_o2jam_charinfo",
                columns: new[] { "USER_NICKNAME" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "member");

            migrationBuilder.DropTable(
                name: "t_o2jam_charcash");

            migrationBuilder.DropTable(
                name: "t_o2jam_item");

            migrationBuilder.DropTable(
                name: "t_o2jam_login");

            migrationBuilder.DropTable(
                name: "t_o2jam_charinfo");
        }
    }
}
