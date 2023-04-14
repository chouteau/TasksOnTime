using DistributedTasksOnTime.MsSqlPersistence.Extensions;

using EFScriptableMigration;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DistributedTasksOnTime.MsSqlPersistence;

public static class StartupExtensions
{
	public static IServiceCollection AddTasksOnTimeMsSqlPersistence(this IServiceCollection services, Action<MsSqlSettings> config)
	{
		var settings = new MsSqlSettings();
		config(settings);

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
		services.AddDbContextFactory<MsSqlDbContext>(lifetime: ServiceLifetime.Transient);
		services.AddHostedService<DbCleaner>();
		return services;
	}

	public async static Task UseTasksOnTimeMsSqlPersistence(this IServiceProvider serviceProvider)
	{
		var cfg = serviceProvider.GetRequiredService<MsSqlSettings>();
		var migration = new DbMigration()
		{
			ConnectionString = cfg.ConnectionString,
			SchemaName = "DistributedTasksOnTime",
			EmbededTypeReference = typeof(StartupExtensions)
		};

		await migration.Start();

		// await TerminateAllRunningTasks(serviceProvider);
	}

	static async Task TerminateAllRunningTasks(IServiceProvider serviceProvider)
	{
		var dbFactory = serviceProvider.GetRequiredService<IDbContextFactory<MsSqlDbContext>>();
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