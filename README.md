# TasksOnTime (3.0.5)

Require only *.Net 3.5* (minimum) with no other dependance
*TasksOnTime* Can be used in service , console, website or wpf/winforms applications
Use only standard ThreadPool

## Where can I get it ?

First, [install Nuget](http://docs.nuget.org/docs/start-here/installing-nuget) then, install [TasksOnTime](http://www.nuget.org/packages/tasksontime) from the package manager console.

> PM> Install-Package TasksOnTime 

## Usage with enqueue :

Each *Task* must implement *ITask* interface

```c#
public interface ITask 
{
    void Execute(ExecutionContext context);
}

```

### Enqueue simple task
```c#

public class MyTask : ITask
{
    public void Execute(ExecutionContext context)
    {
        System.Diagnostics.Debug.WriteLine("Task executed");
    }
}

// Simple task enqueing
TasksHost.Enqueue<MyTask>();

// Enqueue task with delay (start after 5 seconds minimum)
TasksHost.Enqueue<MyTask>(delayInMillisecond: 5 * 1000);
```

See others examples : [Examples(/enqueue.md)]

## Scheduled tasks :

for use scheduled tasks reference *"TasksOnTime.Scheduling"* in your project 
each task can scheduled by month, day, hour, minute or second with interval as you like
each scheduled task can be canceled, removed or forced , each task was executed in standard threadpool
Single instance of scheduler was possible by application

> PM> Install-Package TasksOnTime.Scheduling

Task implements ITask

```c#
var scheduledTask = TasksOnTime.Scheduler.CreateScheduledTask<MyTask>("MyTask")
											.EveryMinute();

TasksOnTime.Scheduler.Add(scheduledTask);
TasksOnTime.Scheduler.Start();

...

TasksOnTime.Scheduler.Stop();
```		

See others examples : [/scheduling.md]

