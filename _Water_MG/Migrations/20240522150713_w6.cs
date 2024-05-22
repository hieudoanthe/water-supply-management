using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace _Water_MG.Migrations
{
    /// <inheritdoc />
    public partial class w6 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "TypePay",
                table: "Payment",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TypePay",
                table: "Payment");
        }
    }
}
