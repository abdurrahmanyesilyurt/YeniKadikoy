using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Kadikoy.Migrations
{
    /// <inheritdoc />
    public partial class AddSponsors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Sponsors",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    SportType = table.Column<int>(type: "integer", nullable: true),
                    Placement = table.Column<int>(type: "integer", nullable: false),
                    PhotoUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    LogoUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    WebsiteUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sponsors", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "8c3b7e3f-9a8b-4c7e-a2cb-60c1badab432", new DateTime(2025, 10, 30, 1, 31, 26, 204, DateTimeKind.Utc).AddTicks(8711), "AQAAAAIAAYagAAAAEET66ZfeI7HzZT/demywZtp6rJ/GDJiR8GEHgPv1uSFAONGfZiDn/Wvmu6QCTlkb6g==", "5c9155ac-657c-4b45-804d-b35cd0690e17" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Sponsors");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "e2bf678a-53fe-4403-8ba0-62131d2fbfd3", new DateTime(2025, 10, 30, 1, 2, 35, 521, DateTimeKind.Utc).AddTicks(3455), "AQAAAAIAAYagAAAAEP+Da87LwQUKuVjHTnRRy3Q5NPTWlexy1cUGlHLxThDG2lstY5E4Gny2Z0L4y9a3jQ==", "d7f17ac2-95ae-442e-ab59-20fdd33d631c" });
        }
    }
}
