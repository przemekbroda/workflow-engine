using ExampleApp.Postgres.Models;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExampleApp.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class NullableEvent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ProcessRequestEventPayload",
                table: "ProcessRequestEvents",
                type: "jsonb",
                nullable: true,
                oldClrType: typeof(ProcessRequestEventPayload),
                oldType: "jsonb");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<ProcessRequestEventPayload>(
                name: "ProcessRequestEventPayload",
                table: "ProcessRequestEvents",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "jsonb",
                oldNullable: true);
        }
    }
}
