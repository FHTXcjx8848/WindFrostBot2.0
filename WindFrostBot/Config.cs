using System;
using JsonTool;
using Newtonsoft.Json;
using WindFrostBot.SDK;

namespace WindFrostBot
{
    public class ConfigWriter
    {
        public static JsonRw<Config> Config;
        public static Config GetConfig()
        {
            return Config.ConfigObj;
        }
        public static readonly string ConfigPath = Path.Combine(AppContext.BaseDirectory, "Config.json");
        public static void InitConfig()
        {
            var setting = new Config();
            var json = new JsonRw<Config>(ConfigPath, setting);
            json.OnError += OnError;
            json.OnCreating += OnCreating;
            Config = json;
            MainSDK.BotConfig = json.ConfigObj;
        }
        static void OnError(object sender, ErrorEventArgs e)
        {
            Message.LogErro("机器人框架配置读取错误:" + e.GetException().ToString());
        }
        static void OnCreating(object sender, CreatingEvent e)
        {
            Message.Info("自动生成机器人初始配置...");
        }
    }
}
