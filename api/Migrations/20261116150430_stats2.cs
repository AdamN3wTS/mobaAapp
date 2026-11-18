using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace api.Migrations
{
    /// <inheritdoc />
    public partial class stats2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DetectorWardsPlaced",
                table: "matchParticipants",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "GoldEarned",
                table: "matchParticipants",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "InhibitorKills",
                table: "matchParticipants",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TotalDamageDealtToChampions",
                table: "matchParticipants",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TurretKills",
                table: "matchParticipants",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VisionScore",
                table: "matchParticipants",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "VisionWardsBoughtInGame",
                table: "matchParticipants",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WardsKilled",
                table: "matchParticipants",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "WardsPlaced",
                table: "matchParticipants",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DetectorWardsPlaced",
                table: "matchParticipants");

            migrationBuilder.DropColumn(
                name: "GoldEarned",
                table: "matchParticipants");

            migrationBuilder.DropColumn(
                name: "InhibitorKills",
                table: "matchParticipants");

            migrationBuilder.DropColumn(
                name: "TotalDamageDealtToChampions",
                table: "matchParticipants");

            migrationBuilder.DropColumn(
                name: "TurretKills",
                table: "matchParticipants");

            migrationBuilder.DropColumn(
                name: "VisionScore",
                table: "matchParticipants");

            migrationBuilder.DropColumn(
                name: "VisionWardsBoughtInGame",
                table: "matchParticipants");

            migrationBuilder.DropColumn(
                name: "WardsKilled",
                table: "matchParticipants");

            migrationBuilder.DropColumn(
                name: "WardsPlaced",
                table: "matchParticipants");
        }
    }
}
