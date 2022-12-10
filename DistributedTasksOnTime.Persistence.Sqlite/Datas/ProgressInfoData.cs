using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedTasksOnTime.Persistence.Sqlite.Datas
{
    [Table("ProgressInfo")]
    internal class ProgressInfoData
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime CreationDate { get; set; } = DateTime.Now;
        public Guid TaskId { get; set; }
        public ProgressType Type { get; set; }
        public string? GroupName { get; set; }
        public string? Subject { get; set; }
        public string? Body { get; set; }
        public string? EntityName { get; set; }
        public string? EntityId { get; set; }
        public object? Entity { get; set; }
        public int? TotalCount { get; set; }
        public int? Index { get; set; }

    }
}
