using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MusicDistributionSystem.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ConsolidateMusicTrackAndAddSlugs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_MediaAssets_MediaAssetId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_DownloadRecords_MediaAssets_MediaAssetId",
                table: "DownloadRecords");

            migrationBuilder.DropForeignKey(
                name: "FK_Likes_MediaAssets_MediaAssetId",
                table: "Likes");

            migrationBuilder.DropForeignKey(
                name: "FK_MusicTracks_Categories_CategoryId",
                table: "MusicTracks");

            migrationBuilder.DropTable(
                name: "MediaAssetTags");

            migrationBuilder.DropTable(
                name: "MediaAssets");

            migrationBuilder.DropIndex(
                name: "IX_DownloadRecords_MediaAssetId",
                table: "DownloadRecords");

            migrationBuilder.DropColumn(
                name: "MediaAssetId",
                table: "DownloadRecords");

            migrationBuilder.RenameColumn(
                name: "MediaAssetId",
                table: "Likes",
                newName: "MusicTrackId");

            migrationBuilder.RenameIndex(
                name: "IX_Likes_UserId_MediaAssetId",
                table: "Likes",
                newName: "IX_Likes_UserId_MusicTrackId");

            migrationBuilder.RenameIndex(
                name: "IX_Likes_MediaAssetId",
                table: "Likes",
                newName: "IX_Likes_MusicTrackId");

            migrationBuilder.RenameColumn(
                name: "MediaAssetId",
                table: "Comments",
                newName: "MusicTrackId");

            migrationBuilder.RenameIndex(
                name: "IX_Comments_MediaAssetId_CreatedAt",
                table: "Comments",
                newName: "IX_Comments_MusicTrackId_CreatedAt");

            migrationBuilder.AddColumn<string>(
                name: "Duration",
                table: "MusicTracks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MimeType",
                table: "MusicTracks",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PlayCount",
                table: "MusicTracks",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "MusicTracks",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Slug",
                table: "Categories",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            // Populate Slugs for existing categories to prevent unique index violation on default empty string ""
            migrationBuilder.Sql(@"
                UPDATE Categories 
                SET Slug = LOWER(REPLACE(REPLACE(Name, ' ', '-'), '&', 'and'))
                WHERE Slug = '' OR Slug IS NULL;

                UPDATE Categories 
                SET Slug = Slug + '-' + SUBSTRING(CAST(Id AS nvarchar(36)), 1, 8)
                WHERE Id IN (
                    SELECT Id FROM (
                        SELECT Id, ROW_NUMBER() OVER (PARTITION BY Slug ORDER BY Id) as RowNum
                        FROM Categories
                    ) t WHERE t.RowNum > 1
                );
            ");

            // Populate Slugs for existing music tracks to prevent unique index violation
            migrationBuilder.Sql(@"
                UPDATE MusicTracks 
                SET Slug = LOWER(REPLACE(REPLACE(Title, ' ', '-'), '&', 'and')) + '-' + LOWER(REPLACE(REPLACE(Artist, ' ', '-'), '&', 'and'))
                WHERE Slug = '' OR Slug IS NULL;

                UPDATE MusicTracks 
                SET Slug = Slug + '-' + SUBSTRING(CAST(Id AS nvarchar(36)), 1, 8)
                WHERE Id IN (
                    SELECT Id FROM (
                        SELECT Id, ROW_NUMBER() OVER (PARTITION BY Slug ORDER BY Id) as RowNum
                        FROM MusicTracks
                    ) t WHERE t.RowNum > 1
                );
            ");

            migrationBuilder.CreateTable(
                name: "TrackTags",
                columns: table => new
                {
                    MusicTrackId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TagId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrackTags", x => new { x.MusicTrackId, x.TagId });
                    table.ForeignKey(
                        name: "FK_TrackTags_MusicTracks_MusicTrackId",
                        column: x => x.MusicTrackId,
                        principalTable: "MusicTracks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrackTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MusicTracks_Slug",
                table: "MusicTracks",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Slug",
                table: "Categories",
                column: "Slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrackTags_TagId",
                table: "TrackTags",
                column: "TagId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_MusicTracks_MusicTrackId",
                table: "Comments",
                column: "MusicTrackId",
                principalTable: "MusicTracks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_MusicTracks_MusicTrackId",
                table: "Likes",
                column: "MusicTrackId",
                principalTable: "MusicTracks",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MusicTracks_Categories_CategoryId",
                table: "MusicTracks",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Comments_MusicTracks_MusicTrackId",
                table: "Comments");

            migrationBuilder.DropForeignKey(
                name: "FK_Likes_MusicTracks_MusicTrackId",
                table: "Likes");

            migrationBuilder.DropForeignKey(
                name: "FK_MusicTracks_Categories_CategoryId",
                table: "MusicTracks");

            migrationBuilder.DropTable(
                name: "TrackTags");

            migrationBuilder.DropIndex(
                name: "IX_MusicTracks_Slug",
                table: "MusicTracks");

            migrationBuilder.DropIndex(
                name: "IX_Categories_Slug",
                table: "Categories");

            migrationBuilder.DropColumn(
                name: "Duration",
                table: "MusicTracks");

            migrationBuilder.DropColumn(
                name: "MimeType",
                table: "MusicTracks");

            migrationBuilder.DropColumn(
                name: "PlayCount",
                table: "MusicTracks");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "MusicTracks");

            migrationBuilder.DropColumn(
                name: "Slug",
                table: "Categories");

            migrationBuilder.RenameColumn(
                name: "MusicTrackId",
                table: "Likes",
                newName: "MediaAssetId");

            migrationBuilder.RenameIndex(
                name: "IX_Likes_UserId_MusicTrackId",
                table: "Likes",
                newName: "IX_Likes_UserId_MediaAssetId");

            migrationBuilder.RenameIndex(
                name: "IX_Likes_MusicTrackId",
                table: "Likes",
                newName: "IX_Likes_MediaAssetId");

            migrationBuilder.RenameColumn(
                name: "MusicTrackId",
                table: "Comments",
                newName: "MediaAssetId");

            migrationBuilder.RenameIndex(
                name: "IX_Comments_MusicTrackId_CreatedAt",
                table: "Comments",
                newName: "IX_Comments_MediaAssetId_CreatedAt");

            migrationBuilder.AddColumn<Guid>(
                name: "MediaAssetId",
                table: "DownloadRecords",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MediaAssets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReviewedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UploadedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AccessLevel = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    ApprovalStatus = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DownloadCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    Duration = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    IsFeatured = table.Column<bool>(type: "bit", nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OriginalFileName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    RejectionReason = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Resolution = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ReviewedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ThumbnailPath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false, defaultValue: 0),
                    UploadedByEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UploadedByName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ViewCount = table.Column<int>(type: "int", nullable: false, defaultValue: 0)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaAssets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MediaAssets_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MediaAssets_Users_ReviewedByUserId",
                        column: x => x.ReviewedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_MediaAssets_Users_UploadedByUserId",
                        column: x => x.UploadedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MediaAssetTags",
                columns: table => new
                {
                    MediaAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TagId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaAssetTags", x => new { x.MediaAssetId, x.TagId });
                    table.ForeignKey(
                        name: "FK_MediaAssetTags_MediaAssets_MediaAssetId",
                        column: x => x.MediaAssetId,
                        principalTable: "MediaAssets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MediaAssetTags_Tags_TagId",
                        column: x => x.TagId,
                        principalTable: "Tags",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DownloadRecords_MediaAssetId",
                table: "DownloadRecords",
                column: "MediaAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaAssets_CategoryId",
                table: "MediaAssets",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaAssets_ReviewedByUserId",
                table: "MediaAssets",
                column: "ReviewedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaAssets_UploadedByUserId",
                table: "MediaAssets",
                column: "UploadedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_MediaAssetTags_TagId",
                table: "MediaAssetTags",
                column: "TagId");

            migrationBuilder.AddForeignKey(
                name: "FK_Comments_MediaAssets_MediaAssetId",
                table: "Comments",
                column: "MediaAssetId",
                principalTable: "MediaAssets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DownloadRecords_MediaAssets_MediaAssetId",
                table: "DownloadRecords",
                column: "MediaAssetId",
                principalTable: "MediaAssets",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Likes_MediaAssets_MediaAssetId",
                table: "Likes",
                column: "MediaAssetId",
                principalTable: "MediaAssets",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MusicTracks_Categories_CategoryId",
                table: "MusicTracks",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
