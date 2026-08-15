using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mozart.Migrations.Sqlite.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "member",
                columns: table => new
                {
                    id = table.Column<long>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    userid = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    passwd = table.Column<byte[]>(type: "BLOB", nullable: false),
                    registdate = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    vip = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    vipdate = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_member", x => x.id);
                });

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
                    Point = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    O2Cash = table.Column<int>(type: "INTEGER", nullable: false),
                    MusicCash = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    ItemCash = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    CashPoint = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0)
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
                    Bag30 = table.Column<short>(type: "INTEGER", nullable: false),
                    BAG_EXT_COUNT = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                    Bag31 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag32 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag33 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag34 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag35 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag36 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag37 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag38 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag39 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag40 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag41 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag42 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag43 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag44 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag45 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag46 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag47 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag48 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag49 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag50 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag51 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag52 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag53 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag54 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag55 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag56 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag57 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag58 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag59 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag60 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag61 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag62 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag63 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag64 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag65 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag66 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag67 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag68 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag69 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag70 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag71 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag72 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag73 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag74 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag75 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag76 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag77 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag78 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag79 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag80 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag81 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag82 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag83 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag84 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag85 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag86 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag87 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag88 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag89 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag90 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag91 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag92 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag93 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag94 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag95 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag96 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag97 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag98 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag99 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag100 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag101 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag102 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag103 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag104 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag105 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag106 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag107 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag108 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag109 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag110 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag111 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag112 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag113 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag114 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag115 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag116 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag117 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag118 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag119 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag120 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag121 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag122 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag123 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag124 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag125 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag126 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag127 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag128 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag129 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag130 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag131 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag132 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag133 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag134 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag135 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag136 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag137 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag138 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag139 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag140 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag141 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag142 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag143 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag144 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag145 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag146 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag147 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag148 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag149 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag150 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag151 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag152 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag153 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag154 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag155 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag156 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag157 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag158 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag159 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag160 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag161 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag162 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag163 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag164 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag165 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag166 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag167 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag168 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag169 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag170 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag171 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag172 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag173 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag174 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag175 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag176 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag177 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag178 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag179 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Bag180 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0),
                    Equip17 = table.Column<short>(type: "INTEGER", nullable: false, defaultValue: (short)0)
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
