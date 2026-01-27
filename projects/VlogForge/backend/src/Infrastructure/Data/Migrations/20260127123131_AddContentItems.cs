using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
#pragma warning disable CA1861 // Avoid constant arrays as arguments

namespace VlogForge.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddContentItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_content_performances_engagement_rate",
                table: "content_performances");

            migrationBuilder.DropIndex(
                name: "IX_content_performances_view_count",
                table: "content_performances");

            migrationBuilder.CreateTable(
                name: "content_items",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    title = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    notes = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: true),
                    status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    platform_tags = table.Column<string>(type: "jsonb", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_content_items", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_content_performances_platform_connection_id_comment_count",
                table: "content_performances",
                columns: new[] { "platform_connection_id", "comment_count" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_content_performances_platform_connection_id_engagement_rate",
                table: "content_performances",
                columns: new[] { "platform_connection_id", "engagement_rate" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_content_performances_platform_connection_id_like_count",
                table: "content_performances",
                columns: new[] { "platform_connection_id", "like_count" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_content_performances_platform_connection_id_view_count",
                table: "content_performances",
                columns: new[] { "platform_connection_id", "view_count" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_content_items_is_deleted",
                table: "content_items",
                column: "is_deleted");

            migrationBuilder.CreateIndex(
                name: "IX_content_items_status",
                table: "content_items",
                column: "status");

            migrationBuilder.CreateIndex(
                name: "IX_content_items_user_id",
                table: "content_items",
                column: "user_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "content_items");

            migrationBuilder.DropIndex(
                name: "IX_content_performances_platform_connection_id_comment_count",
                table: "content_performances");

            migrationBuilder.DropIndex(
                name: "IX_content_performances_platform_connection_id_engagement_rate",
                table: "content_performances");

            migrationBuilder.DropIndex(
                name: "IX_content_performances_platform_connection_id_like_count",
                table: "content_performances");

            migrationBuilder.DropIndex(
                name: "IX_content_performances_platform_connection_id_view_count",
                table: "content_performances");

            migrationBuilder.CreateIndex(
                name: "IX_content_performances_engagement_rate",
                table: "content_performances",
                column: "engagement_rate");

            migrationBuilder.CreateIndex(
                name: "IX_content_performances_view_count",
                table: "content_performances",
                column: "view_count");
        }
    }
}

#pragma warning restore CA1861
