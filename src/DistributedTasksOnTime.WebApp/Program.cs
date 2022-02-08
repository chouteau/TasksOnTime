using DistributedTasksOnTime.BlazorComponent;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

using Ariane;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Host.AddDistributedTasksOnTimeBlazor();


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

var bus = app.Services.GetRequiredService<Ariane.IServiceBus>();
await bus.StartReadingAsync();

app.Run();
