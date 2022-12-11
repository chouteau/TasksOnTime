using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

namespace DistributedTasksOnTime.JsonFilePersistence;

public static class StartupExtensions
{
    public static IServiceCollection AddTasksOnTimeJsonFilePersistence(this IServiceCollection services, Action<PersistenceSettings> config)
    {
        var settings = new PersistenceSettings();
        config?.Invoke(settings);
        services.AddSingleton(settings);

		if (string.IsNullOrWhiteSpace(settings.StoreFolder))
        {
            throw new ArgumentException("StoreFolder is null or empty");
        }

		if (settings.StoreFolder.StartsWith(@".\"))
		{
			var currentFolder = System.IO.Path.GetDirectoryName(typeof(StartupExtensions).Assembly.Location)!;
			settings.StoreFolder = System.IO.Path.Combine(currentFolder, settings.StoreFolder);
		}
		if (!System.IO.Directory.Exists(settings.StoreFolder))
		{
			System.IO.Directory.CreateDirectory(settings.StoreFolder);
		}

        services.AddTransient<IDbRepository, FileDbRepository>();

        return services;
    }
}
