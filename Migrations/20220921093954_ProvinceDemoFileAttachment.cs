using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SMSS.Migrations
{
    public partial class ProvinceDemoFileAttachment : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProvinceDemoFileAttachments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Province_Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    File_Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Attachment = table.Column<byte[]>(type: "varbinary(MAX)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProvinceDemoFileAttachments", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "ConcurrencyStamp",
                value: "02aa430c-f34e-48a3-b385-c36a87eed6ac");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "b44960c2-42cc-4e5b-aefb-6c5234c458b8");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                column: "ConcurrencyStamp",
                value: "014e7f1e-5fff-4028-9bb2-184aa72bc752");

            migrationBuilder.InsertData(
                table: "Sectors",
                columns: new[] { "ID", "Sector_Name" },
                values: new object[] { 98, "UX designer" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProvinceDemoFileAttachments");

            migrationBuilder.DeleteData(
                table: "Sectors",
                keyColumn: "ID",
                keyValue: 98);

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "ConcurrencyStamp",
                value: "f46cfb58-723d-491a-bfeb-e6d48f667b78");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "ConcurrencyStamp",
                value: "af0d52ae-078d-4537-9b1a-5548d7611c1e");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                column: "ConcurrencyStamp",
                value: "e22fb404-8ef5-4ef9-9bee-a8442855207f");
        }
    }
}
