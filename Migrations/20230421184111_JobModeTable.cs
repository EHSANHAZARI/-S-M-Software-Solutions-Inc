using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SMSS.Migrations
{
    public partial class JobModeTable : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Id",
                table: "ProvinceDemoFileAttachments",
                newName: "ID");

            migrationBuilder.AddColumn<int>(
                name: "Job_Mode_ID",
                table: "Company_Jobs",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Province_Demo_File_Attachment_ID",
                table: "Company_Jobs",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "JobModes",
                columns: table => new
                {
                    ID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Job_Mode_Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobModes", x => x.ID);
                });


            migrationBuilder.CreateTable(
                name: "UnsubscribeUsers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnsubscribeUsers", x => x.Id);
                });

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

            migrationBuilder.CreateIndex(
                name: "IX_Company_Jobs_Job_Mode_ID",
                table: "Company_Jobs",
                column: "Job_Mode_ID");

            migrationBuilder.CreateIndex(
                name: "IX_Company_Jobs_Province_Demo_File_Attachment_ID",
                table: "Company_Jobs",
                column: "Province_Demo_File_Attachment_ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Company_Jobs_JobModes_Job_Mode_ID",
                table: "Company_Jobs",
                column: "Job_Mode_ID",
                principalTable: "JobModes",
                principalColumn: "ID");

            migrationBuilder.AddForeignKey(
                name: "FK_Company_Jobs_ProvinceDemoFileAttachments_Province_Demo_File_Attachment_ID",
                table: "Company_Jobs",
                column: "Province_Demo_File_Attachment_ID",
                principalTable: "ProvinceDemoFileAttachments",
                principalColumn: "ID");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Company_Jobs_JobModes_Job_Mode_ID",
                table: "Company_Jobs");

            migrationBuilder.DropForeignKey(
                name: "FK_Company_Jobs_ProvinceDemoFileAttachments_Province_Demo_File_Attachment_ID",
                table: "Company_Jobs");

            migrationBuilder.DropTable(
                name: "JobModes");

            migrationBuilder.DropTable(
                name: "UnsubscribeUserEmailRequests");

            migrationBuilder.DropTable(
                name: "UnsubscribeUsers");

            migrationBuilder.DropIndex(
                name: "IX_Company_Jobs_Job_Mode_ID",
                table: "Company_Jobs");

            migrationBuilder.DropIndex(
                name: "IX_Company_Jobs_Province_Demo_File_Attachment_ID",
                table: "Company_Jobs");

            migrationBuilder.DropColumn(
                name: "Job_Mode_ID",
                table: "Company_Jobs");

            migrationBuilder.DropColumn(
                name: "Province_Demo_File_Attachment_ID",
                table: "Company_Jobs");

            migrationBuilder.RenameColumn(
                name: "ID",
                table: "ProvinceDemoFileAttachments",
                newName: "Id");

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
        }
    }
}
