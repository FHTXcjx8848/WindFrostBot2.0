using System;
using System.Data;

namespace WindFrostBot.SDK
{
    public class MainSDK
    {
        public static Config BotConfig { get; set; }
        public static IDbConnection Db { get; set; }
        public static BotClient QQClient { get; set; }
    }
    public abstract class Plugin
    {
        public List<Command> Commands = new List<Command>();
        public abstract string PluginName();
        public abstract string Version();
        public abstract string Author();
        public abstract string Description();
        public abstract void OnLoad();
    }
}