using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

using Microsoft.EntityFrameworkCore;

namespace DistributedTasksOnTime.MsSqlPersistence;

internal class MsSqlDbContext : DbContext
{
	private readonly MsSqlSettings _msSqlSettings;

	//public MsSqlDbContext()
	//  : this(new MsSqlSettings())
	//{
	//	/* For migration only */
	//}

	public MsSqlDbContext(MsSqlSettings msSqlSettings)
    {
		_msSqlSettings = msSqlSettings;
	}

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
		optionsBuilder.UseSqlServer(_msSqlSettings.ConnectionString);
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		foreach (var entityType in modelBuilder.Model.GetEntityTypes())
		{
			var idProperty = entityType.FindProperty("Id");
			if (idProperty != null)
			{
				entityType.SetPrimaryKey(idProperty);
			}
		}

		var hostRegistrationTable = modelBuilder.Entity<HostRegistrationInfo>().ToTable("DistributedTask_HostRegistration");
		hostRegistrationTable.Ignore(p => p.Key);
		hostRegistrationTable.Ignore(p => p.TaskList);
		hostRegistrationTable.Property(p => p.HostName).HasMaxLength(200);
		hostRegistrationTable.Property(p => p.MachineName).HasMaxLength(200);

		var progressInfoTable = modelBuilder.Entity<ProgressInfo>().ToTable("DistributedTask_ProgressInfo");
		progressInfoTable.Property(p => p.GroupName).HasMaxLength(200);
		progressInfoTable.Property(p => p.Subject).HasMaxLength(500);
		progressInfoTable.Property(p => p.Body).HasMaxLength(1024);
		progressInfoTable.Property(p => p.Entity)
			.HasConversion(
				v => System.Text.Json.JsonSerializer.Serialize(v, new JsonSerializerOptions()),
				v => string.IsNullOrWhiteSpace(v) ? string.Empty : System.Text.Json.JsonSerializer.Deserialize<object>(v!, new JsonSerializerOptions())!
			).HasMaxLength(5000);
		progressInfoTable.Ignore(p => p.EntityName);
		progressInfoTable.Ignore(p => p.EntityId);

		var runningTaskTable = modelBuilder.Entity<RunningTask>().ToTable("DistributedTask_RunningTask");
		runningTaskTable.Ignore(p => p.ProgressLogs);
		runningTaskTable.Property(p => p.TaskName).HasMaxLength(200);
		runningTaskTable.Property(p => p.HostKey).HasMaxLength(200);
		runningTaskTable.Property(p => p.ErrorStack).HasMaxLength(5000);

		var scheduledTaskTable = modelBuilder.Entity<ScheduledTask>().ToTable("DistributedTask_ScheduledTask");
		scheduledTaskTable.Ignore(p => p.FromEditor);
		scheduledTaskTable.Property(p => p.Parameters)
			.HasConversion(
				v => System.Text.Json.JsonSerializer.Serialize(v, new JsonSerializerOptions()),
				v => string.IsNullOrWhiteSpace(v) ? new Dictionary<string, string>() : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(v!, new JsonSerializerOptions())!
			).HasMaxLength(5000);
		scheduledTaskTable.Property(p => p.AssemblyQualifiedName).HasMaxLength(500);
		scheduledTaskTable.Property(p => p.Name).HasMaxLength(100);
		scheduledTaskTable.Property(p => p.Description).HasMaxLength(1024);
	}

	public DbSet<HostRegistrationInfo> HostRegistrations { get; set; }
	public DbSet<ProgressInfo> ProgressInfos { get; set; }
	public DbSet<RunningTask> RunningTasks { get; set; }
	public DbSet<ScheduledTask> ScheduledTasks { get; set; }
}
