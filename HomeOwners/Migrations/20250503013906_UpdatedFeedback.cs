using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HomeOwners.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedFeedback : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "HasRating",
                table: "Bookings",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HasRating",
                table: "Bookings");
        }
    }
}
