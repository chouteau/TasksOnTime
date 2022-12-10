using System.IO.Pipes;

using AutoMapper;

using DistributedTasksOnTime.Persistence.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DistributedTasksOnTime.Persistence.Sqlite
{
    internal class SqlDbRepository : IDbRepository
    {
        private const string CACHE_REGISTRATIONS = "hostregistrations";

        private readonly IDbContextFactory<SqliteDbContext> _dbContextFactory;
        private readonly IMemoryCache _cache;
        private readonly IMapper _mapper;

        public SqlDbRepository(IDbContextFactory<SqliteDbContext> dbContextFactory,
            IMemoryCache cache,
            AutoMapper.IMapper mapper)
        {
            _dbContextFactory = dbContextFactory;
            _cache = cache;
            _mapper = mapper;
        }

        public void PersistHostRegistrationList(List<HostRegistrationInfo> list)
        {
            var existing = GetHostRegistrationList();
            var existingkeys = existing.Select(i => i.Key).ToList();
            var persistkeys = list.Select(i => i.Key).ToList();

            var db = _dbContextFactory.CreateDbContext();

            var insertList = existingkeys.Except(persistkeys);
            foreach (var key in insertList)
            {
                var ri = _mapper.Map<Datas.HostRegistrationData>(list.Single(i => i.Key == key));
                db.HostRegistrations!.Add(ri);
            }

            var updateList = existingkeys.Intersect(persistkeys);
            foreach (var key in updateList)
            {
                var data = db.HostRegistrations!.Single(i => i.Key.Equals(key));
                data = _mapper.Map(list.Single(i => i.Key == key),data);
                db.HostRegistrations!.Attach(data);
            }

            var deleteList = persistkeys.Except(existingkeys);
            foreach (var key in deleteList)
            {
                var ri = _mapper.Map<Datas.HostRegistrationData>(list.Single(i => i.Key == key));
                db.HostRegistrations!.Remove(ri);
            }

            var updatecount = db.SaveChanges();
            if (updatecount > 0)
            {
                _cache.Remove(CACHE_REGISTRATIONS);
            }
        }

        public List<HostRegistrationInfo> GetHostRegistrationList()
        {
            _cache.TryGetValue(CACHE_REGISTRATIONS, out List<HostRegistrationInfo>? list);
            if (list != null)
            {
                return list;
            }
            var db = _dbContextFactory.CreateDbContext();
            var datas = db.HostRegistrations!.ToList();
            list = _mapper.Map<List<HostRegistrationInfo>>(datas);
            if (list != null)
            {
                _cache.Set(CACHE_REGISTRATIONS, list);
            }
            return list!;
        }

        public void PersistScheduledTaskList(List<ScheduledTask> list)
        {
            // throw new NotImplementedException();
        }

        public List<ScheduledTask> GetScheduledTaskList()
        {
            return new List<ScheduledTask>();
        }

    }
}