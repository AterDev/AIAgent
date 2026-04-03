using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class SplitApplicationAgents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AgentExecutions_AIAgents_AgentId",
                table: "AgentExecutions");

            migrationBuilder.DropIndex(
                name: "IX_AIAgents_ApplicationId",
                table: "AIAgents");

            migrationBuilder.DropIndex(
                name: "IX_AIAgents_Name",
                table: "AIAgents");

            migrationBuilder.DropIndex(
                name: "IX_AIAgents_UserId",
                table: "AIAgents");

            migrationBuilder.DropIndex(
                name: "IX_AgentExecutions_AgentId_Status",
                table: "AgentExecutions");

            migrationBuilder.DropColumn(
                name: "ApplicationId",
                table: "AIAgents");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "AIAgents");

            migrationBuilder.RenameColumn(
                name: "IsTemplate",
                table: "AIAgents",
                newName: "IsPublic");

            migrationBuilder.AddColumn<bool>(
                name: "IsApplicationAgent",
                table: "AgentExecutions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "ApplicationAgents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    ModelId = table.Column<string>(type: "text", nullable: false),
                    SystemPrompt = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    Tools = table.Column<List<string>>(type: "text[]", nullable: false),
                    Enable = table.Column<bool>(type: "boolean", nullable: false),
                    ApplicationId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationAgents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ApplicationAgents_Applications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "Applications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIAgents_Name",
                table: "AIAgents",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AgentExecutions_AgentId_IsApplicationAgent_Status",
                table: "AgentExecutions",
                columns: new[] { "AgentId", "IsApplicationAgent", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationAgents_ApplicationId",
                table: "ApplicationAgents",
                columns: new[] { "TenantId", "ApplicationId" });

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationAgents_ApplicationId_Name",
                table: "ApplicationAgents",
                columns: new[] { "TenantId", "ApplicationId", "Name" },
                unique: true,
                filter: "\"IsDeleted\" = false");

            migrationBuilder.CreateIndex(
                name: "IX_ApplicationAgents_UserId",
                table: "ApplicationAgents",
                columns: new[] { "TenantId", "UserId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationAgents");

            migrationBuilder.DropIndex(
                name: "IX_AIAgents_Name",
                table: "AIAgents");

            migrationBuilder.DropIndex(
                name: "IX_AgentExecutions_AgentId_IsApplicationAgent_Status",
                table: "AgentExecutions");

            migrationBuilder.DropColumn(
                name: "IsApplicationAgent",
                table: "AgentExecutions");

            migrationBuilder.RenameColumn(
                name: "IsPublic",
                table: "AIAgents",
                newName: "IsTemplate");

            migrationBuilder.AddColumn<Guid>(
                name: "ApplicationId",
                table: "AIAgents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "UserId",
                table: "AIAgents",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AIAgents_ApplicationId",
                table: "AIAgents",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_AIAgents_Name",
                table: "AIAgents",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_AIAgents_UserId",
                table: "AIAgents",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AgentExecutions_AgentId_Status",
                table: "AgentExecutions",
                columns: new[] { "AgentId", "Status" });

            migrationBuilder.AddForeignKey(
                name: "FK_AgentExecutions_AIAgents_AgentId",
                table: "AgentExecutions",
                column: "AgentId",
                principalTable: "AIAgents",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
