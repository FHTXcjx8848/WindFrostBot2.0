using System;
using System.Drawing;

namespace WindFrostBot.SDK
{
    public class QCommand
    {
        public MessageEventArgs eventArgs { get; private set; }
        public int Type = 0;
        public QCommand(MessageEventArgs eventArgs)
        {
            this.eventArgs = eventArgs;
        }
        public void SendTextMessage(string message)
        {
            switch(Type)
            {
                case 0:
                    MainSDK.QQClient.SendMessage(message,eventArgs);
                    break;
                case 1:
                    break;
                default:
                    break;
            }
        }
        public void SendImage(string url)
        {
            switch (Type)
            {
                case 0:
                    MainSDK.QQClient.SendMedia(eventArgs, url);
                    break;
                case 1:
                    break;
            }
        }
    }
}
