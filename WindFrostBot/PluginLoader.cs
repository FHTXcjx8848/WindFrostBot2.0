using System.Reflection;
using WindFrostBot.SDK;
namespace WindFrostBot;

public static class PluginLoader
{
    public static readonly string PluginsDirectory = Path.Combine(AppContext.BaseDirectory, "Plugins");
    private static readonly Dictionary<string, Assembly> LoadedAssemblies = new();
    public static readonly List<Plugin> Plugins = new();
    public static void LoadPlugins()
    {
        void CreateAndAddPluginInstances(Assembly assembly)
        {
            foreach (var type in assembly.GetExportedTypes())
            {
                if (!type.IsSubclassOf(typeof(Plugin)) || !type.IsPublic || type.IsAbstract)
                    continue;

                Plugin pluginInstance;
                try
                {
                    pluginInstance = (Activator.CreateInstance(type) as Plugin)!;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"加载插件 \"{type.FullName}\" 时出错.", ex);
                }

                Plugins.Add(pluginInstance);
            }
        }
        CreateAndAddPluginInstances(Assembly.GetExecutingAssembly());
        var pluginPaths = Directory.GetFiles(PluginsDirectory, "*.dll");
        foreach (var pluginPath in pluginPaths)
            try
            {
                if (LoadedAssemblies.TryGetValue(pluginPath, out _))
                    continue;

                Assembly assembly;
                try
                {
                    byte[] assemblyData = File.ReadAllBytes(pluginPath);
                    assembly = Assembly.Load(assemblyData);
                }
                catch (BadImageFormatException)
                {
                    continue;
                }
                LoadedAssemblies.Add(pluginPath, assembly);
                CreateAndAddPluginInstances(assembly);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"加载 \"{pluginPath}\" 失败.", ex);
            }
        foreach (var p in Plugins)
        {
            try
            {
                p.OnLoad();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"插件 \"{p.PluginName()}\" 在初始化时出错.", ex);
            }
            Message.LogInfo($"{p.PluginName()} Version:{p.Version()}(by {p.Author()}) 成功加载!");
        }
        Message.LogInfo($"总共加载 {Plugins.Count} 个插件.");
    }
}