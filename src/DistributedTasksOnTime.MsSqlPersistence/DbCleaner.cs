using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DistributedTasksOnTime.MsSqlPersistence;

internal class DbCleaner(
	IDbContextFactory<MsSqlDbContext> dbContextFactory,
	MsSqlSettings settings,
	ILogger<DbCleaner> logger
	)
	: BackgroundService
{
	private readonly ILogger _logger = logger;

	protected override async Task ExecuteAsync(CancellationToken stoppingToken)
	{
		while (!stoppingToken.IsCancellationRequested)
		{
			await Task.Delay(1000 * 60);

			try
			{
				await CleanOldDatas();
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, ex.Message);
			}
		}
	}

	private async Task CleanOldDatas()
	{
		var db = await dbContextFactory.CreateDbContextAsync();

		var timeout = DateTime.Today.AddDays(settings.DayCountOfRentention * -1);
		var runningQuery = from runningTask in db.RunningTasks
						   where runningTask.TerminatedDate != null
								   && runningTask.CreationDate < timeout
						   select runningTask;

		var runningList = await runningQuery.ToListAsync();
		foreach (var item in runningList)
		{
			db.RunningTasks!.Remove(item);
			db.Entry(item).State = EntityState.Deleted;
		}

		await db.SaveChangesAsync();

		var progressQuery = from progressInfo in db.ProgressInfos
							where progressInfo.CreationDate < timeout
							select progressInfo;

		var progressList = await progressQuery.ToListAsync();
		foreach (var item in progressList)
		{
			db.ProgressInfos!.Remove(item);
			db.Entry(item).State = EntityState.Deleted;
		}

		await db.SaveChangesAsync();
	}
}
