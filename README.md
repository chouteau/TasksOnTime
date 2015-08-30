# TasksOnTime (3.0.5)
Simply enqueue (with delay) or schedule any c# task as you want and monitor it

**TasksOnTime** require only .Net 4.5 (minimum) no other dependance

**TasksOnTime** can be used in service , console, website or wpf/winforms applications

**TasksOnTime** only use standard *ThreadPool*

## Where can I get it ?

First, [install Nuget](http://docs.nuget.org/docs/start-here/installing-nuget) then, install [TasksOnTime](http://www.nuget.org/packages/tasksontime) from the package manager console.

> PM> Install-Package TasksOnTime 

## Usage :

Each *Task* type must implement *ITask* interface

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

// Simple enqueing
TasksHost.Enqueue<MyTask>();

// Enqueue with monitoring
var id = Guid.NewGuiId()
TasksHost.Enqueue<MyTask>(id);

var history = TaskHost.GetHistory(id);

Console.Writeline(history.TerminatedDate);

// Start synchronized task
var mre = new ManualResetEvent(false);
TasksHost.Enqueue<MyTask>(completed: (dic) => mre.Set());
mre.WaitAll();
```