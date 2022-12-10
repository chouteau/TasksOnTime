using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.DependencyInjection;

namespace DistributedTasksOnTime.Persistence
{
    public static class StartupExtensions
    {
        public static IServiceCollection AddPersistence(this IServiceCollection services, Action<PersistenceSettings> config)
        {
            var settings = new PersistenceSettings();
            config?.Invoke(settings);
            services.AddSingleton(settings);

            if (string.IsNullOrWhiteSpace(settings.StoreFolder))
            {
                throw new ArgumentException("StoreFolder is null or empty");
            }

            if (!System.IO.Directory.Exists(settings.StoreFolder)) 
            {
                System.IO.Directory.CreateDirectory(settings.StoreFolder);
            }

            services.AddTransient<IDbRepository, FileDbRepository>();

            return services;
        }
    }
}
