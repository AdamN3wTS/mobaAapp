using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class AddRuneStats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ChampionStatsRune",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChampionStatsInfoId = table.Column<Guid>(type: "uuid", nullable: false),
                    PerkId = table.Column<int>(type: "integer", nullable: false),
                    Games = table.Column<int>(type: "integer", nullable: false),
                    WinRate = table.Column<double>(type: "double precision", nullable: false),
                    PickRate = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChampionStatsRune", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChampionStatsRune_championStatsInfos_ChampionStatsInfoId",
                        column: x => x.ChampionStatsInfoId,
                        principalTable: "championStatsInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChampionStatsRunePage",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChampionStatsInfoId = table.Column<Guid>(type: "uuid", nullable: false),
                    KeystoneId = table.Column<int>(type: "integer", nullable: false),
                    PrimaryStyleId = table.Column<int>(type: "integer", nullable: false),
                    SubStyleId = table.Column<int>(type: "integer", nullable: false),
                    Games = table.Column<int>(type: "integer", nullable: false),
                    WinRate = table.Column<double>(type: "double precision", nullable: false),
                    PickRate = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChampionStatsRunePage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChampionStatsRunePage_championStatsInfos_ChampionStatsInfoId",
                        column: x => x.ChampionStatsInfoId,
                        principalTable: "championStatsInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChampionStatsShard",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ChampionStatsInfoId = table.Column<Guid>(type: "uuid", nullable: false),
                    OffensePerk = table.Column<int>(type: "integer", nullable: false),
                    FlexPerk = table.Column<int>(type: "integer", nullable: false),
                    DefensePerk = table.Column<int>(type: "integer", nullable: false),
                    Games = table.Column<int>(type: "integer", nullable: false),
                    WinRate = table.Column<double>(type: "double precision", nullable: false),
                    PickRate = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChampionStatsShard", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChampionStatsShard_championStatsInfos_ChampionStatsInfoId",
                        column: x => x.ChampionStatsInfoId,
                        principalTable: "championStatsInfos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChampionStatsRune_ChampionStatsInfoId",
                table: "ChampionStatsRune",
                column: "ChampionStatsInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_ChampionStatsRunePage_ChampionStatsInfoId",
                table: "ChampionStatsRunePage",
                column: "ChampionStatsInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_ChampionStatsShard_ChampionStatsInfoId",
                table: "ChampionStatsShard",
                column: "ChampionStatsInfoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChampionStatsRune");

            migrationBuilder.DropTable(
                name: "ChampionStatsRunePage");

            migrationBuilder.DropTable(
                name: "ChampionStatsShard");
        }
    }
}
