using System;
using System.Collections.Generic;
using System.Text;

namespace TasksOnTime
{
	public static class ProgressExtensions
	{
		public static void StartNotification(this ExecutionContext context, string groupName, string subject = null, string body = null)
		{
			context.Progress.Notify(
				new ProgressInfo()
				{
					TaskId = context.Id,
					Type = ProgressType.Start,
					GroupName = groupName,
					Subject = subject,
					Body = body,
				});
		}

		public static void WriteNotification(this ExecutionContext context, string groupName, string subject, string body = null)
		{
			context.Progress.Notify(
					new ProgressInfo()
					{
						TaskId = context.Id,
						Type = ProgressType.Write,
						GroupName = groupName,
						Subject = subject,
						Body = body,
					});
		}

		public static void StartProgressNotification(this ExecutionContext context, string groupName, string subject, int? totalCount = null)
		{
			context.Progress.Notify(
					new ProgressInfo()
					{
						TaskId = context.Id,
						Type = ProgressType.StartProgress,
						GroupName = groupName,
						Subject = subject,
						TotalCount = totalCount
					});
		}

		public static void StartContinuousProgressNotification(this ExecutionContext context, string groupName, string subject)
		{
			context.Progress.Notify(
				new ProgressInfo()
				{
					TaskId = context.Id,
					Type = ProgressType.StartContinuousProgress,
					GroupName = groupName,
					Subject = subject,
				});
		}

		public static void ProgressNotification(this ExecutionContext context, string groupName, string subject, int? index = null)
		{
			context.Progress.Notify(
				new ProgressInfo()
				{
					TaskId = context.Id,
					Type = ProgressType.Progress,
					GroupName = groupName,
					Subject = subject,
					Index = index,
				});
		}

		public static void EndProgressNotification(this ExecutionContext context, string groupName)
		{
			context.Progress.Notify(
				new ProgressInfo()
				{
					TaskId = context.Id,
					Type = ProgressType.EndProgress,
					GroupName = groupName,
				});
		}

		public static void EndContinuousProgressNotification(this ExecutionContext context, string groupName)
		{
			context.Progress.Notify(
				new ProgressInfo()
				{
					TaskId = context.Id,
					Type = ProgressType.EndContinuousProgress,
					GroupName = groupName,
				});
		}

		public static void EntityChangedNotification(this ExecutionContext context, string groupName, string entitName, string entityId, object entity = null)
		{
			context.Progress.Notify(
				new ProgressInfo()
				{
					TaskId = context.Id,
					Type = ProgressType.EntityChanged,
					GroupName = groupName,
					EntityName = entitName,
					EntityId = entityId,
					Entity = entity
				});
		}

		public static void FailedNotification(this ExecutionContext context, string groupName, string errorMessage)
		{
			context.Progress.Notify(
				new ProgressInfo()
				{
					TaskId = context.Id,
					Type = ProgressType.Failed,
					GroupName = groupName,
					Subject = errorMessage,
				});
		}

		public static void CancelNotification(this ExecutionContext context, string groupName)
		{
			context.Progress.Notify(
				new ProgressInfo()
				{
					TaskId = context.Id,
					Type = ProgressType.Cancel,
					GroupName = groupName,
				});
		}

		public static void CompletedNotification(this ExecutionContext context, string groupName, string subject = null, string body = null)
		{
			context.Progress.Notify(
				new ProgressInfo()
				{
					TaskId = context.Id,
					Type = ProgressType.Completed,
					GroupName = groupName,
					Subject = subject,
					Body = body
				});
		}

	}
}
