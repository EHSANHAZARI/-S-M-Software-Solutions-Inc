using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMSS.Migrations
{
    public partial class HiringClientLogoTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.AddColumn<string>(
                name: "UnsubscribeReasons",
                table: "UnsubscribeUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HiringClientLogos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Client_Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Client_Province = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Hiring_Client_Logo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateAdded = table.Column<DateTime>(name: "Date Added", type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HiringClientLogos", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "ConcurrencyStamp",
                value: "20386210-57b9-46f4-a49a-c89bd6cb94fd");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "1cc1f5c0-d54b-46a7-b0b3-aa0a4845114a");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                column: "ConcurrencyStamp",
                value: "ddc74ee0-4b44-43aa-9538-67bdc44209e7");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HiringClientLogos");

            migrationBuilder.DropColumn(
                name: "UnsubscribeReasons",
                table: "UnsubscribeUsers");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "ConcurrencyStamp",
                value: "8b6ebe87-dee8-41de-ad5b-10a2e1718e3d");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "4bd5d0ed-2184-4037-a187-d62fd49729bc");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                column: "ConcurrencyStamp",
                value: "70374b79-7a0d-402f-913b-b954de4d04ab");
        }
    }
}
