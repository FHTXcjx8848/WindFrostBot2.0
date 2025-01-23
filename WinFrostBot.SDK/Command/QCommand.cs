using System;
using System.Drawing;

namespace WindFrostBot.SDK
{
    public class QCommand
    {
        public MessageEventArgs eventArgs { get; private set; }
        public int Type = 0;
        public int seq = 1;
        public QCommand(MessageEventArgs eventArgs)
        {
            this.eventArgs = eventArgs;
        }
        public void SendTextMessage(string message)
        {
            switch(Type)
            {
                case 0:
                    MainSDK.QQClient.SendMessage("\n" + message, eventArgs, seq);
                    seq++;
                    break;
                case 1:
                    break;
                default:
                    break;
            }
        }
        public void SendImage(Image img)
        {
            switch (Type)
            {
                case 0:
                    MainSDK.QQClient.SendMedia(eventArgs, img, seq);
                    seq++;
                    break;
                case 1:
                    break;
            }
        }
        public void SendImage(string url)
        {
            switch (Type)
            {
                case 0:
                    MainSDK.QQClient.SendMedia(eventArgs, url, seq);
                    seq++;
                    break;
                case 1:
                    break;
            }
        }
    }
}
