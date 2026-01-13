using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ExampleApp.Postgres.Migrations
{
    /// <inheritdoc />
    public partial class LastModifiedAtIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ProcessRequests_LastModifiedAt",
                table: "ProcessRequests",
                column: "LastModifiedAt");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ProcessRequests_LastModifiedAt",
                table: "ProcessRequests");
        }
    }
}
