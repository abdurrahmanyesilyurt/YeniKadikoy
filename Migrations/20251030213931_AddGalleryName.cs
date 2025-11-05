using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Kadikoy.Migrations
{
    /// <inheritdoc />
    public partial class AddGalleryName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Galleries",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "29e7bbea-ec3a-4e43-aecf-9f0af9feffd4", new DateTime(2025, 10, 30, 21, 39, 30, 641, DateTimeKind.Utc).AddTicks(5616), "AQAAAAIAAYagAAAAEM32BH4Fiy9qCtWUfnHGg88D4nyA/IHE+AP5dBnzawFuwuFW8EeOiOUkX1JnMEyKPA==", "511d78a2-25cd-412b-a116-28da3bb03459" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Galleries");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "97d38ccc-ab79-47fb-a18d-03ac6566c4e2", new DateTime(2025, 10, 30, 21, 33, 47, 237, DateTimeKind.Utc).AddTicks(2746), "AQAAAAIAAYagAAAAEFIkXWMGPHD71xkhOX6zxRa9caOWHRkOY0pAoZXydf0WOucFAGug7IPnTsn64epknQ==", "6b61f33c-9858-4aad-9d4b-3077a403eb1d" });
        }
    }
}
