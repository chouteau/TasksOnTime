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

        internal string ConnectionString
        {
            get
            {
                if (StoreFolder.StartsWith(@".\"))
                {
                    var currentFolder = System.IO.Path.GetDirectoryName(this.GetType().Assembly.Location)!;
                    StoreFolder = System.IO.Path.Combine(currentFolder, StoreFolder);
                }
                if (!System.IO.Directory.Exists(StoreFolder))
                {
                    System.IO.Directory.CreateDirectory(StoreFolder);
                }
                var dbFileName = System.IO.Path.Combine(StoreFolder, DbFileName);
                var cs = $"FileName={dbFileName}";
                return cs;
            }
        }
    }
}
