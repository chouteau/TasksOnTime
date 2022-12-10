using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace DistributedTasksOnTime.Persistence.Sqlite
{
    internal class SqliteDbContext : DbContext
    {
        private readonly SqliteSettings _sqliteSettings;

        public SqliteDbContext(SqliteSettings sqliteSettings)
        {
            _sqliteSettings = sqliteSettings;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.EnableServiceProviderCaching(true);
            optionsBuilder.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
            optionsBuilder.UseSqlite(_sqliteSettings.ConnectionString);
        }

        public DbSet<Datas.HostRegistrationData>? HostRegistrations { get; set; }
        public DbSet<Datas.ProgressInfoData>? ProgressInfos { get; set; }
        public DbSet<Datas.RunningTaskData>? RunningTasks { get; set; }
        public DbSet<Datas.ScheduledTaskData>? ScheduledTasks { get; set; }
    }
}
