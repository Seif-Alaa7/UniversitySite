using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class computedcolumnsql : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "DegreePoints",
                table: "StudentSubjects",
                type: "decimal(18,3)",
                nullable: false,
                computedColumnSql: "CASE\r\n                                  WHEN Grade IS NULL THEN NULL\r\n                                  WHEN Grade = 0 THEN 4.0\r\n                                  WHEN Grade = 1 THEN 3.667\r\n                                  WHEN Grade = 2 THEN 3.333\r\n                                  WHEN Grade = 3 THEN 3.0\r\n                                  WHEN Grade = 4 THEN 2.667\r\n                                  WHEN Grade = 5 THEN 2.333\r\n                                  WHEN Grade = 6 THEN 2.0\r\n                                  ELSE 0.0 END",
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<decimal>(
                name: "DegreePoints",
                table: "StudentSubjects",
                type: "decimal(18,3)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,3)",
                oldComputedColumnSql: "CASE\r\n                                  WHEN Grade IS NULL THEN NULL\r\n                                  WHEN Grade = 0 THEN 4.0\r\n                                  WHEN Grade = 1 THEN 3.667\r\n                                  WHEN Grade = 2 THEN 3.333\r\n                                  WHEN Grade = 3 THEN 3.0\r\n                                  WHEN Grade = 4 THEN 2.667\r\n                                  WHEN Grade = 5 THEN 2.333\r\n                                  WHEN Grade = 6 THEN 2.0\r\n                                  ELSE 0.0 END");
        }
    }
}
