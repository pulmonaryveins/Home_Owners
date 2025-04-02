using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeOwners.Migrations
{
    /// <inheritdoc />
    public partial class UpdateHomeOwnerFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Address",
                table: "HomeOwnerUsers");

            migrationBuilder.RenameColumn(
                name: "PropertyId",
                table: "HomeOwnerUsers",
                newName: "HouseNumber");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "HouseNumber",
                table: "HomeOwnerUsers",
                newName: "PropertyId");

            migrationBuilder.AddColumn<string>(
                name: "Address",
                table: "HomeOwnerUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
