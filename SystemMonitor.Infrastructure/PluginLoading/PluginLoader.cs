namespace SystemMonitor.Infrastructure.PluginLoading;

using System.Reflection;
using SystemMonitor.Core.Interfaces;

/// <summary>
/// Simple loader to discover IMonitorPlugin implementations in assemblies.
/// For the architectural skeleton we keep this minimal. In the future, you can load plugin DLLs from a folder,
/// use AssemblyLoadContext for isolation, validate signatures, and sandbox if needed.
/// </summary>
public static class PluginLoader
{
    public static IEnumerable<Type> DiscoverPluginTypes(Assembly assembly)
    {
        return assembly.GetTypes().Where(t => !t.IsAbstract && typeof(IMonitorPlugin).IsAssignableFrom(t));
    }
}