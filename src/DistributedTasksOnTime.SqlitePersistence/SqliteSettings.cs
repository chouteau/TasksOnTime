using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedTasksOnTime.SqlitePersistence
{
    public class SqliteSettings
    {
        public string StoreFolder { get; set; } = @".\TasksOnTime";

        public string DbFileName { get; set; } = "tasksontime.db";

        public int DayCountOfRentention { get; set; } = 7;

        internal string ConnectionString { get; set; } = null!;
    }
}
