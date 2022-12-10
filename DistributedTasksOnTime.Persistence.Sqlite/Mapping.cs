using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutoMapper;

using DistributedTasksOnTime.Persistence.Models;
using DistributedTasksOnTime.Persistence.Sqlite.Datas;

namespace DistributedTasksOnTime.Persistence.Sqlite
{
    internal class Mapping : AutoMapper.Profile
    {
        public Mapping()
        {
            CreateMap<RunningTask, RunningTaskData>()
                .ReverseMap();

            CreateMap<ScheduledTask, ScheduledTaskData>()
                .ReverseMap();

            CreateMap<ProgressInfo, ProgressInfoData>()
                .ReverseMap();

            CreateMap<HostRegistrationInfo, HostRegistrationData>()
                .ReverseMap();
        }
    }
}
