using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ITSystem.Migrations
{
    /// <inheritdoc />
    public partial class RenameTemperatureToProducedCount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TemperatureC",
                table: "ProductionLogs",
                newName: "ProducedCount");

            migrationBuilder.AlterColumn<int>(
                name: "ProducedCount",
                table: "ProductionLogs",
                type: "int",
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "ProducedCount",
                table: "ProductionLogs",
                type: "float",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.RenameColumn(
                name: "ProducedCount",
                table: "ProductionLogs",
                newName: "TemperatureC");
        }
    }
}