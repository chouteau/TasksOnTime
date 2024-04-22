using DistributedTasksOnTime.BlazorComponent;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

using ArianeBus;
using DistributedTasksOnTime.Orchestrator;
using DistributedTasksOnTime.MsSqlPersistence;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

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
var cs = builder.Configuration.GetConnectionString("DistributedTasksOnTimeSqlServer");
builder.Services.AddTasksOnTimeMsSqlPersistence(config =>
{
	config.ConnectionString = cs;
});

builder.Services.AddArianeBus(config =>
{
	config.PrefixName = "dtot.";
	config.BusConnectionString = dtotSettings.AzureBusConnectionString;
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

await CreateDatabase(cs);

await app.Services.UseTasksOnTimeMsSqlPersistence();

await app.RunAsync();

static async Task CreateDatabase(string connectionString)
{
	var content = @"
use master

IF DB_ID('{DB_NAME}') IS NULL
BEGIN
	CREATE DATABASE {DB_NAME}
END
";

	var cs = new SqlConnectionStringBuilder(connectionString);
	var dbName = cs.InitialCatalog;

	content = content.Replace("{DB_NAME}", dbName);

	cs.InitialCatalog = "master";

	using (var cnx = new Microsoft.Data.SqlClient.SqlConnection(cs.ToString()))
	{
		using (var cmd = cnx.CreateCommand())
		{
			cmd.CommandText = content;
			cmd.CommandType = System.Data.CommandType.Text;
			try
			{
				cnx.Open();
				await cmd.ExecuteNonQueryAsync();
			}
			finally
			{
				cnx.Close();
			}
		}
	}
}
