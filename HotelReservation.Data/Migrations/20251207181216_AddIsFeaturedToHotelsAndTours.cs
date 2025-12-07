using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelReservation.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIsFeaturedToHotelsAndTours : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "Tours",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "Hotels",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "Tours");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "Hotels");
        }
    }
}
