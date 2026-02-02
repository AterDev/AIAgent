using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddStorageTypeToRagDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Step 1: Added nullable column without default
            migrationBuilder.AddColumn<int>(
                name: "StorageType",
                table: "RagDocuments",
                type: "integer",
                nullable: true);

            // Step 2: Backfilled based on FilePath patterns
            // S3 storage indicators:
            // - URLs starting with http/https/s3 (direct S3 URLs)
            // - Object keys: simple paths without filesystem prefixes (uploads/, /, C:\)
            // Note: This heuristic may not catch all edge cases (e.g., relative paths like '../file.pdf')
            // but covers the common patterns used in the application
            migrationBuilder.Sql(@"
                UPDATE ""RagDocuments""
                SET ""StorageType"" = 1
                WHERE ""FilePath"" LIKE 'http://%' 
                   OR ""FilePath"" LIKE 'https://%'
                   OR ""FilePath"" LIKE 's3://%'
                   OR (""FilePath"" NOT LIKE 'uploads/%' AND ""FilePath"" NOT LIKE '/%' AND ""FilePath"" NOT LIKE '%:\\%');
            ");

            // Default remaining nulls to Local (0)
            migrationBuilder.Sql(@"
                UPDATE ""RagDocuments""
                SET ""StorageType"" = 0
                WHERE ""StorageType"" IS NULL;
            ");

            // Step 3: Made column non-nullable with default value for new rows
            migrationBuilder.AlterColumn<int>(
                name: "StorageType",
                table: "RagDocuments",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StorageType",
                table: "RagDocuments");
        }
    }
}
