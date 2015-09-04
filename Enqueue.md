
# Multiple usage of TasksHost 

## Enqueue with monitoring
```c#
var id = Guid.NewGuiId()
TasksHost.Enqueue<MyTask>(id);

var history = TaskHost.GetHistory(id);

Console.Writeline(history.TerminatedDate);
```

## Start synchronized task
```c#
var mre = new ManualResetEvent(false);
TasksHost.Enqueue<MyTask>(completed: (dic) => mre.Set());
mre.WaitOne();
```

## Enqueue parameterized task
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

## Enqueue long task and cancel it
```c#
public class LongTask : ITask
{
    public void Execute(ExecutionContext context)
	{
        if (context.IsCancelRequested) // Break on start
        {
            break;
        }

		for (int i = 0; i < 10; i++)
		{
            if (context.IsCancelRequested) // Break on each loop
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

TasksHost.Enqueue<LongTask>(key,
	completed: (dic) =>
	{
		mre.Set();
	});

System.Threading.Thread.Sleep(2 * 1000);
TasksHost.Cancel(key);

mre.WaitOne();

var history = TasksHost.GetHistory(key);
```
