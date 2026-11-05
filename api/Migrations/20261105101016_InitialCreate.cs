using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "championStatsInfos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChampionId = table.Column<int>(type: "integer", nullable: false),
                    ChampionName = table.Column<string>(type: "text", nullable: false),
                    Games = table.Column<int>(type: "integer", nullable: false),
                    WinRate = table.Column<double>(type: "double precision", nullable: false),
                    PickRate = table.Column<double>(type: "double precision", nullable: false),
                    BanRate = table.Column<double>(type: "double precision", nullable: false),
                    TeamPosition = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_championStatsInfos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "crawlPlayers",
                columns: table => new
                {
                    Puuid = table.Column<string>(type: "text", nullable: false),
                    Tier = table.Column<string>(type: "text", nullable: true),
                    Platform = table.Column<string>(type: "text", nullable: false),
                    LastCrawledAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_crawlPlayers", x => x.Puuid);
                });

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
                name: "championStatsItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChampionStatsInfoId = table.Column<Guid>(type: "uuid", nullable: false),
                    ItemId = table.Column<int>(type: "integer", nullable: false),
                    Games = table.Column<int>(type: "integer", nullable: false),
                    WinRate = table.Column<double>(type: "double precision", nullable: false),
                    PickRate = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_championStatsItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_championStatsItems_championStatsInfos_ChampionStatsInfoId",
                        column: x => x.ChampionStatsInfoId,
                        principalTable: "championStatsInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "championStatsSpells",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChampionStatsInfoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Spell1Id = table.Column<int>(type: "integer", nullable: false),
                    Spell2Id = table.Column<int>(type: "integer", nullable: false),
                    PickRate = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_championStatsSpells", x => x.Id);
                    table.ForeignKey(
                        name: "FK_championStatsSpells_championStatsInfos_ChampionStatsInfoId",
                        column: x => x.ChampionStatsInfoId,
                        principalTable: "championStatsInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "matchBans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchId = table.Column<string>(type: "text", nullable: false),
                    ChampionId = table.Column<int>(type: "integer", nullable: false),
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
                    ItemIds = table.Column<List<int>>(type: "integer[]", nullable: false),
                    PrimaryStyleId = table.Column<int>(type: "integer", nullable: false),
                    SubStyleId = table.Column<int>(type: "integer", nullable: false),
                    KeystoneId = table.Column<int>(type: "integer", nullable: false),
                    OffensePerk = table.Column<int>(type: "integer", nullable: false),
                    FlexPerk = table.Column<int>(type: "integer", nullable: false),
                    DefensePerk = table.Column<int>(type: "integer", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "participantRunes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MatchParticipantId = table.Column<Guid>(type: "uuid", nullable: false),
                    Slot = table.Column<byte>(type: "smallint", nullable: false),
                    StyleId = table.Column<int>(type: "integer", nullable: false),
                    PerkId = table.Column<int>(type: "integer", nullable: false),
                    Var1 = table.Column<int>(type: "integer", nullable: false),
                    Var2 = table.Column<int>(type: "integer", nullable: false),
                    Var3 = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_participantRunes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_participantRunes_matchParticipants_MatchParticipantId",
                        column: x => x.MatchParticipantId,
                        principalTable: "matchParticipants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_championStatsItems_ChampionStatsInfoId",
                table: "championStatsItems",
                column: "ChampionStatsInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_championStatsSpells_ChampionStatsInfoId",
                table: "championStatsSpells",
                column: "ChampionStatsInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_matchBans_MatchId",
                table: "matchBans",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_matchParticipants_MatchId",
                table: "matchParticipants",
                column: "MatchId");

            migrationBuilder.CreateIndex(
                name: "IX_participantRunes_MatchParticipantId",
                table: "participantRunes",
                column: "MatchParticipantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "championStatsItems");

            migrationBuilder.DropTable(
                name: "championStatsSpells");

            migrationBuilder.DropTable(
                name: "crawlPlayers");

            migrationBuilder.DropTable(
                name: "matchBans");

            migrationBuilder.DropTable(
                name: "participantRunes");

            migrationBuilder.DropTable(
                name: "championStatsInfos");

            migrationBuilder.DropTable(
                name: "matchParticipants");

            migrationBuilder.DropTable(
                name: "matches");
        }
    }
}
