using DistributedTasksOnTime.BlazorComponent;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

using Ariane;
using DistributedTasksOnTime.Orchestrator;
using DistributedTasksOnTime.SqlitePersistence;

var currentFolder = System.IO.Path.GetDirectoryName(typeof(Program).Assembly.Location);

var builder = WebApplication.CreateBuilder(args);

var localConfig = System.IO.Path.Combine(currentFolder, "localconfig", "appsettings.json");
builder.Configuration.SetBasePath(currentFolder)
		.AddJsonFile("appSettings.json", true, false)
		.AddJsonFile($"appSettings.{builder.Environment.EnvironmentName}.json", true, false)
        .AddJsonFile(localConfig, true, false)
        .AddEnvironmentVariables();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

var section = builder.Configuration.GetSection("DistributedTasksOnTime");
var dtotSettings = new DistributedTasksOnTimeServerSettings();
dtotSettings.ScheduledTaskListBlazorPage = "/";
section.Bind(dtotSettings);

builder.AddDistributedTasksOnTimeBlazor(dtotSettings);
builder.Services.AddTasksOnTimeSqlitePersistence(config =>
{
	config.ConnectionString = builder.Configuration.GetConnectionString("DistributedTasksOnTimeSqlite");
});

builder.Services.ConfigureArianeAzure();
builder.Services.ConfigureAriane(register =>
{
	register.SetupArianeRegisterDistributedTasksOnTimeOrchestrator(dtotSettings);
}, s =>
{
	s.DefaultAzureConnectionString = dtotSettings.AzureBusConnectionString;
});

if (System.Environment.UserInteractive
	&& !builder.Environment.IsProduction())
{
	builder.Logging.SetMinimumLevel(LogLevel.Trace);
	builder.Logging.AddConsole();
	builder.Logging.AddDebug();
}
else
{
	builder.Logging.SetMinimumLevel(LogLevel.Information);
}
builder.Logging.AddFilter((p, c, l) =>
{
	if ((c.StartsWith("Microsoft")
		&& l <= LogLevel.Information))
	{
		return false;
	}
	return true;
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

await app.Services.UseTasksOnTimeSqlitePersistence();

var bus = app.Services.GetRequiredService<Ariane.IServiceBus>();
await bus.StartReadingAsync();

app.Run();
