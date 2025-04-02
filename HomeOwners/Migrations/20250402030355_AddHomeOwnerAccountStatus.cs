using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeOwners.Migrations
{
    /// <inheritdoc />
    public partial class AddHomeOwnerAccountStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountStatus",
                table: "HomeOwnerUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "HomeOwnerUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccountStatus",
                table: "HomeOwnerUsers");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "HomeOwnerUsers");
        }
    }
}
