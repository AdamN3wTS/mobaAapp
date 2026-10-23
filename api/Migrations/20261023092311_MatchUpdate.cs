using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class MatchUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "matches",
                columns: table => new
                {
                    MatchId = table.Column<string>(type: "text", nullable: false),
                    Patch = table.Column<string>(type: "text", nullable: false),
                    DurationSeconds = table.Column<int>(type: "integer", nullable: false),
                    PlayedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_matches", x => x.MatchId);
                });

            migrationBuilder.CreateTable(
                name: "matchBans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchId = table.Column<string>(type: "text", nullable: false),
                    ChampionId = table.Column<int>(type: "integer", nullable: true),
                    TeamId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_matchBans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_matchBans_matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "matches",
                        principalColumn: "MatchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "matchParticipants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchId = table.Column<string>(type: "text", nullable: false),
                    Puuid = table.Column<string>(type: "text", nullable: false),
                    GameName = table.Column<string>(type: "text", nullable: false),
                    TagLine = table.Column<string>(type: "text", nullable: false),
                    ChampionId = table.Column<int>(type: "integer", nullable: false),
                    ChampionName = table.Column<string>(type: "text", nullable: false),
                    TeamPosition = table.Column<string>(type: "text", nullable: false),
                    TeamId = table.Column<int>(type: "integer", nullable: false),
                    IsWin = table.Column<bool>(type: "boolean", nullable: false),
                    Level = table.Column<int>(type: "integer", nullable: false),
                    Kills = table.Column<int>(type: "integer", nullable: false),
                    Deaths = table.Column<int>(type: "integer", nullable: false),
                    Assists = table.Column<int>(type: "integer", nullable: false),
                    Cs = table.Column<int>(type: "integer", nullable: false),
                    Spell1Id = table.Column<int>(type: "integer", nullable: false),
                    Spell2Id = table.Column<int>(type: "integer", nullable: false),
                    ItemIds = table.Column<List<int>>(type: "integer[]", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_matchParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_matchParticipants_matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "matches",
                        principalColumn: "MatchId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_matchBans_MatchId",
                table: "matchBans",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_matchParticipants_MatchId",
                table: "matchParticipants",
                column: "MatchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "matchBans");

            migrationBuilder.DropTable(
                name: "matchParticipants");

            migrationBuilder.DropTable(
                name: "matches");
        }
    }
}
