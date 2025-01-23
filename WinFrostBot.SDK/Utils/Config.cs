using Org.BouncyCastle.Bcpg.OpenPgp;
using System;

namespace WindFrostBot.SDK
{
    public class Config
    {
        public string BotName = "WF";
        public bool EnableLog = true;
        public string AppID = "";
        public string Secret = "";
        public string FileServerUrl = "";
        public string SQLtype = "sqlite";
        public string MySqlHost = "";
        public string MySqlDbName = "";
        public string MySqlUsername = "";
        public string MySqlPassword = "";
    }
}
