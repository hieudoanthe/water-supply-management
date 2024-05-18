using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _Water_MG.Migrations
{
    /// <inheritdoc />
    public partial class w_1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TypeAccount",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TypeAccount",
                table: "Accounts");
        }
    }
}
