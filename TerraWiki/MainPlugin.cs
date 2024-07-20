using WindFrostBot.SDK;
using OpenQA.Selenium.Chrome;
using System.Drawing;

namespace TerraWikiPluin
{
    public class WikiPlugin : Plugin
    {
        public override string PluginName()
        {
            return "WikiPlugin";
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
            CommandManager.InitGroupCommand(this, WikiCommand, "wiki指令", "wiki","/wiki");
            CommandManager.InitGroupCommand(this, Help, "帮助菜单", "help", "/help");
            CommandManager.InitGroupCommand(this, About, "关于", "about", "/about");

        }
        public static void About(CommandArgs args)
        {
            args.Api.SendTextMessage("关于此机器人:\n开发者:3484721784\n然后就没了a.");
        }
        public static void Help(CommandArgs args)
        {
            args.Api.SendTextMessage("当前指令列表:\n/wiki <内容>,在泰拉wiki上查询内容并返回图像\n/about ,关于本机器人\n待开发...");
        }
        public static void WikiCommand(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                return;
            }
            if (args.Parameters.Count < 1)
            {
                args.Api.SendTextMessage($"参数不足!\n正确用法:wiki <内容>");
                return;
            }
            string item = args.Parameters[0];
            args.Api.SendTextMessage($"正在生成泰拉WIKI上关于[{item}]的页面！\n延迟会很长请耐心等待!");
            var op = new ChromeOptions();
            op.AddArguments("headless", "disable-gpu");
            using (var dir = new ChromeDriver(op))
            {
                var t2 = Task<byte[]>.Run(() =>
                {
                    dir.Navigate().GoToUrl($"https://terraria.wiki.gg/zh/wiki/Special:%E6%90%9C%E7%B4%A2?search={System.Web.HttpUtility.UrlEncode(item)}");
                    string w = dir.ExecuteScript("return document.body.scrollWidth").ToString();
                    string h = dir.ExecuteScript("return document.body.scrollHeight").ToString();
                    dir.Manage().Window.Size = new System.Drawing.Size(int.Parse(w), int.Parse(h));
                    return new MemoryStream(dir.GetScreenshot().AsByteArray);
                });
                Image img = Image.FromStream(t2.Result);
                args.Api.SendImage(img);
            }
        }
    }
}