using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicDistributionSystem.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ExpandMediaPlatformSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AlbumId",
                table: "MusicTracks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ArtistId",
                table: "MusicTracks",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TrackNumber",
                table: "MusicTracks",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "MusicTrackId",
                table: "Likes",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "VideoId",
                table: "Likes",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AccessLevel",
                table: "ImageAssets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "CategoryId",
                table: "ImageAssets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "ImageAssets",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DownloadCount",
                table: "ImageAssets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "FileSizeBytes",
                table: "ImageAssets",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.AddColumn<int>(
                name: "Height",
                table: "ImageAssets",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsFeatured",
                table: "ImageAssets",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MimeType",
                table: "ImageAssets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "ImageAssets",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReviewedAtUtc",
                table: "ImageAssets",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ReviewedByUserId",
                table: "ImageAssets",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "ImageAssets",
                type: "nvarchar(180)",
                maxLength: 180,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailPath",
                table: "ImageAssets",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UploadedByEmail",
                table: "ImageAssets",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "UploadedByUserId",
                table: "ImageAssets",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "ViewCount",
                table: "ImageAssets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Width",
                table: "ImageAssets",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "MusicTrackId",
                table: "Comments",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Comments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<Guid>(
                name: "VideoId",
                table: "Comments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Artists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Bio = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ProfilePicturePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BannerImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Country = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    IsVerified = table.Column<bool>(type: "bit", nullable: false),
                    WebsiteUrl = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    InstagramUrl = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TwitterUrl = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    SpotifyUrl = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    YouTubeUrl = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Artists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Artists_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Playlists",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CoverImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsPublic = table.Column<bool>(type: "bit", nullable: false),
                    IsEditorial = table.Column<bool>(type: "bit", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Playlists", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Playlists_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Albums",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    ArtistId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CoverImagePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1500)", maxLength: 1500, nullable: true),
                    ReleaseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AccessLevel = table.Column<int>(type: "int", nullable: false),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    ApprovalStatus = table.Column<int>(type: "int", nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Albums", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Albums_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Albums_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Albums_Users_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Videos",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(180)", maxLength: 180, nullable: false),
                    Slug = table.Column<string>(type: "nvarchar(220)", maxLength: 220, nullable: false),
                    ArtistName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    ArtistId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OriginalFileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ThumbnailPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Duration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ApprovalStatus = table.Column<int>(type: "int", nullable: false),
                    AccessLevel = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ViewCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    DownloadCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    UploadedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UploadedByName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UploadedByEmail = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ReviewedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ReviewedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Videos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Videos_Artists_ArtistId",
                        column: x => x.ArtistId,
                        principalTable: "Artists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Videos_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Videos_Users_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Videos_Users_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PlaylistTracks",
                columns: table => new
                {
                    PlaylistId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MusicTrackId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    AddedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlaylistTracks", x => new { x.PlaylistId, x.MusicTrackId });
                    table.ForeignKey(
                        name: "FK_PlaylistTracks_MusicTracks_MusicTrackId",
                        column: x => x.MusicTrackId,
                        principalTable: "MusicTracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlaylistTracks_Playlists_PlaylistId",
                        column: x => x.PlaylistId,
                        principalTable: "Playlists",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MusicTracks_AlbumId",
                table: "MusicTracks",
                column: "AlbumId");

            migrationBuilder.CreateIndex(
                name: "IX_MusicTracks_ApprovalStatus_IsFeatured_CreatedAt",
                table: "MusicTracks",
                columns: new[] { "ApprovalStatus", "IsFeatured", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_MusicTracks_ArtistId",
                table: "MusicTracks",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_Likes_UserId_VideoId",
                table: "Likes",
                columns: new[] { "UserId", "VideoId" });

            migrationBuilder.CreateIndex(
                name: "IX_Likes_VideoId",
                table: "Likes",
                column: "VideoId");

            migrationBuilder.CreateIndex(
                name: "IX_ImageAssets_CategoryId",
                table: "ImageAssets",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_ImageAssets_ReviewedByUserId",
                table: "ImageAssets",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_ImageAssets_UploadedByUserId",
                table: "ImageAssets",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Comments_VideoId_CreatedAt",
                table: "Comments",
                columns: new[] { "VideoId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Albums_ArtistId",
                table: "Albums",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_Albums_CategoryId",
                table: "Albums",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Albums_Slug",
                table: "Albums",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Albums_UploadedByUserId",
                table: "Albums",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Artists_Slug",
                table: "Artists",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Artists_UserId",
                table: "Artists",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_Slug",
                table: "Playlists",
                column: "Slug");

            migrationBuilder.CreateIndex(
                name: "IX_Playlists_UserId",
                table: "Playlists",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PlaylistTracks_MusicTrackId",
                table: "PlaylistTracks",
                column: "MusicTrackId");

            migrationBuilder.CreateIndex(
                name: "IX_Videos_ApprovalStatus_IsFeatured_CreatedAt",
                table: "Videos",
                columns: new[] { "ApprovalStatus", "IsFeatured", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Videos_ArtistId",
                table: "Videos",
                column: "ArtistId");

            migrationBuilder.CreateIndex(
                name: "IX_Videos_CategoryId",
                table: "Videos",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_Videos_ReviewedByUserId",
                table: "Videos",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Videos_Slug",
                table: "Videos",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Videos_UploadedByUserId",
                table: "Videos",
                column: "UploadedByUserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_Videos_VideoId",
                table: "Comments",
                column: "VideoId",
                principalTable: "Videos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ImageAssets_Categories_CategoryId",
                table: "ImageAssets",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ImageAssets_Users_ReviewedByUserId",
                table: "ImageAssets",
                column: "ReviewedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ImageAssets_Users_UploadedByUserId",
                table: "ImageAssets",
                column: "UploadedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_Videos_VideoId",
                table: "Likes",
                column: "VideoId",
                principalTable: "Videos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MusicTracks_Albums_AlbumId",
                table: "MusicTracks",
                column: "AlbumId",
                principalTable: "Albums",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_MusicTracks_Artists_ArtistId",
                table: "MusicTracks",
                column: "ArtistId",
                principalTable: "Artists",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_Videos_VideoId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_ImageAssets_Categories_CategoryId",
                table: "ImageAssets");

            migrationBuilder.DropForeignKey(
                name: "FK_ImageAssets_Users_ReviewedByUserId",
                table: "ImageAssets");

            migrationBuilder.DropForeignKey(
                name: "FK_ImageAssets_Users_UploadedByUserId",
                table: "ImageAssets");

            migrationBuilder.DropForeignKey(
                name: "FK_Likes_Videos_VideoId",
                table: "Likes");

            migrationBuilder.DropForeignKey(
                name: "FK_MusicTracks_Albums_AlbumId",
                table: "MusicTracks");

            migrationBuilder.DropForeignKey(
                name: "FK_MusicTracks_Artists_ArtistId",
                table: "MusicTracks");

            migrationBuilder.DropTable(
                name: "Albums");

            migrationBuilder.DropTable(
                name: "PlaylistTracks");

            migrationBuilder.DropTable(
                name: "Videos");

            migrationBuilder.DropTable(
                name: "Playlists");

            migrationBuilder.DropTable(
                name: "Artists");

            migrationBuilder.DropIndex(
                name: "IX_MusicTracks_AlbumId",
                table: "MusicTracks");

            migrationBuilder.DropIndex(
                name: "IX_MusicTracks_ApprovalStatus_IsFeatured_CreatedAt",
                table: "MusicTracks");

            migrationBuilder.DropIndex(
                name: "IX_MusicTracks_ArtistId",
                table: "MusicTracks");

            migrationBuilder.DropIndex(
                name: "IX_Likes_UserId_VideoId",
                table: "Likes");

            migrationBuilder.DropIndex(
                name: "IX_Likes_VideoId",
                table: "Likes");

            migrationBuilder.DropIndex(
                name: "IX_ImageAssets_CategoryId",
                table: "ImageAssets");

            migrationBuilder.DropIndex(
                name: "IX_ImageAssets_ReviewedByUserId",
                table: "ImageAssets");

            migrationBuilder.DropIndex(
                name: "IX_ImageAssets_UploadedByUserId",
                table: "ImageAssets");

            migrationBuilder.DropIndex(
                name: "IX_Comments_VideoId_CreatedAt",
                table: "Comments");

            migrationBuilder.DropColumn(
                name: "AlbumId",
                table: "MusicTracks");

            migrationBuilder.DropColumn(
                name: "ArtistId",
                table: "MusicTracks");

            migrationBuilder.DropColumn(
                name: "TrackNumber",
                table: "MusicTracks");

            migrationBuilder.DropColumn(
                name: "VideoId",
                table: "Likes");

            migrationBuilder.DropColumn(
                name: "AccessLevel",
                table: "ImageAssets");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "ImageAssets");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "ImageAssets");

            migrationBuilder.DropColumn(
                name: "DownloadCount",
                table: "ImageAssets");

            migrationBuilder.DropColumn(
                name: "FileSizeBytes",
                table: "ImageAssets");

            migrationBuilder.DropColumn(
                name: "Height",
                table: "ImageAssets");

            migrationBuilder.DropColumn(
                name: "IsFeatured",
                table: "ImageAssets");

            migrationBuilder.DropColumn(
                name: "MimeType",
                table: "ImageAssets");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "ImageAssets");

            migrationBuilder.DropColumn(
                name: "ReviewedAtUtc",
                table: "ImageAssets");

            migrationBuilder.DropColumn(
                name: "ReviewedByUserId",
                table: "ImageAssets");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "ImageAssets");

            migrationBuilder.DropColumn(
                name: "ThumbnailPath",
                table: "ImageAssets");

            migrationBuilder.DropColumn(
                name: "UploadedByEmail",
                table: "ImageAssets");

            migrationBuilder.DropColumn(
                name: "UploadedByUserId",
                table: "ImageAssets");

            migrationBuilder.DropColumn(
                name: "ViewCount",
                table: "ImageAssets");

            migrationBuilder.DropColumn(
                name: "Width",
                table: "ImageAssets");

            migrationBuilder.DropColumn(
                name: "VideoId",
                table: "Comments");

            migrationBuilder.AlterColumn<Guid>(
                name: "MusicTrackId",
                table: "Likes",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "MusicTrackId",
                table: "Comments",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Comments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);
        }
    }
}
