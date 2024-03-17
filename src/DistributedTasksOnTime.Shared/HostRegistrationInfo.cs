﻿namespace DistributedTasksOnTime;

public class HostRegistrationInfo
{
    public HostRegistrationInfo()
    {
        TaskList = new List<TaskRegistrationInfo>();
    }
    public Guid Id { get; set; } = Guid.NewGuid();
    public string MachineName { get; set; }
    public string HostName { get; set; }
    public string Key => $"{MachineName}||{HostName}";
    public HostRegistrationState State { get; set; }
    public IList<TaskRegistrationInfo> TaskList { get; set; }
}
