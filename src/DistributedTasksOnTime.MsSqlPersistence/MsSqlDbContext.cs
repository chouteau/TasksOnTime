using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.EntityFrameworkCore;

namespace DistributedTasksOnTime.MsSqlPersistence;

internal class MsSqlDbContext : DbContext
{
	private readonly MsSqlSettings _msSqlSettings;

	public MsSqlDbContext(MsSqlSettings msSqlSettings)
    {
		_msSqlSettings = msSqlSettings;
	}

	protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
	{
		optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
		optionsBuilder.UseSqlServer(_msSqlSettings.ConnectionString);
	}

	public DbSet<Datas.HostRegistrationData> HostRegistrations { get; set; }
	public DbSet<Datas.ProgressInfoData> ProgressInfos { get; set; }
	public DbSet<Datas.RunningTaskData> RunningTasks { get; set; }
	public DbSet<Datas.ScheduledTaskData> ScheduledTasks { get; set; }
}
