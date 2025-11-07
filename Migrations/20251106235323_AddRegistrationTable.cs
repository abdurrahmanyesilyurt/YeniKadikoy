using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Kadikoy.Migrations
{
    /// <inheritdoc />
    public partial class AddRegistrationTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Registrations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AthleteFullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    BirthDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ParentFullName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    ParentPhone = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    ParentEmail = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ParentAddress = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Branch = table.Column<int>(type: "integer", nullable: false),
                    AgeGroup = table.Column<int>(type: "integer", nullable: false),
                    ProgramType = table.Column<int>(type: "integer", nullable: false),
                    HealthNotes = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsApproved = table.Column<bool>(type: "boolean", nullable: false),
                    ApprovedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ApprovedByUserId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Registrations", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "b048ed10-3d83-41b8-b4ab-1cb491b0446b", new DateTime(2025, 11, 6, 23, 53, 22, 274, DateTimeKind.Utc).AddTicks(9059), "AQAAAAIAAYagAAAAEJgeQRbibEB+YfT6mjgBO8XgYHqNhd54ZHAfs0rnvK8cVFVyHNQr1CjH6ZKz0r7L0w==", "8976b8e4-15aa-4a4a-b46f-96c1dad2f5b5" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Registrations");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "ConcurrencyStamp", "CreatedAt", "PasswordHash", "SecurityStamp" },
                values: new object[] { "29e7bbea-ec3a-4e43-aecf-9f0af9feffd4", new DateTime(2025, 10, 30, 21, 39, 30, 641, DateTimeKind.Utc).AddTicks(5616), "AQAAAAIAAYagAAAAEM32BH4Fiy9qCtWUfnHGg88D4nyA/IHE+AP5dBnzawFuwuFW8EeOiOUkX1JnMEyKPA==", "511d78a2-25cd-412b-a116-28da3bb03459" });
        }
    }
}
