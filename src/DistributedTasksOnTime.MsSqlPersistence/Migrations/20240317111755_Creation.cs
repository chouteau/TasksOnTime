using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DistributedTasksOnTime.MsSqlPersistence.Migrations
{
    /// <inheritdoc />
    public partial class Creation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DistributedTask_HostRegistration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MachineName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    HostName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    State = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DistributedTask_HostRegistration", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DistributedTask_ProgressInfo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TaskId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    GroupName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Body = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    Entity = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    TotalCount = table.Column<int>(type: "int", nullable: true),
                    Index = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DistributedTask_ProgressInfo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DistributedTask_RunningTask",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TaskName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    HostKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EnqueuedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RunningDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancelingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CanceledDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TerminatedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorStack = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    IsForced = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DistributedTask_RunningTask", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DistributedTask_ScheduledTask",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Period = table.Column<int>(type: "int", nullable: false),
                    Interval = table.Column<int>(type: "int", nullable: false),
                    StartDay = table.Column<int>(type: "int", nullable: false),
                    StartHour = table.Column<int>(type: "int", nullable: false),
                    StartMinute = table.Column<int>(type: "int", nullable: false),
                    AssemblyQualifiedName = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    StartedCount = table.Column<int>(type: "int", nullable: false),
                    Enabled = table.Column<bool>(type: "bit", nullable: false),
                    AllowMultipleInstance = table.Column<bool>(type: "bit", nullable: false),
                    AllowLocalMultipleInstances = table.Column<bool>(type: "bit", nullable: false),
                    NextRunningDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Parameters = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    ProcessMode = table.Column<int>(type: "int", nullable: false),
                    LastDurationInSeconds = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DistributedTask_ScheduledTask", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DistributedTask_HostRegistration");

            migrationBuilder.DropTable(
                name: "DistributedTask_ProgressInfo");

            migrationBuilder.DropTable(
                name: "DistributedTask_RunningTask");

            migrationBuilder.DropTable(
                name: "DistributedTask_ScheduledTask");
        }
    }
}
