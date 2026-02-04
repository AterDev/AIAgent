using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddStorageProvider : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "StorageProviders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(60)", maxLength: 60, nullable: false),
                    IsCloud = table.Column<bool>(type: "boolean", nullable: false),
                    Path = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Endpoint = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    AccessKeyId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    AccessKeySecret = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    BucketName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    Region = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedTime = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false),
                    TenantId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StorageProviders", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_StorageProviders_IsActive",
                table: "StorageProviders",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_StorageProviders_Name",
                table: "StorageProviders",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "StorageProviders");
        }
    }
}
