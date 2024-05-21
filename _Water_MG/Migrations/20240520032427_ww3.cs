using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _Water_MG.Migrations
{
    /// <inheritdoc />
    public partial class ww3 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TypeMeter",
                table: "Meters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TypeMeter",
                table: "Meters");
        }
    }
}
