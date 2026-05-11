using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicDistributionSystem.Migrations
{
    [DbContext(typeof(MusicDistributionSystem.Infrastructure.Persistence.ApplicationDbContext))]
    [Migration("20260503120000_AddMusicReviewAndCoverMetadata")]
    public partial class AddMusicReviewAndCoverMetadata : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CoverImagePath",
                table: "MusicTracks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "MusicTracks",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAtUtc",
                table: "MusicTracks",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReviewedByUserId",
                table: "MusicTracks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_MusicTracks_ReviewedByUserId",
                table: "MusicTracks",
                column: "ReviewedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_MusicTracks_Users_ReviewedByUserId",
                table: "MusicTracks",
                column: "ReviewedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MusicTracks_Users_ReviewedByUserId",
                table: "MusicTracks");

            migrationBuilder.DropIndex(
                name: "IX_MusicTracks_ReviewedByUserId",
                table: "MusicTracks");

            migrationBuilder.DropColumn(
                name: "CoverImagePath",
                table: "MusicTracks");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "MusicTracks");

            migrationBuilder.DropColumn(
                name: "ReviewedAtUtc",
                table: "MusicTracks");

            migrationBuilder.DropColumn(
                name: "ReviewedByUserId",
                table: "MusicTracks");
        }
    }
}
