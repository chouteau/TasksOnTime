using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TasksOnTime.Notification
{
    public static class ContextExtensions
    {
        public static void StartNotification(this ExecutionContext context, string groupName, string subject = null, string body = null)
        {
			NotificationService.Progress.Add(
				new NotificationItem()
				{
					TaskId = context.Id,
					Type = NotificationType.Start,
					GroupName = groupName,
					Subject = subject,
					Body = body,
				});
        }

        public static void WriteNotification(this ExecutionContext context, string groupName, string subject, string body = null)
        {
			NotificationService.Progress.Add(
					new NotificationItem()
					{
						TaskId = context.Id,
						Type = NotificationType.Write,
						GroupName = groupName,
						Subject = subject,
						Body = body,
					});
        }

        public static void StartProgressNotification(this ExecutionContext context, string groupName, string subject, int? totalCount = null)
        {
			NotificationService.Progress.Add(
					new NotificationItem()
					{
						TaskId = context.Id,
						Type = NotificationType.StartProgress,
						GroupName = groupName,
						Subject = subject,
						TotalCount = totalCount
					});
        }

        public static void StartContinuousProgressNotification(this ExecutionContext context, string groupName, string subject)
        {
			NotificationService.Progress.Add(
				new NotificationItem()
				{
					TaskId = context.Id,
					Type = NotificationType.StartContinuousProgress,
					GroupName = groupName,
					Subject = subject,
				});
        }

        public static void ProgressNotification(this ExecutionContext context, string groupName, string subject, int? index = null)
        {
			NotificationService.Progress.Add(
				new NotificationItem()
				{
					TaskId = context.Id,
					Type = NotificationType.Progress,
					GroupName = groupName,
					Subject = subject,
					Index = index,
				});
        }

        public static void EndProgressNotification(this ExecutionContext context, string groupName)
        {
			NotificationService.Progress.Add(
				new NotificationItem()
				{
					TaskId = context.Id,
					Type = NotificationType.EndProgress,
					GroupName = groupName,
				});
		}

		public static void EndContinuousProgressNotification(this ExecutionContext context, string groupName)
        {
			NotificationService.Progress.Add(
				new NotificationItem()
				{
					TaskId = context.Id,
					Type = NotificationType.EndContinuousProgress,
					GroupName = groupName,
				});
		}

		public static void EntityChangedNotification(this ExecutionContext context, string groupName, string entitName, string entityId, object entity = null)
        {
			NotificationService.Progress.Add(
				new NotificationItem()
				{
					TaskId = context.Id,
					Type = NotificationType.EntityChanged,
					GroupName = groupName,
					EntityName = entitName,
					EntityId = entityId,
					Entity = entity
				});
		}

		public static void FailedNotification(this ExecutionContext context, string groupName, string errorMessage)
        {
			NotificationService.Progress.Add(
				new NotificationItem()
				{
					TaskId = context.Id,
					Type = NotificationType.Failed,
					GroupName = groupName,
					Subject = errorMessage,
				});
		}

		public static void CancelNotification(this ExecutionContext context, string groupName)
        {
			NotificationService.Progress.Add(
				new NotificationItem()
				{
					TaskId = context.Id,
					Type = NotificationType.Cancel,
					GroupName = groupName,
				});
		}

		public static void CompletedNotification(this ExecutionContext context, string groupName, string subject = null, string body = null)
        {
			NotificationService.Progress.Add(
				new NotificationItem()
				{
					TaskId = context.Id,
					Type = NotificationType.Completed,
					GroupName = groupName,
					Subject = subject,
					Body = body
				});
		}
	}
}
