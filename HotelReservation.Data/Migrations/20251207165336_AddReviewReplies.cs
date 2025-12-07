using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelReservation.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddReviewReplies : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReservationId",
                table: "HotelReviews",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "HotelReviews",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ReviewReplies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ReviewId = table.Column<int>(type: "int", nullable: false),
                    ManagerId = table.Column<int>(type: "int", nullable: false),
                    ReplyText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewReplies", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewReplies_HotelReviews_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "HotelReviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ReviewReplies_Users_ManagerId",
                        column: x => x.ManagerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HotelReviews_ReservationId",
                table: "HotelReviews",
                column: "ReservationId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewReplies_ManagerId",
                table: "ReviewReplies",
                column: "ManagerId");

            migrationBuilder.CreateIndex(
                name: "IX_ReviewReplies_ReviewId",
                table: "ReviewReplies",
                column: "ReviewId");

            migrationBuilder.AddForeignKey(
                name: "FK_HotelReviews_Reservations_ReservationId",
                table: "HotelReviews",
                column: "ReservationId",
                principalTable: "Reservations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_HotelReviews_Reservations_ReservationId",
                table: "HotelReviews");

            migrationBuilder.DropTable(
                name: "ReviewReplies");

            migrationBuilder.DropIndex(
                name: "IX_HotelReviews_ReservationId",
                table: "HotelReviews");

            migrationBuilder.DropColumn(
                name: "ReservationId",
                table: "HotelReviews");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "HotelReviews");
        }
    }
}
