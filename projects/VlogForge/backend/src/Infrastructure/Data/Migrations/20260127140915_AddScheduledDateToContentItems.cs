using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VlogForge.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddScheduledDateToContentItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "scheduled_date",
                table: "content_items",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_content_items_scheduled_date",
                table: "content_items",
                column: "scheduled_date");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_content_items_scheduled_date",
                table: "content_items");

            migrationBuilder.DropColumn(
                name: "scheduled_date",
                table: "content_items");
        }
    }
}
