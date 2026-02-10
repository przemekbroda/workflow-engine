using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExampleApp.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ProcessRequests",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TestStateSnapshot = table.Column<string>(type: "jsonb", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessRequests", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProcessRequestEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "uuidv7()"),
                    EventName = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Index = table.Column<int>(type: "integer", nullable: false),
                    ProcessRequestEventPayload = table.Column<string>(type: "jsonb", nullable: true),
                    ProcessRequestId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProcessRequestEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProcessRequestEvents_ProcessRequests_ProcessRequestId",
                        column: x => x.ProcessRequestId,
                        principalTable: "ProcessRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProcessRequestEvents_ProcessRequestId",
                table: "ProcessRequestEvents",
                column: "ProcessRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_ProcessRequests_LastModifiedAt",
                table: "ProcessRequests",
                column: "LastModifiedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ProcessRequestEvents");

            migrationBuilder.DropTable(
                name: "ProcessRequests");
        }
    }
}
