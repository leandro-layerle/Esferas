using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Esferas.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarLinkUnicoARespuesta : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "LinkUnicoId",
                table: "Respuestas",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_Respuestas_LinkUnicoId",
                table: "Respuestas",
                column: "LinkUnicoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Respuestas_LinksUnicos_LinkUnicoId",
                table: "Respuestas",
                column: "LinkUnicoId",
                principalTable: "LinksUnicos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Respuestas_LinksUnicos_LinkUnicoId",
                table: "Respuestas");

            migrationBuilder.DropIndex(
                name: "IX_Respuestas_LinkUnicoId",
                table: "Respuestas");

            migrationBuilder.DropColumn(
                name: "LinkUnicoId",
                table: "Respuestas");
        }
    }
}
