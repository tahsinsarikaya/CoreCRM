using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CoreCRM.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "entity_definitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    DisplayName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entity_definitions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "entity_records",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    values = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_entity_records", x => x.Id);
                    table.ForeignKey(
                        name: "FK_entity_records_entity_definitions_EntityDefinitionId",
                        column: x => x.EntityDefinitionId,
                        principalTable: "entity_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "field_definitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EntityDefinitionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Label = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    IsRequired = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_field_definitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_field_definitions_entity_definitions_EntityDefinitionId",
                        column: x => x.EntityDefinitionId,
                        principalTable: "entity_definitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_entity_definitions_Name",
                table: "entity_definitions",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_entity_records_EntityDefinitionId",
                table: "entity_records",
                column: "EntityDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_field_definitions_EntityDefinitionId_Name",
                table: "field_definitions",
                columns: new[] { "EntityDefinitionId", "Name" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "entity_records");

            migrationBuilder.DropTable(
                name: "field_definitions");

            migrationBuilder.DropTable(
                name: "entity_definitions");
        }
    }
}
