using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DistributedTasksOnTime.MsSqlPersistence.Extensions;

public static class ServiceCollectionExtensions
{
    public static bool IsRegistered<S>(this IServiceCollection services) where S : class
    {
        var existing = services.Where(i => i.ServiceType == typeof(S)).ToList();
        return existing.Any();
    }
    public static void ReplaceService<I, T>(this IServiceCollection services) where T : class, I
    {
        var existing = services.Where(i => i.ServiceType == typeof(I)).ToList();
        if (existing.Count != 1)
        {
            throw new NotSupportedException($"try to replace not registered interface {typeof(I)}");
        }
        var remove = existing.Single();
        var descriptor = new ServiceDescriptor(typeof(I), typeof(T), existing.Single().Lifetime);
        services.Replace(descriptor);
        services.Remove(remove);
    }

    public static void RemoveService<I>(this IServiceCollection services) where I : class
    {
        var existing = services.Where(i => i.ServiceType == typeof(I)).ToList();
        if (existing.Count == 0)
        {
            throw new NotSupportedException($"try to replace not registered interface {typeof(I)}");
        }

        foreach (var existingService in existing)
        {
            services.Remove(existingService);
        }
    }
}
