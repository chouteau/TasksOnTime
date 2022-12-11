using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using AutoMapper;

using DistributedTasksOnTime.SqlitePersistence.Datas;
using DistributedTasksOnTime.SqlitePersistence.Extensions;

namespace DistributedTasksOnTime.SqlitePersistence
{
    internal class Mapping : AutoMapper.Profile
    {
        public Mapping()
        {
            CreateMap<RunningTask, RunningTaskData>()
                .ReverseMap();

            CreateMap<ScheduledTask, ScheduledTaskData>()
                .ForMember(d => d.SerializedParameters, opt => opt.ResolveUsing(s => System.Text.Json.JsonSerializer.Serialize(s.Parameters)));

            CreateMap<ScheduledTaskData, ScheduledTask>()
                .ForMember(d => d.Parameters, opt => opt.ResolveUsing(s => string.IsNullOrWhiteSpace(s.SerializedParameters) ? new Dictionary<string, string>() : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(s.SerializedParameters)));

            CreateMap<ProgressInfo, ProgressInfoData>()
                .ForMember(d => d.ProgressIndex, opt => opt.MapFrom(s => s.Index))
                .ForMember(d => d.SerializedEntity, opt => opt.ResolveUsing(s => s.Entity == null ? null : System.Text.Json.JsonSerializer.Serialize(s.Entity)));

            CreateMap<ProgressInfoData, ProgressInfo>()
                .ForMember(d => d.Index, opt => opt.MapFrom(s => s.ProgressIndex))
                .ForMember(d => d.Entity, opt => opt.ResolveUsing(s => string.IsNullOrWhiteSpace(s.SerializedEntity) ? null : System.Text.Json.JsonSerializer.Deserialize<object>(s.SerializedEntity)));

            CreateMap<HostRegistrationInfo, HostRegistrationData>()
                .ForMember(i => i.UniqueKey, opt => opt.MapFrom(i => i.Key))
                .ReverseMap();
        }
    }
}
