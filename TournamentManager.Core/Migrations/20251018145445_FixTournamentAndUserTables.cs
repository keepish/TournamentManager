using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TournamentManager.Core.Migrations
{
    /// <inheritdoc />
    public partial class FixTournamentAndUserTables : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "Users",
                newName: "Login");

            migrationBuilder.RenameColumn(
                name: "CrearedDate",
                table: "Tournaments",
                newName: "CreatedDate");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 10, 18, 14, 54, 44, 487, DateTimeKind.Utc).AddTicks(9479));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 10, 18, 14, 54, 44, 487, DateTimeKind.Utc).AddTicks(9995));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2025, 10, 18, 14, 54, 44, 487, DateTimeKind.Utc).AddTicks(9997));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Login",
                table: "Users",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "CreatedDate",
                table: "Tournaments",
                newName: "CrearedDate");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 10, 15, 20, 33, 44, 489, DateTimeKind.Local).AddTicks(8113));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 10, 15, 20, 33, 44, 489, DateTimeKind.Local).AddTicks(8592));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2025, 10, 15, 20, 33, 44, 489, DateTimeKind.Local).AddTicks(8595));
        }
    }
}
