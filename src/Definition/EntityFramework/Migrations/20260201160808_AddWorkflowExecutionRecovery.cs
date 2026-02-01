using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EntityFramework.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkflowExecutionRecovery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContextJson",
                table: "WorkflowExecutions",
                type: "character varying(8000)",
                maxLength: 8000,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "ExecutedStepCount",
                table: "WorkflowExecutions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "ExecutionMode",
                table: "WorkflowExecutions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsAbandoned",
                table: "WorkflowExecutions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LastCheckpointStepIndex",
                table: "WorkflowExecutions",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxRetries",
                table: "WorkflowExecutions",
                type: "integer",
                nullable: false,
                defaultValue: 3);

            // Backfill existing rows with default MaxRetries value
            migrationBuilder.Sql(
                "UPDATE \"WorkflowExecutions\" SET \"MaxRetries\" = 3 WHERE \"MaxRetries\" = 0;");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ResumedAt",
                table: "WorkflowExecutions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RetryCount",
                table: "WorkflowExecutions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "StepExecutionsJson",
                table: "WorkflowExecutions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContextJson",
                table: "WorkflowExecutions");

            migrationBuilder.DropColumn(
                name: "ExecutedStepCount",
                table: "WorkflowExecutions");

            migrationBuilder.DropColumn(
                name: "ExecutionMode",
                table: "WorkflowExecutions");

            migrationBuilder.DropColumn(
                name: "IsAbandoned",
                table: "WorkflowExecutions");

            migrationBuilder.DropColumn(
                name: "LastCheckpointStepIndex",
                table: "WorkflowExecutions");

            migrationBuilder.DropColumn(
                name: "MaxRetries",
                table: "WorkflowExecutions");

            migrationBuilder.DropColumn(
                name: "ResumedAt",
                table: "WorkflowExecutions");

            migrationBuilder.DropColumn(
                name: "RetryCount",
                table: "WorkflowExecutions");

            migrationBuilder.DropColumn(
                name: "StepExecutionsJson",
                table: "WorkflowExecutions");
        }
    }
}
