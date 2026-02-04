using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddAIPromptAndRagAgentConfig : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StorageType",
                table: "RagDocuments");

            migrationBuilder.RenameColumn(
                name: "ContentType",
                table: "RagDocuments",
                newName: "FileType");

            migrationBuilder.AddColumn<Guid>(
                name: "StorageProviderId",
                table: "RagDocuments",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AlterColumn<string>(
                name: "SystemPrompt",
                table: "AIAgents",
                type: "character varying(5000)",
                maxLength: 5000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "AIPrompts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Content = table.Column<string>(type: "character varying(5000)", maxLength: 5000, nullable: false),
                    GroupName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AIPrompts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RagAgentConfigs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Key = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    AIModelInfoId = table.Column<Guid>(type: "uuid", nullable: true),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    AIPromptId = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RagAgentConfigs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RagAgentConfigs_AIModelInfos_AIModelInfoId",
                        column: x => x.AIModelInfoId,
                        principalTable: "AIModelInfos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_RagAgentConfigs_AIPrompts_AIPromptId",
                        column: x => x.AIPromptId,
                        principalTable: "AIPrompts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AIPrompts_GroupName",
                table: "AIPrompts",
                column: "GroupName");

            migrationBuilder.CreateIndex(
                name: "IX_AIPrompts_Name",
                table: "AIPrompts",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_AIPrompts_Name_GroupName",
                table: "AIPrompts",
                columns: new[] { "Name", "GroupName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RagAgentConfigs_AIModelInfoId",
                table: "RagAgentConfigs",
                column: "AIModelInfoId");

            migrationBuilder.CreateIndex(
                name: "IX_RagAgentConfigs_AIPromptId",
                table: "RagAgentConfigs",
                column: "AIPromptId");

            migrationBuilder.CreateIndex(
                name: "IX_RagAgentConfigs_Key",
                table: "RagAgentConfigs",
                column: "Key",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RagAgentConfigs");

            migrationBuilder.DropTable(
                name: "AIPrompts");

            migrationBuilder.DropColumn(
                name: "StorageProviderId",
                table: "RagDocuments");

            migrationBuilder.RenameColumn(
                name: "FileType",
                table: "RagDocuments",
                newName: "ContentType");

            migrationBuilder.AddColumn<int>(
                name: "StorageType",
                table: "RagDocuments",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "SystemPrompt",
                table: "AIAgents",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(5000)",
                oldMaxLength: 5000);
        }
    }
}
