using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Esferas.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEsLinkEmpresaToLinkUnico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "EsLinkEmpresa",
                table: "LinksUnicos",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaFinalizacion",
                table: "LinksUnicos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "Token",
                table: "LinksUnicos",
                type: "uniqueidentifier",
                nullable: false,
                defaultValueSql: "NEWID()");

            migrationBuilder.CreateIndex(
                name: "IX_LinksUnicos_Token",
                table: "LinksUnicos",
                column: "Token",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_LinksUnicos_Token",
                table: "LinksUnicos");

            migrationBuilder.DropColumn(
                name: "EsLinkEmpresa",
                table: "LinksUnicos");

            migrationBuilder.DropColumn(
                name: "FechaFinalizacion",
                table: "LinksUnicos");

            migrationBuilder.DropColumn(
                name: "Token",
                table: "LinksUnicos");
        }
    }
}
