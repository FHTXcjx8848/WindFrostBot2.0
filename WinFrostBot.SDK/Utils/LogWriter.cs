using System;

namespace WindFrostBot.SDK
{
    public class Message
    {
        public static void LogErro(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{DateTime.Now}][{MainSDK.BotConfig.BotName}]" + message);
            LogWriter.Writer($"[{DateTime.Now}][{MainSDK.BotConfig.BotName}]" + message);
            Console.ResetColor();
        }
        public static void Erro(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[{DateTime.Now}][{MainSDK.BotConfig.BotName}]" + message);
            Console.ResetColor();
        }
        public static void BlueText(string message)
        {
            Console.ForegroundColor = ConsoleColor.Blue;
            Console.WriteLine($"[{DateTime.Now}][{MainSDK.BotConfig.BotName}]" + message);
            Console.ResetColor();
        }
        public static void Info(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[{DateTime.Now}][{MainSDK.BotConfig.BotName}]" + message);
            Console.ResetColor();
        }
        public static void LogInfo(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"[{DateTime.Now}][{MainSDK.BotConfig.BotName}]" + message);
            LogWriter.Writer($"[{DateTime.Now}][{MainSDK.BotConfig.BotName}]" + message);
            Console.ResetColor();
        }
        public static void LongInfo(string message)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message);
            Console.ResetColor();
        }
        public class LogWriter
        {
            //public static FileStream stream { get; set; }
            public static string Name { get; set; }
            public static readonly string LogPath = Path.Combine(AppContext.BaseDirectory, "Log");
            public static void StartLog()
            {
                if (MainSDK.BotConfig.EnableLog)
                {
                    if (!File.Exists(LogPath))
                    {
                        Directory.CreateDirectory(LogPath);
                    }
                    string name = $"{DateTime.Now.ToString().Replace("/", "-").Replace(":", "-").Replace(" ", "_")}.log";
                    var file = File.Create(LogPath + $"/{name}");
                    file.Close();
                    //stream = file;
                    Name = name;
                }
            }
            public static void Writer(string text)
            {
                if (MainSDK.BotConfig.EnableLog)
                {
                    string tp = LogPath + $"/{Name}";
                    string t = File.ReadAllText(tp);
                    t += text + "\n";
                    File.WriteAllText(tp, t);
                }
            }
        }
    }
}
