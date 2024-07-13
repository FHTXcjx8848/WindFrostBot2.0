using System;
using WindFrostBot.SDK;
using Spectre.Console;
using System.Reflection;
using System.Runtime.Loader;

namespace WindFrostBot
{
    public class Program
    {
        static void Init()
        {
            AppDomain.CurrentDomain.AssemblyResolve += (sender, eventArgs) =>
            {
                string assemblyName = new AssemblyName(eventArgs.Name).Name;
                string path = Path.Combine(AppContext.BaseDirectory, "bin", $"{assemblyName}.dll");

                if (File.Exists(path))
                {
                    return AssemblyLoadContext.Default.LoadFromAssemblyPath(path);
                }

                return null;
            };
        }
        static void Main(string[] arg)
        {
            Init();
            AnsiConsole.Write(new FigletText("WindFrostBot").Color(Spectre.Console.Color.Aqua));
            ConfigWriter.InitConfig();
            Message.LogWriter.StartLog();
            DataBase.Init();
            Message.BlueText("WindFrostBot2.0 正在启动...");
            if (MainSDK.BotConfig.EnableLog)
            {
                Message.BlueText("日志功能已开启.");
            }
            if (!File.Exists(PluginLoader.PluginsDirectory))
            {
                Directory.CreateDirectory(PluginLoader.PluginsDirectory);
            }
            PluginLoader.LoadPlugins();
            StartBot();
            Message.BlueText("WindFrostBot2.0 启动成功!");
            for(; ;)
                Console.ReadLine();
        }
        public static  async void StartBot()
        {
            MainSDK.QQClient = new BotClient(MainSDK.BotConfig.AppID, MainSDK.BotConfig.Secret);
            CommandManager.InitCommandToBot();
        }
    }
}