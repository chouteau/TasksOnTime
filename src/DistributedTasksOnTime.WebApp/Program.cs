using DistributedTasksOnTime.Orchestrator;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

using Ariane;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Host.AddDistributedTasksOnTimeOrchestrator();

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
