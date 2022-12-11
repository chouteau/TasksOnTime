using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace DistributedTasksOnTime.SqlitePersistence
{
	internal class DbCleaner : BackgroundService
	{
		private readonly IDbContextFactory<SqliteDbContext> _dbContextFactory;
		private readonly SqliteSettings _settings;
		private readonly ILogger _logger;

		public DbCleaner(IDbContextFactory<SqliteDbContext> dbContextFactory,
			SqliteSettings settings,
			ILogger<DbCleaner> logger)
		{
			_dbContextFactory = dbContextFactory;
			_settings = settings;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (true)
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
			var db = _dbContextFactory.CreateDbContext();

			var timeout = DateTime.Today.AddDays(_settings.DayCountOfRentention * -1);
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
}
