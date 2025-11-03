using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Esferas.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddFechaCreacionLinkUnico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCreacion",
                table: "LinksUnicos",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaCreacion",
                table: "LinksUnicos");
        }
    }
}
