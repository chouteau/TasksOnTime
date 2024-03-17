using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedTasksOnTime.MsSqlPersistence;

public class MsSqlSettings
{
    public int DayCountOfRentention { get; set; } = 7;
    public string ConnectionString { get; set; } = null!;
}
