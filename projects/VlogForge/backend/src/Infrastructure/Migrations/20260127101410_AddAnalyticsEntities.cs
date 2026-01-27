using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable
#pragma warning disable CA1861 // Avoid constant arrays as arguments (EF Core generated code)

namespace VlogForge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAnalyticsEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "creator_profiles",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    username = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    display_name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    bio = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    profile_picture_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    open_to_collaborations = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    collaboration_preferences = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_creator_profiles", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(254)", maxLength: 254, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    EmailVerified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    EmailVerificationTokenHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailVerificationTokenExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    PasswordResetTokenHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    PasswordResetTokenExpiry = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    FailedLoginAttempts = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    LockoutEnd = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Version = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "connected_platforms",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    profile_id = table.Column<Guid>(type: "uuid", nullable: false),
                    platform_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    handle = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    profile_url = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    is_verified = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    follower_count = table.Column<int>(type: "integer", nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_connected_platforms", x => x.id);
                    table.ForeignKey(
                        name: "FK_connected_platforms_creator_profiles_profile_id",
                        column: x => x.profile_id,
                        principalTable: "creator_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "creator_profile_niche_tags",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    tag = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    CreatorProfileId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_creator_profile_niche_tags", x => x.id);
                    table.ForeignKey(
                        name: "FK_creator_profile_niche_tags_creator_profiles_CreatorProfileId",
                        column: x => x.CreatorProfileId,
                        principalTable: "creator_profiles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "metrics_snapshots",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    platform_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    snapshot_date = table.Column<DateTime>(type: "date", nullable: false),
                    follower_count = table.Column<long>(type: "bigint", nullable: false),
                    daily_views = table.Column<long>(type: "bigint", nullable: false),
                    daily_likes = table.Column<long>(type: "bigint", nullable: false),
                    daily_comments = table.Column<long>(type: "bigint", nullable: false),
                    engagement_rate = table.Column<double>(type: "double precision", precision: 10, scale: 4, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_metrics_snapshots", x => x.id);
                    table.ForeignKey(
                        name: "FK_metrics_snapshots_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "platform_connections",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    platform_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    encrypted_access_token = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    encrypted_refresh_token = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    token_expires_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    platform_account_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    platform_account_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    last_sync_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    error_message = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_connections", x => x.id);
                    table.ForeignKey(
                        name: "FK_platform_connections_Users_user_id",
                        column: x => x.user_id,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    TokenHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RevokedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    RevokedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ReplacedByTokenHash = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedByIp = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "content_performances",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    platform_connection_id = table.Column<Guid>(type: "uuid", nullable: false),
                    platform_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    content_id = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    title = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    thumbnail_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    content_url = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    published_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    view_count = table.Column<long>(type: "bigint", nullable: false),
                    like_count = table.Column<long>(type: "bigint", nullable: false),
                    comment_count = table.Column<long>(type: "bigint", nullable: false),
                    share_count = table.Column<long>(type: "bigint", nullable: false),
                    engagement_rate = table.Column<double>(type: "double precision", precision: 10, scale: 4, nullable: false),
                    last_updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_content_performances", x => x.id);
                    table.ForeignKey(
                        name: "FK_content_performances_platform_connections_platform_connecti~",
                        column: x => x.platform_connection_id,
                        principalTable: "platform_connections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "platform_metrics",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    platform_connection_id = table.Column<Guid>(type: "uuid", nullable: false),
                    platform_type = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    follower_count = table.Column<long>(type: "bigint", nullable: false),
                    total_views = table.Column<long>(type: "bigint", nullable: false),
                    total_likes = table.Column<long>(type: "bigint", nullable: false),
                    total_comments = table.Column<long>(type: "bigint", nullable: false),
                    total_shares = table.Column<long>(type: "bigint", nullable: false),
                    engagement_rate = table.Column<double>(type: "double precision", precision: 10, scale: 4, nullable: false),
                    metrics_updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    updated_by = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_platform_metrics", x => x.id);
                    table.ForeignKey(
                        name: "FK_platform_metrics_platform_connections_platform_connection_id",
                        column: x => x.platform_connection_id,
                        principalTable: "platform_connections",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_connected_platforms_profile_id_platform_type",
                table: "connected_platforms",
                columns: new[] { "profile_id", "platform_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_content_performances_engagement_rate",
                table: "content_performances",
                column: "engagement_rate");

            migrationBuilder.CreateIndex(
                name: "IX_content_performances_platform_connection_id_content_id",
                table: "content_performances",
                columns: new[] { "platform_connection_id", "content_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_content_performances_platform_type",
                table: "content_performances",
                column: "platform_type");

            migrationBuilder.CreateIndex(
                name: "IX_content_performances_view_count",
                table: "content_performances",
                column: "view_count");

            migrationBuilder.CreateIndex(
                name: "IX_creator_profile_niche_tags_CreatorProfileId_tag",
                table: "creator_profile_niche_tags",
                columns: new[] { "CreatorProfileId", "tag" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_creator_profiles_user_id",
                table: "creator_profiles",
                column: "user_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_creator_profiles_username",
                table: "creator_profiles",
                column: "username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_metrics_snapshots_platform_type",
                table: "metrics_snapshots",
                column: "platform_type");

            migrationBuilder.CreateIndex(
                name: "IX_metrics_snapshots_user_id_platform_type_snapshot_date",
                table: "metrics_snapshots",
                columns: new[] { "user_id", "platform_type", "snapshot_date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_metrics_snapshots_user_id_snapshot_date",
                table: "metrics_snapshots",
                columns: new[] { "user_id", "snapshot_date" });

            migrationBuilder.CreateIndex(
                name: "IX_platform_connections_token_expires_at",
                table: "platform_connections",
                column: "token_expires_at");

            migrationBuilder.CreateIndex(
                name: "IX_platform_connections_user_id_platform_type",
                table: "platform_connections",
                columns: new[] { "user_id", "platform_type" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_platform_metrics_platform_connection_id",
                table: "platform_metrics",
                column: "platform_connection_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_platform_metrics_platform_type",
                table: "platform_metrics",
                column: "platform_type");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_TokenHash",
                table: "RefreshTokens",
                column: "TokenHash");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "connected_platforms");

            migrationBuilder.DropTable(
                name: "content_performances");

            migrationBuilder.DropTable(
                name: "creator_profile_niche_tags");

            migrationBuilder.DropTable(
                name: "metrics_snapshots");

            migrationBuilder.DropTable(
                name: "platform_metrics");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "creator_profiles");

            migrationBuilder.DropTable(
                name: "platform_connections");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
