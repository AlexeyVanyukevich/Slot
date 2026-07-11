using System.Reflection;

using Microsoft.Extensions.DependencyInjection;

namespace UBP.Endpoints;

internal static class ServiceDescriptorExtensions
{
    public static IEnumerable<ServiceDescriptor> GetTransientDescriptors<TService>()
    {
        var assembly = Assembly.GetEntryAssembly()
            ?? throw new InvalidOperationException("Unable to resolve the entry assembly.");

        return assembly.GetTypes()
            .Where(static type => type is { IsAbstract: false, IsInterface: false } && typeof(TService).IsAssignableFrom(type))
            .Select(static type => ServiceDescriptor.Transient(typeof(TService), type));
    }
}
