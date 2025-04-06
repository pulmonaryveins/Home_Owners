using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeOwners.Migrations
{
    /// <inheritdoc />
    public partial class FacilitiesImageUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Make ImageUrl column nullable
            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Facilities",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true,
                defaultValue: "/images/placeholder.jpg");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert to non-nullable without default value
            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Facilities",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: false);
        }
    }
}