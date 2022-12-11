using System.Collections.Generic;
using System.IO.Pipes;
using System.Threading.Tasks;

using AutoMapper;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Caching.Memory;

namespace DistributedTasksOnTime.SqlitePersistence
{
    internal class SqlDbRepository : IDbRepository
    {
        private const string CACHE_REGISTRATIONS = "hostregistrations";
        private const string CACHE_SCHEDULEDTASKS = "scheduledtasks";

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

        public void SaveHostRegistration(HostRegistrationInfo hostRegistrationInfo)
        {
            var db = _dbContextFactory.CreateDbContext();

            var existing = db.HostRegistrations!.SingleOrDefault(i => i.UniqueKey.Equals(hostRegistrationInfo.Key));
            if (existing == null)
            {
                var ri = _mapper.Map<Datas.HostRegistrationData>(hostRegistrationInfo);
                db.HostRegistrations!.Add(ri);
            }
            else
            {
                existing = _mapper.Map(hostRegistrationInfo, existing);
                db.HostRegistrations!.Attach(existing!);
                db.Entry(existing!).State = EntityState.Modified;
            }

            // TODO: Supprimer les taches planifiées qui existent mais ne sont plus référencées

            var updatecount = db.SaveChanges();
            if (updatecount > 0)
            {
                _cache.Remove(CACHE_REGISTRATIONS);
            }
        }

        public void DeleteHostRegistration(string key)
        {
            var db = _dbContextFactory.CreateDbContext();
            var existing = db.HostRegistrations!.SingleOrDefault(i => i.UniqueKey.Equals(key));
            if (existing != null)
            {
                db.Remove(existing!);
                db.Entry(existing).State = EntityState.Deleted;
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

        public void SaveScheduledTask(ScheduledTask scheduledTask)
        {
            var db = _dbContextFactory.CreateDbContext();

            var name = scheduledTask.Name;
            var existing = db.ScheduledTasks!.SingleOrDefault(i => i.Name.Equals(name));
            if (existing == null)
            {
                var st = _mapper.Map<Datas.ScheduledTaskData>(scheduledTask);
                db.ScheduledTasks!.Add(st);
            }
            else
            {
                existing = _mapper.Map(scheduledTask, existing);
                existing.LastUpdate = DateTime.Now;
                db.ScheduledTasks!.Attach(existing);
                db.Entry(existing).State = EntityState.Modified;
            }

            var updatecount = db.SaveChanges();
            if (updatecount > 0)
            {
                _cache.Remove(CACHE_SCHEDULEDTASKS);
            }
        }

        public void DeleteScheduledTask(string taskName)
        {
            var db = _dbContextFactory.CreateDbContext();
            var existing = db.ScheduledTasks!.SingleOrDefault(i => i.Name.Equals(taskName));
            if (existing != null)
            {
                db.Remove(existing!);
                db.Entry(existing).State = EntityState.Deleted;
            }
            var updatecount = db.SaveChanges();
            if (updatecount > 0)
            {
                _cache.Remove(CACHE_SCHEDULEDTASKS);
            }
        }

        public List<ScheduledTask> GetScheduledTaskList()
        {
            _cache.TryGetValue(CACHE_SCHEDULEDTASKS, out List<ScheduledTask>? list);
            if (list != null)
            {
                return list;
            }
            var db = _dbContextFactory.CreateDbContext();
            var datas = db.ScheduledTasks!.ToList();
            list = _mapper.Map<List<ScheduledTask>>(datas);
            if (list != null)
            {
                _cache.Set(CACHE_SCHEDULEDTASKS, list);
            }
            return list!;
        }

        public List<RunningTask> GetRunningTaskList(bool withProgress = false)
        {
            var db = _dbContextFactory.CreateDbContext();
            var data = db.RunningTasks!.ToList();
            var result = _mapper.Map<List<RunningTask>>(data);
            if (withProgress)
            {
                var taskIdList = result.Select(i => i.Id).Distinct().ToList();
                var progressList = db.ProgressInfos!.Where(i => taskIdList.Contains(i.TaskId)).OrderBy(i => i.CreationDate).ToList();
                foreach (var progressData in progressList)
                {
                    var runningTask = result.Single(i => i.Id == progressData.TaskId);
                    var progress = _mapper.Map<ProgressInfo>(progressData);
                    runningTask.ProgressLogs.Add(progress);
                }
            }
            return result;
        }

        public void SaveRunningTask(RunningTask task)
        {
            var db = _dbContextFactory.CreateDbContext();
            var existing = db.RunningTasks!.SingleOrDefault(i => i.Id == task.Id);
            if (existing == null)
            {
                var data = _mapper.Map<Datas.RunningTaskData>(task);
                db.RunningTasks!.Add(data);
            }
            else
            {
                existing = _mapper.Map(task, existing);
                db.RunningTasks!.Attach(existing);
                db.Entry(existing).State = EntityState.Modified;
            }
            db.SaveChanges();
        }

        public void ResetRunningTasks()
        {
            var db = _dbContextFactory.CreateDbContext();
            FormattableString script = $"Delete from RunningTask";
            db.Database.ExecuteSql(script);
            db.SaveChanges();
        }

        public void SaveProgressInfo(ProgressInfo progressInfo)
        {
            var db = _dbContextFactory.CreateDbContext();
            var data = _mapper.Map<Datas.ProgressInfoData>(progressInfo);
            db.ProgressInfos!.Add(data);
            db.SaveChanges();
        }

        public void PersistAll()
        {
            // Do nothing
        }
    }
}