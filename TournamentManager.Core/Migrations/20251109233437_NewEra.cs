using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TournamentManager.Core.Migrations
{
    /// <inheritdoc />
    public partial class NewEra : Migration
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

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Tournaments",
                type: "varchar(15)",
                maxLength: 15,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldMaxLength: 10)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedDate",
                value: new DateTime(2025, 11, 9, 23, 34, 37, 157, DateTimeKind.Utc).AddTicks(6539));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 2,
                column: "CreatedDate",
                value: new DateTime(2025, 11, 9, 23, 34, 37, 157, DateTimeKind.Utc).AddTicks(7015));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: 3,
                column: "CreatedDate",
                value: new DateTime(2025, 11, 9, 23, 34, 37, 157, DateTimeKind.Utc).AddTicks(7017));
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

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Tournaments",
                type: "varchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(15)",
                oldMaxLength: 15)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

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
