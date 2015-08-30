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

// Enqueue task with delay (start after 5 seconds minimum)
TasksHost.Enqueue<MyTask>(delayInMillisecond: 5 * 1000);

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

### Enqueue parameterized task
```c#
public class ParameterizedTask : ITask
{
    public void Execute(ExecutionContext context)
    {
        var inputParameter = context.Parameters["input"];
        context.Parameters.Add("output", "test");
    }
}

var id = Guid.NewGuid();
var mre = new ManualResetEvent(false);
TasksHost.Enqueue<ParameterizedTask>(id,
    new Dictionary<string, object>()
    {
        { "input", "test" }
    }, completed: (dic) =>
    {
        var output = dic["output"];
        mre.Set();
    });

mre.WaitOne();
```

### Enqueue long task and cancel it
```c#
public class LongTask : ITask
{
    public void Execute(ExecutionContext context)
	{
		for (int i = 0; i < 10; i++)
		{
            if (context.IsCancelRequested)
            {
                break;
            }
            System.Diagnostics.Debug.Write(i);
			System.Threading.Thread.Sleep(1 * 1000);
        }
	}
}

var mre = new ManualResetEvent(false);
var key = Guid.NewGuid();

var parameter = new Dictionary<string, object>();
parameter.Add("count", 0);

TasksHost.Enqueue<LongTask>(key,
    parameter,
	completed: (dic) =>
	{
		mre.Set();
	});

System.Threading.Thread.Sleep(2 * 1000);
TasksHost.Cancel(key);

mre.WaitOne();

var history = TasksHost.GetHistory(key);
```

