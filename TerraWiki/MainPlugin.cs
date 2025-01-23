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
            CommandManager.InitGroupCommand(this, WikiCommand, "wiki指令", "wiki");
            //CommandManager.InitGroupCommand(this, About, "关于", "about");
        }
        public static void WikiCommand(CommandArgs args)
        {
            if (args.Parameters.Count < 1)
            {
                args.Api.SendTextMessage($"参数不足!\n正确用法:wiki <内容>");
                return;
            }
            string item = args.Parameters[0];
            args.Api.SendTextMessage($"正在生成泰拉WIKI上关于[{item}]的页面！\n延迟会很长请耐心等待!\n（有概率无法生成）");
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