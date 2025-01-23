using WindFrostBot.SDK;

namespace ExampleP
{
    public class ExamplePlugin : Plugin
    {
        public override string PluginName()
        {
            return "TestPlugin";
        }

        public override string Version()
        {
            return "2.0";
        }

        public override string Author()
        {
            return "Cjx";
        }

        public override string Description()
        {
            return "ExamplePlugin";
        }

        public override void OnLoad()
        {
            CommandManager.InitGroupCommand(this, TestCommand, "测试指令", "pic");
            CommandManager.InitGroupCommand(this, TestCommand1, "测试指令", "test", "测试");
            CommandManager.InitGroupCommand(this, TestCommand2, "测试指令", "test2", "测试2");
        }
        public static void TestCommand2(CommandArgs args)
        {
            string message = $"消息:{args.Message}\nAuthor:{args.eventArgs.Author}\nGroupOpenId:{args.eventArgs.GroupOpenId}\nMsgId:{args.eventArgs.MsgId}\nContent:{args.eventArgs.Content}";
            args.Api.SendTextMessage(message);
        }
        public static void TestCommand1(CommandArgs args)
        {
            string message = args.Message;
            if(args.Parameters.Count > 0)
            {
                message += " " + string.Join(" ",args.Parameters);
            }
            args.Api.SendTextMessage(message);
            //args.Api.SendTextMessage(message);
        }
        public static void TestCommand(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                return;
            }
            args.Api.SendImage(MainSDK.QQClient.UploadFileToServer(args.Parameters[0]).Result);
        }
    }
}