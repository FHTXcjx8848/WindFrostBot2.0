using WindFrostBot.SDK;

namespace ExamplePForWFBot
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
            CommandManager.InitGroupCommand(this, TestCommand, "测试指令", "测试");
            CommandManager.InitGroupCommand(this, TestCommand1, "测试指令", "test");
            CommandManager.InitGroupCommand(this, Upload, "测试指令", "upload");
        }
        public static void TestCommand1(CommandArgs args)
        {
            args.Api.SendTextMessage("测试成功！");
        }
        public static void TestCommand(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                return;
            }
            args.Api.SendImage(args.Parameters[0]);
        }
        public static void Upload(CommandArgs args)
        {
            if(args.Parameters.Count < 1)
            {
                return;
            }
            string url = MainSDK.QQClient.UploadFileToServer(args.Parameters[0]).Result;
            Message.Info("上传:" + url);
        }
    }
}