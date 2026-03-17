using CollectorShop.Infrastructure.Data.Seeding;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CollectorShop.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedCatalogData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            SeedDataHelper.SeedCatalogData(migrationBuilder);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            SeedDataHelper.UnseedCatalogData(migrationBuilder);
        }
    }
}
