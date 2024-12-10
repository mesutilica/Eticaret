using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Eticaret.Data.Migrations
{
    /// <inheritdoc />
    public partial class NewsConfigurationDesc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "News",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(750)",
                oldMaxLength: 750,
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AppUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreateDate", "UserGuid" },
                values: new object[] { new DateTime(2024, 11, 1, 8, 5, 3, 496, DateTimeKind.Local).AddTicks(7968), new Guid("b3ca673c-71a5-4eb4-a4bb-5ea51dc04469") });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreateDate",
                value: new DateTime(2024, 11, 1, 8, 5, 3, 497, DateTimeKind.Local).AddTicks(1299));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreateDate",
                value: new DateTime(2024, 11, 1, 8, 5, 3, 497, DateTimeKind.Local).AddTicks(1305));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "News",
                type: "nvarchar(750)",
                maxLength: 750,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "AppUsers",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "CreateDate", "UserGuid" },
                values: new object[] { new DateTime(2024, 10, 20, 8, 48, 52, 707, DateTimeKind.Local).AddTicks(3447), new Guid("4a023f59-6783-47b7-b3b8-29be7694ede7") });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreateDate",
                value: new DateTime(2024, 10, 20, 8, 48, 52, 707, DateTimeKind.Local).AddTicks(5779));

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreateDate",
                value: new DateTime(2024, 10, 20, 8, 48, 52, 707, DateTimeKind.Local).AddTicks(5828));
        }
    }
}
