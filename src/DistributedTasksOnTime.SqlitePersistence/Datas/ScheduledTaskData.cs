using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedTasksOnTime.SqlitePersistence.Datas
{
    [Table("ScheduledTask")]
    internal class ScheduledTaskData
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public ScheduledTaskTimePeriod Period { get; set; }
        public int Interval { get; set; }
        public int StartDay { get; set; }
        public int StartHour { get; set; }
        public int StartMinute { get; set; }
        public string AssemblyQualifiedName { get; set; } = null!;
        public int StartedCount { get; set; }
        public bool Enabled { get; set; }
        public bool AllowMultipleInstance { get; set; }
        public bool AllowLocalMultipleInstances { get; set; }
        public DateTime NextRunningDate { get; set; }
        public string? SerializedParameters { get; set; }
        public string? Description { get; set; }
        public ProcessMode ProcessMode { get; set; }
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public DateTime LastUpdate { get; set; } = DateTime.Now;
        [NotMapped]
        public bool FromEditor { get; set; } = false;
    }
}
