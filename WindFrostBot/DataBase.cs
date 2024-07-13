using System;
using MySql.Data.MySqlClient;
using Microsoft.Data.Sqlite;
using WindFrostBot.SDK;

namespace WindFrostBot
{
    public class DataBase
    {
        public static readonly string Path = AppContext.BaseDirectory;
        public static void Init()
        {
            #region 数据库部分
            string path = $"{Path}/Bot.sqlite";
            if (!File.Exists(path))
            {
                File.Create(path);
            }
            if (MainSDK.BotConfig.SQLtype.Equals("mysql", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrWhiteSpace(MainSDK.BotConfig.MySqlHost) || string.IsNullOrWhiteSpace(MainSDK.BotConfig.MySqlDbName))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Message.LogErro("MySQL配置异常！禁用插件CSFTBot...");
                    Console.ResetColor();
                    return;
                }
                string[] host = MainSDK.BotConfig.MySqlHost.Split(':');
                MainSDK.Db = new MySqlConnection
                {
                    ConnectionString = String.Format("Server={0}; Port={1}; Database={2}; Uid={3}; Pwd={4};",
                        host[0],
                        host.Length == 1 ? "3306" : host[1],
                        MainSDK.BotConfig.MySqlDbName,
                        MainSDK.BotConfig.MySqlUsername,
                        MainSDK.BotConfig.MySqlPassword)
                };
            }
            else if (MainSDK.BotConfig.SQLtype.Equals("sqlite", StringComparison.OrdinalIgnoreCase))
            {
                MainSDK.Db = new SqliteConnection(string.Format("Data Source={0}", path));
            }
            else
            {
                throw new InvalidOperationException("错误的数据库类型!");
            }
            #endregion
        }
    }
}
