using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace TournamentManager.Core.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "category",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    minWeight = table.Column<decimal>(type: "decimal(4,1)", precision: 4, scale: 1, nullable: false),
                    maxWeight = table.Column<decimal>(type: "decimal(4,1)", precision: 4, scale: 1, nullable: false),
                    minAge = table.Column<int>(type: "int", nullable: false),
                    maxAge = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "participant",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    surname = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    patronymic = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    phone = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true),
                    gender = table.Column<ulong>(type: "bit(1)", nullable: false),
                    birthday = table.Column<DateTime>(type: "datetime", nullable: false),
                    weight = table.Column<decimal>(type: "decimal(4,1)", precision: 4, scale: 1, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "user",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    surname = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    patronymic = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true),
                    login = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    passwordHash = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "tournament",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    organizerId = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    description = table.Column<string>(type: "varchar(300)", maxLength: 300, nullable: true),
                    startDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    endDate = table.Column<DateTime>(type: "datetime", nullable: false),
                    address = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "organizerId",
                        column: x => x.organizerId,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "tournament_category",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    tournamentId = table.Column<int>(type: "int", nullable: false),
                    categoryId = table.Column<int>(type: "int", nullable: false),
                    judgeId = table.Column<int>(type: "int", nullable: false),
                    sitesNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "categoryId",
                        column: x => x.categoryId,
                        principalTable: "category",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "judgeId",
                        column: x => x.judgeId,
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "tournamentId",
                        column: x => x.tournamentId,
                        principalTable: "tournament",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "participant_tournament_category",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    tournamentCategoryId = table.Column<int>(type: "int", nullable: false),
                    participantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "participantId",
                        column: x => x.participantId,
                        principalTable: "participant",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "tournamentCategoryId",
                        column: x => x.tournamentCategoryId,
                        principalTable: "tournament_category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "match",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    firstParticipantId = table.Column<int>(type: "int", nullable: false),
                    secondParticipantId = table.Column<int>(type: "int", nullable: true),
                    firstParticipantScore = table.Column<int>(type: "int", nullable: false),
                    secondParticipantScore = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PRIMARY", x => x.id);
                    table.ForeignKey(
                        name: "firstParticipantId",
                        column: x => x.firstParticipantId,
                        principalTable: "participant_tournament_category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "secondParticipantId",
                        column: x => x.secondParticipantId,
                        principalTable: "participant_tournament_category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySQL:Charset", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "firstParticipantId_idx",
                table: "match",
                column: "firstParticipantId");

            migrationBuilder.CreateIndex(
                name: "secondParticipantId_idx",
                table: "match",
                column: "secondParticipantId");

            migrationBuilder.CreateIndex(
                name: "participantId_idx",
                table: "participant_tournament_category",
                column: "participantId");

            migrationBuilder.CreateIndex(
                name: "tournamentCategoryId_idx",
                table: "participant_tournament_category",
                column: "tournamentCategoryId");

            migrationBuilder.CreateIndex(
                name: "id_idx",
                table: "tournament",
                column: "organizerId");

            migrationBuilder.CreateIndex(
                name: "id_idx1",
                table: "tournament_category",
                column: "categoryId");

            migrationBuilder.CreateIndex(
                name: "id_idx11",
                table: "tournament_category",
                column: "tournamentId");

            migrationBuilder.CreateIndex(
                name: "judgeId_idx",
                table: "tournament_category",
                column: "judgeId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "match");

            migrationBuilder.DropTable(
                name: "participant_tournament_category");

            migrationBuilder.DropTable(
                name: "participant");

            migrationBuilder.DropTable(
                name: "tournament_category");

            migrationBuilder.DropTable(
                name: "category");

            migrationBuilder.DropTable(
                name: "tournament");

            migrationBuilder.DropTable(
                name: "user");
        }
    }
}
