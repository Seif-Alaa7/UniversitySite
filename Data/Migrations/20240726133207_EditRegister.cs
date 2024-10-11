using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class EditRegister : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Students",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "HighBoards",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ApplicationUserId",
                table: "Doctors",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "UserType",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Students_ApplicationUserId",
                table: "Students",
                column: "ApplicationUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HighBoards_ApplicationUserId",
                table: "HighBoards",
                column: "ApplicationUserId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Doctors_ApplicationUserId",
                table: "Doctors",
                column: "ApplicationUserId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Doctors_AspNetUsers_ApplicationUserId",
                table: "Doctors",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_HighBoards_AspNetUsers_ApplicationUserId",
                table: "HighBoards",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);

            migrationBuilder.AddForeignKey(
                name: "FK_Students_AspNetUsers_ApplicationUserId",
                table: "Students",
                column: "ApplicationUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Doctors_AspNetUsers_ApplicationUserId",
                table: "Doctors");

            migrationBuilder.DropForeignKey(
                name: "FK_HighBoards_AspNetUsers_ApplicationUserId",
                table: "HighBoards");

            migrationBuilder.DropForeignKey(
                name: "FK_Students_AspNetUsers_ApplicationUserId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_Students_ApplicationUserId",
                table: "Students");

            migrationBuilder.DropIndex(
                name: "IX_HighBoards_ApplicationUserId",
                table: "HighBoards");

            migrationBuilder.DropIndex(
                name: "IX_Doctors_ApplicationUserId",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Students");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "HighBoards");

            migrationBuilder.DropColumn(
                name: "ApplicationUserId",
                table: "Doctors");

            migrationBuilder.DropColumn(
                name: "UserType",
                table: "AspNetUsers");
        }
    }
}
