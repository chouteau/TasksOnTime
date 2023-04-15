using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

using DistributedTasksOnTime.SqlitePersistence.Extensions;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DistributedTasksOnTime.SqlitePersistence;

public static class StartupExtensions
{
    public static IServiceCollection AddTasksOnTimeSqlitePersistence(this IServiceCollection services, Action<SqliteSettings> config)
    {
        var settings = new SqliteSettings();
        config(settings);

		var csb = new SqliteConnectionStringBuilder(settings.ConnectionString);
		var directory = System.IO.Path.GetDirectoryName(csb.DataSource)!;

        if (directory.StartsWith(@".\"))
        {
            var currentFolder = System.IO.Path.GetDirectoryName(typeof(StartupExtensions).Assembly.Location)!;
            directory = System.IO.Path.Combine(currentFolder, directory);
        }
        if (!System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }
		var dbFileName = System.IO.Path.GetFileName(csb.DataSource);

		csb.DataSource = System.IO.Path.Combine(directory, dbFileName);
        settings.ConnectionString = csb.ConnectionString;

        services.AddSingleton(settings);

        services.AddAutoMapper(config =>
        {
            config.AddProfile<Mapping>();
        });
		services.AddTransient<IDbRepository, SqlDbRepository>();
		if (!services.IsRegistered<IMemoryCache>())
		{
			services.AddMemoryCache();
		}
        services.AddDbContextFactory<SqliteDbContext>(lifetime: ServiceLifetime.Transient);
		services.AddHostedService<DbCleaner>();
        return services;
    }

    public async static Task UseTasksOnTimeSqlitePersistence(this IServiceProvider serviceProvider)
    {
        var settings = serviceProvider.GetRequiredService<SqliteSettings>();
		var logger = serviceProvider.GetRequiredService<ILogger<SqliteSettings>>();

		logger.LogInformation($"CS:{settings.ConnectionString}");

		try
		{
			await CreateRunningTable(settings.ConnectionString);
			await CreateScheduledTable(settings.ConnectionString);
			await CreateHostRegistrationTable(settings.ConnectionString);
			await CreateProgressInfoTable(settings.ConnectionString);
			// await TerminateAllRunningTasks(serviceProvider);
		}
		catch (Exception ex)
		{
			logger.LogCritical(ex, ex.Message);
		}
    }

    static async Task CreateRunningTable(string cs)
    {
        var table = @"
Create table if not exists
	RunningTask (
		Id uniqueidentifier primary key,
		TaskName nvarchar(200) not null,
		HostKey nvarchar(200) null,
		CreationDate DateTime not null,
		EnqueuedDate DateTime null,
		RunningDate DateTime null,
		CancelingDate DateTime null,
		CanceledDate DateTime null,
		TerminatedDate DateTime null,
		FailedDate DateTime null,
		ErrorStack nvarchar(5000),
		IsForced bit null
	)
";
        using var db = new SqliteConnection(cs);
        using var createTableCommand = new SqliteCommand(table, db);
		await db.OpenAsync();
        await createTableCommand.ExecuteNonQueryAsync();
		await db.CloseAsync();
    }

    static async Task CreateScheduledTable(string cs)
    {
        var table = @"
Create table if not exists
	ScheduledTask (
		Id uniqueidentifier primary key,
		Name nvarchar(200) not null,
		Period int not null,
		Interval int not null,
		StartDay int not null,
		StartHour int not null,
		StartMinute int not null,
		AssemblyQualifiedName nvarchar(200) not null,
		StartedCount int not null,
		Enabled bit not null,
		AllowMultipleInstance bit not null,
		AllowLocalMultipleInstances bit not null,
		NextRunningDate datetime not null,
		SerializedParameters nvarchar(5000) not null,
		Description nvarchar(1024) not null,
		ProcessMode int not null,
		CreationDate datetime not null,
		LastUpdate datetime not null
	)
";
        using var db = new SqliteConnection(cs);
        using var createTableCommand = new SqliteCommand(table, db);
        await db.OpenAsync();
        await createTableCommand.ExecuteNonQueryAsync();
        await db.CloseAsync();
    }

    static async Task CreateHostRegistrationTable(string cs)
    {
        var table = @"
Create table if not exists
	HostRegistration (
		Id uniqueidentifier primary key,
		UniqueKey varchar(400) not null,
		MachineName nvarchar(200) not null,
		HostName nvarchar(200) not null,
		State int not null,
		CreationDate datetime not null
	)
";
        using var db = new SqliteConnection(cs);
        using var createTableCommand = new SqliteCommand(table, db);
        await db.OpenAsync();
        await createTableCommand.ExecuteNonQueryAsync();
        await db.CloseAsync();
    }

    static async Task CreateProgressInfoTable(string cs)
    {
        var table = @"
Create table if not exists
	ProgressInfo (
		Id uniqueidentifier primary key,
		CreationDate datetime not null,
		TaskId uniqueidentifier not null,
		Type int not null,
		GroupName nvarchar(100) null,
		Subject nvarchar(500) null,
		Body nvarchar(1024) null,
		EntityName nvarchar(500) null,
		EntityId nvarchar(100) null,
		SerializedEntity nvarchar(5000) null,
		TotalCount int null,
		ProgressIndex int null
	)
";
        using var db = new SqliteConnection(cs);
        using var createTableCommand = new SqliteCommand(table, db);
        await db.OpenAsync();
        await createTableCommand.ExecuteNonQueryAsync();
        await db.CloseAsync();
    }

	static async Task TerminateAllRunningTasks(IServiceProvider serviceProvider)
	{
		var dbFactory = serviceProvider.GetRequiredService<IDbContextFactory<SqliteDbContext>>();
		using var db = await dbFactory.CreateDbContextAsync();

		var runningTaskList = await db.RunningTasks!.Where(i => i.TerminatedDate == null).ToListAsync();
		foreach (var runningTask in runningTaskList)
		{
			runningTask.TerminatedDate = DateTime.Now;
			runningTask.CanceledDate = DateTime.Now;
			runningTask.ErrorStack = "service restarted before task ending";
			db.Entry(runningTask).State = EntityState.Modified;
		}

		await db.SaveChangesAsync();
	}
}
