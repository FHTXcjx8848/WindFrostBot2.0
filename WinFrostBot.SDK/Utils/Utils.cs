using System;
using System.Drawing;

namespace WindFrostBot.SDK
{
    public class Utils
    {
        public static void SendTextMessage(string message,long group,int type = 0)
        {
            switch (type)
            {
                case 0:
                    
                    break;
                default:
                    break;
            }
        }
        public void SendImage(Image img,long group,int type = 0)
        {
            switch (type)
            {
                case 0:

                    break;
            }
        }
        //判断群聊列表
        public static bool CanSend(long group)
        {
            bool cansend = false;
            foreach(var g in MainSDK.BotConfig.QGroups)
            {
                if (g == group)
                {
                    cansend = true;
                    break;
                }
            }
            return cansend;
        }
        //判断Admin
        public static bool IsAdmin(long account)
        {
            if (MainSDK.BotConfig.Admins.Contains(account) || MainSDK.BotConfig.Owners.Contains(account))
            {
                return true;
            }
            return false;
        }
        //判断Owner
        public static bool IsOwner(long account)
        {
            if (MainSDK.BotConfig.Owners.Contains(account))
            {
                return true;
            }
            return false;
        }
    }
}
