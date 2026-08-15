using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Mozart.Migrations.SqlServer.Migrations
{
    /// <inheritdoc />
    public partial class CreateUserMessage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "t_o2jam_message",
                columns: table => new
                {
                    Seq = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SenderID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SenderIndexID = table.Column<int>(type: "int", nullable: false),
                    SenderNickName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReceiverID = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ReceiverIndexID = table.Column<int>(type: "int", nullable: false),
                    ReceiverNickName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Content = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    WriteDate = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    ReadDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReadFlag = table.Column<string>(type: "char(1)", nullable: false, defaultValue: "0"),
                    TypeFlag = table.Column<string>(type: "char(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_t_o2jam_message", x => x.Seq);
                    table.ForeignKey(
                        name: "FK_t_o2jam_message_t_o2jam_charinfo_ReceiverIndexID",
                        column: x => x.ReceiverIndexID,
                        principalTable: "t_o2jam_charinfo",
                        principalColumn: "USER_INDEX_ID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_t_o2jam_message_ReceiverIndexID",
                table: "t_o2jam_message",
                column: "ReceiverIndexID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "t_o2jam_message");
        }
    }
}
