using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace RifaDeliverySystem.Web.Migrations
{
    public partial class anulanul : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Annulments");

            migrationBuilder.DropTable(
                name: "BulkAnnulments");

            migrationBuilder.AddColumn<int>(
                name: "Extravio",
                table: "Renditions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Robo",
                table: "Renditions",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Extravio",
                table: "Renditions");

            migrationBuilder.DropColumn(
                name: "Robo",
                table: "Renditions");

            migrationBuilder.CreateTable(
                name: "Annulments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    RenditionId = table.Column<int>(type: "integer", nullable: false),
                    Count = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Annulments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Annulments_Renditions_RenditionId",
                        column: x => x.RenditionId,
                        principalTable: "Renditions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BulkAnnulments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    VendorId = table.Column<int>(type: "integer", nullable: false),
                    Date = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndNumber = table.Column<int>(type: "integer", nullable: false),
                    Reason = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    StartNumber = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BulkAnnulments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BulkAnnulments_Vendors_VendorId",
                        column: x => x.VendorId,
                        principalTable: "Vendors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Annulments_RenditionId",
                table: "Annulments",
                column: "RenditionId");

            migrationBuilder.CreateIndex(
                name: "IX_BulkAnnulments_VendorId",
                table: "BulkAnnulments",
                column: "VendorId");
        }
    }
}
