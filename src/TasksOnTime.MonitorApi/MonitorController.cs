using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Http;

using TasksOnTime.Scheduling;

namespace TasksOnTime.MonitorApi
{
	[RoutePrefix("api/tasksontimemonitor")]
	public class MonitorController : ApiController
    {
		public MonitorController()
		{

		}

		[System.Web.Http.HttpGet]
		[System.Web.Http.HttpPost]
		[Route("ping")]
		public DateTime Ping()
		{
			return DateTime.Now;
		}

		[System.Web.Http.HttpGet]
		[Route("list")]
		public IQueryable<ScheduledTaskViewModel> GetList()
		{
			var taskList =  Scheduler.GetList();
			var result = new List<ScheduledTaskViewModel>();
			foreach (var item in taskList)
			{
				var task = new ScheduledTaskViewModel()
				{
					Name = item.Name,
					CreationDate = item.CreationDate,
					NextRunningDate = item.NextRunningDate,
					Period = item.Period,
					StartedCount = item.StartedCount,
					Enabled = item.Enabled,
					Exception = item.Exception,
				};

				var historyList = TasksHost.GetHistory(item.Name);
				var historyListVM = new List<TaskHistoryViewModel>();
				foreach (var historyItem in historyList)
				{
					var historyVM = new TaskHistoryViewModel();
					historyVM.CanceledDate = historyItem.CanceledDate;
					historyVM.CreationDate = historyItem.CreationDate;
					historyVM.Exception = historyItem.Exception;
					historyVM.Id = historyItem.Id;
					historyVM.Name = historyItem.Name;
					historyVM.Parameters = historyItem.Parameters;
					historyVM.StartedDate = historyItem.StartedDate;
					historyVM.TerminatedDate = historyItem.TerminatedDate;
					historyListVM.Add(historyVM);
                }
				task.HistoryList = historyListVM;
				result.Add(task);
			}
			return result.AsQueryable();
		}

		[System.Web.Http.HttpGet]
		[Route("force")]
		public object ForceTask(string id)
		{
			Scheduler.ForceTask(id);
			return new
			{
				message = "Task forced"
			};
		}

	}
}
