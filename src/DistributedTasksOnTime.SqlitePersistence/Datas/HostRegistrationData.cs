﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DistributedTasksOnTime.SqlitePersistence.Datas
{
    [Table("HostRegistration")]
    internal class HostRegistrationData
    {
        [Key]
        public Guid Id { get; set; }
        public string UniqueKey { get; set; } = null!;
        public string MachineName { get; set; } = null!;
        public string HostName { get; set; } = null!;
        public HostRegistrationState State { get; set; }
        public DateTime CreationDate { get; set; } = DateTime.Now;
    }
}
