using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TasksOnTime.Notification
{
    public class NotificationService
    {
        private static ITaskProgress m_TaskProgress;

        public static Lazy<ITaskProgress> m_LazyProgressInstance = new Lazy<ITaskProgress>(() =>
        {
            return m_TaskProgress
                    ?? GlobalConfiguration.DependencyResolver.GetService(typeof(ITaskProgress)) as ITaskProgress
                    ?? new VoidTaskProgress();
        }, true);

        public static ITaskProgress Progress
        {
            get
            {
                return m_LazyProgressInstance.Value;
            }
        }

        public static void SetTaskProgress(ITaskProgress taskProgress)
        {
            m_TaskProgress = taskProgress;
        }
    }
}
