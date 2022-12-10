using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

using DistributedTasksOnTime.Persistence.Extensions;

using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;

namespace DistributedTasksOnTime.Persistence.Sqlite;

public static class StartupExtensions
{
    public static IServiceCollection AddSqlitePersistence(this IServiceCollection services, Action<SqliteSettings> config)
    {
        var settings = new SqliteSettings();
        config(settings);
        services.AddSingleton(settings);
        services.AddAutoMapper(config =>
        {
            config.AddProfile<Mapping>();
        });
		services.ReplaceService<IDbRepository, SqlDbRepository>();
		if (!services.IsRegistered<IMemoryCache>())
		{
			services.AddMemoryCache();
		}
        return services;
    }

    public async static Task UseSqlitePersistence(this IServiceProvider serviceProvider)
    {
        var settings = serviceProvider.GetRequiredService<SqliteSettings>();
        await CreateRunningTable(settings.ConnectionString);
		await CreateScheduledTable(settings.ConnectionString);
		await CreateHostRegistrationTable(settings.ConnectionString);
		await CreateProgressInfoTable(settings.ConnectionString);
    }

    static async Task CreateRunningTable(string cs)
    {
        var table = @"
Create table if not exists
	RunningTask (
		Id uniqueidentifier primary key,
		TaskName varchar(200) not null,
		HostKey varchar(200) not null,
		CreationDate DateTime not null,
		EnqueuedDate DateTime null,
		RunningDate DateTime null,
		CancelingDate DateTime null,
		CanceledDate DateTime null,
		TerminatedDate DateTime null,
		FailedDate DateTime null,
		ErrorStack varchar(max),
		IsForced bit null
	)
";
        using var db = new SqliteConnection(cs);
        var createTableCommand = new SqliteCommand(table, db);
        await createTableCommand.ExecuteReaderAsync();
    }

    static async Task CreateScheduledTable(string cs)
    {
        var table = @"
Create table if not exists
	ScheduledTask (
		Id uniqueidentifier primary key,
		Name varchar(200) not null,
		ScheduledTaskTimePeriod int not null,
		Interval int not null,
		StartDay int not null,
		StartHour int not null,
		StartMinute int not null,
		AssemblyQualifiedName varchar(200) not null,
		StartedCount int not null,
		Enabled bit not null,
		AllowMultipleInstance bit not null,
		AllowLocalMultipleInstances bit not null,
		NextRunningDate datetime not null,
		Parameters varchar(200) not null,
		Description varchar(1024) not null,
		ProcessMode int not null,
		CreationDate datetime not null,
		LastUpdate datetime not null
	)
";
        using var db = new SqliteConnection(cs);
        var createTableCommand = new SqliteCommand(table, db);
        await createTableCommand.ExecuteReaderAsync();
    }

    static async Task CreateHostRegistrationTable(string cs)
    {
        var table = @"
Create table if not exists
	HostRegistration (
		Id uniqueidentifier primary key,
		MachineName varchar(200) not null,
		HostName varchar(200) not null,
		HostRegistrationState int not null,
		CreationDate datetime not null
	)
";
        using var db = new SqliteConnection(cs);
        var createTableCommand = new SqliteCommand(table, db);
        await createTableCommand.ExecuteReaderAsync();
    }

    static async Task CreateProgressInfoTable(string cs)
    {
        var table = @"
Create table if not exists
	ProgressInfo (
		Id uniqueidentifier primary key,
		CreationDate datetime not null,
		TaskId uniqueidentifier not null,
		ProgressType int not null,
		GroupName varchar(100) null,
		Subject varchar(500) null,
		Body varchar(1024) null,
		EntityName varchar(500) null,
		EntityId varchar(100) null,
		Entity varchar(max) null,
		TotalCount int null,
		Index int null
	)
";
        using var db = new SqliteConnection(cs);
        var createTableCommand = new SqliteCommand(table, db);
        await createTableCommand.ExecuteReaderAsync();
    }

}
