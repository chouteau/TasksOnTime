using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedTasksOnTime.SqlitePersistence.Datas
{
    [Table("RunningTask")]
    internal class RunningTaskData
    {
        [Key]
        public Guid Id { get; set; }
        public string TaskName { get; set; } = null!;
        public string? HostKey { get; set; } = null!;
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public DateTime? EnqueuedDate { get; set; }
        public DateTime? RunningDate { get; set; }
        public DateTime? CancelingDate { get; set; }
        public DateTime? CanceledDate { get; set; }
        public DateTime? TerminatedDate { get; set; }
        public DateTime? FailedDate { get; set; }
        public string? ErrorStack { get; set; }
        public bool IsForced { get; set; }

    }
}
