using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CollectorShop.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ReviewStatusEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reviews_IsApproved",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Reviews");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Reviews",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_Status",
                table: "Reviews",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Reviews_Status",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Reviews");

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Reviews",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_IsApproved",
                table: "Reviews",
                column: "IsApproved");
        }
    }
}
