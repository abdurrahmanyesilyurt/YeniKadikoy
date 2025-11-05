using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Kadikoy.Migrations
{
    /// <inheritdoc />
    public partial class AddGallery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Galleries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FileName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    S3Url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Order = table.Column<int>(type: "integer", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Galleries", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "97d38ccc-ab79-47fb-a18d-03ac6566c4e2", new DateTime(2025, 10, 30, 21, 33, 47, 237, DateTimeKind.Utc).AddTicks(2746), "AQAAAAIAAYagAAAAEFIkXWMGPHD71xkhOX6zxRa9caOWHRkOY0pAoZXydf0WOucFAGug7IPnTsn64epknQ==", "6b61f33c-9858-4aad-9d4b-3077a403eb1d" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Galleries");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "8c3b7e3f-9a8b-4c7e-a2cb-60c1badab432", new DateTime(2025, 10, 30, 1, 31, 26, 204, DateTimeKind.Utc).AddTicks(8711), "AQAAAAIAAYagAAAAEET66ZfeI7HzZT/demywZtp6rJ/GDJiR8GEHgPv1uSFAONGfZiDn/Wvmu6QCTlkb6g==", "5c9155ac-657c-4b45-804d-b35cd0690e17" });
        }
    }
}
