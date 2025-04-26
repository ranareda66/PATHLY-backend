using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PATHLY_API.Migrations
{
    /// <inheritdoc />
    public partial class trip : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LocationId",
                table: "Users",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "Users");
        }
    }
}
