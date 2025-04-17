using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeOwners.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedServicePersonnel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ServicePersonnelId",
                table: "ServiceRequests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceRequests_ServicePersonnelId",
                table: "ServiceRequests",
                column: "ServicePersonnelId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceRequests_ServicePersonnel_ServicePersonnelId",
                table: "ServiceRequests",
                column: "ServicePersonnelId",
                principalTable: "ServicePersonnel",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceRequests_ServicePersonnel_ServicePersonnelId",
                table: "ServiceRequests");

            migrationBuilder.DropIndex(
                name: "IX_ServiceRequests_ServicePersonnelId",
                table: "ServiceRequests");

            migrationBuilder.DropColumn(
                name: "ServicePersonnelId",
                table: "ServiceRequests");
        }
    }
}
