using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kadikoy.Migrations
{
    /// <inheritdoc />
    public partial class AddNewsTypeToNews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "NewsType",
                table: "News",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "e2bf678a-53fe-4403-8ba0-62131d2fbfd3", new DateTime(2025, 10, 30, 1, 2, 35, 521, DateTimeKind.Utc).AddTicks(3455), "AQAAAAIAAYagAAAAEP+Da87LwQUKuVjHTnRRy3Q5NPTWlexy1cUGlHLxThDG2lstY5E4Gny2Z0L4y9a3jQ==", "d7f17ac2-95ae-442e-ab59-20fdd33d631c" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NewsType",
                table: "News");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "1f60f72a-a124-42f7-9117-f3e0a2d50383", new DateTime(2025, 10, 29, 23, 1, 52, 886, DateTimeKind.Utc).AddTicks(8691), "AQAAAAIAAYagAAAAEIZG13slbLthTvdXHzs1TRbPux75ni0VPM7B8UuGzLA8GE9w1OF4vpwAs/Jso3fp/A==", "d12760cd-8f5e-443f-adb0-a092ae020ef1" });
        }
    }
}
