using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using CSR;
using System.Web.Script.Serialization;
using System.Threading.Tasks;
using ManageWindow;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using System.Linq.Expressions;
using PFShop;
using static PFShop.FormINFO;
using System.Runtime.CompilerServices;
//using PFShop;

namespace CSRDemo
{
    public class Program
    {
        private static MCCSAPI api = null;
        public static void WriteLine(object content)
        {
            Console.WriteLine(content);
        }
        //public static Task<T> StartSTATask<T>(Func<T> func)
        //{
        //    var tcs = new TaskCompletionSource<T>();
        //    var thread = new Thread(() =>
        //    {
        //        try
        //        {
        //            tcs.SetResult(func());
        //        }
        //        catch (Exception e)
        //        {
        //            tcs.SetException(e);
        //        }
        //    });
        //    thread.SetApartmentState(ApartmentState.STA);
        //    thread.Start();
        //    return tcs.Task;
        //} 
        //private static Task windowTask = null;
        private static Thread windowthread = null;
        private static ManualResetEvent manualResetEvent = null;
        private static bool windowOpened = false;
        private static void ShowSettingWindow()
        {
            try
            {
                if (windowthread == null)
                {
                    windowthread = new Thread(new ThreadStart(() =>
                    {
                        while (true)
                        {
                            try
                            {
                                windowOpened = true;
                                new MainWindow().ShowDialog();
                                GC.Collect();
                                manualResetEvent = new ManualResetEvent(false);
                                windowOpened = false;
                                manualResetEvent.WaitOne();
                            }
                            catch (Exception err) { WriteLine("窗体执行过程中发生错误\n信息" + err.ToString()); }
                        }
                    }));
                    windowthread.SetApartmentState(ApartmentState.STA);
                    windowthread.Start();
                }
                else
                { if (windowOpened) WriteLine("窗体已经打开"); else manualResetEvent.Set(); }
            }
            catch (Exception err)
            {
#if DEBUG
                WriteLine(err.ToString());
#endif
            }
        }

        #region API方法补充
        public static string GetUUID(string name) => JArray.Parse(api.getOnLinePlayers()).First(l => l.Value<string>("playername") == name).Value<string>("uuid");
        public static void Feedback(string name, string text) => api.runcmd($"tellraw \"{name}\" {{\"rawtext\":[{{\"text\":\"§e§l[PFSHOP]§r§b{StringToUnicode(text)}\"}}]}}");
        public static void ExecuteCMD(string name, string cmd) => api.runcmd($"execute \"{name}\" ~~~ {cmd}");
        public static string StringToUnicode(string s)//字符串转UNICODE代码
        {
            char[] charbuffers = s.ToCharArray();
            byte[] buffer;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < charbuffers.Length; i++)
            {
                buffer = Encoding.Unicode.GetBytes(charbuffers[i].ToString());
                sb.Append(String.Format("\\u{0:X2}{1:X2}", buffer[1], buffer[0]));
            }
            return sb.ToString();
        }
        #endregion
        #region 表单方法
        #region struct

        #endregion
        public static void LoadFormTip(string playername)
        {
            ExecuteCMD(playername, "title @s times 0 20 10");
            ExecuteCMD(playername, "titleraw @s title {\"rawtext\":[{\"text\":\"\n\n\n\n\"}]}");
            ExecuteCMD(playername, "titleraw @s subtitle {\"rawtext\":[{\"text\":\"§a   Loading    §a\"}]}");
            _ = Task.Run(() =>
            {
                Thread.Sleep(250);
                ExecuteCMD(playername, "titleraw @s subtitle {\"rawtext\":[{\"text\":\"§a   Loading..   §a\"}]}");
                Thread.Sleep(250);
                ExecuteCMD(playername, "titleraw @s subtitle {\"rawtext\":[{\"text\":\"§a   Loading....  §a\"}]}");
                Thread.Sleep(250);
                ExecuteCMD(playername, "titleraw @s subtitle {\"rawtext\":[{\"text\":\"§a   Loading...... §a\"}]}");
                Thread.Sleep(250);
                ExecuteCMD(playername, "titleraw @s clear");
            });
        }
        public static List<FormINFO> FormQueue = new List<FormINFO>();
        public static void SendForm(FormINFO form)
        {
            try
            {
                LoadFormTip(form.playername);
                Task.Run(() =>
                {
                    try
                    {
                        Thread.Sleep(250);
                        switch (form.Type)
                        {
                            case FormType.Simple:
                                //WriteLine(form.buttons.ToString()); 
                                form.id = api.sendSimpleForm(form.playeruuid, form.title, form.content.ToString(), form.buttons.ToString());
                                break;
                            case FormType.SimpleIMG:
                                JArray buttons = new JArray();
                                foreach (JObject btsou in form.buttons)
                                {
                                    JObject button = new JObject() { new JProperty("text", btsou.Value<string>("text")) };
                                    if (btsou.ContainsKey("image"))
                                    {
                                        button.Add("image", new JObject()
                                    {
                                        new JProperty("type",Regex.IsMatch(btsou.Value<string>("image"),@"[a-zA-Z0-9][-a-zA-Z0-9]{0,62}(\.[a-zA-Z0-9][-a-zA-Z0-9]{0,62})+\.?")   ? "url" : "path"),
                                        new JProperty("data",btsou.Value<string>("image"))
                                    });
                                    }
                                    buttons.Add(button);
                                }
                                var content = new JObject() {
                                new JProperty("content",form.content.ToString()),
                                new JProperty("type","form"),
                                new JProperty("title",form.title),
                                new JProperty("buttons",buttons),
                            };
                                form.id = api.sendCustomForm(form.playeruuid, content.ToString());
                                break;
                            case FormType.Custom:
                                form.id = api.sendCustomForm(form.playeruuid, new JObject { new JProperty("type", "custom_form"), new JProperty("title", form.title), new JProperty("content", form.content) }.ToString());
                                break;
                            case FormType.Model:
                                form.id = api.sendModalForm(form.playeruuid, form.title, form.content.ToString(), form.buttons[0].ToString(), form.buttons[1].ToString());
                                break;
                            default:
                                break;
                        }
                        if (form.id != 0) FormQueue.Add(form); else WriteLine("表单发送失败!");
                    }
                    catch (Exception err) { WriteLine("表单发送失败!\n" + err.ToString()); }
                });
            }
            catch (Exception err) { WriteLine("表单发送失败!\n" + err.ToString()); }
        }
        #region 序列化信息
        public static JObject GetButton(string text, string image) => new JObject { new JProperty("text", text), new JProperty("image", image), };
        public static JObject GetButtonRaw(string text) => new JObject { new JProperty("text", text) };
        public static Func<JArray, JArray> GetSell = (items) =>
       {
           JArray get = new JArray() { GetButtonRaw("<==返回上级菜单") };
           foreach (JObject item in items)
           {
               //WriteLine(GetButton($"#{item.Value<string>("order")}§l{item.Value<string>("name")}\n{(item.Value<decimal>("price") % 1 > 0 ? item["price"].ToString() + '+' : Math.Round(item.Value<decimal>("price")).ToString())}像素币/个"
               //    , item.Value<string>("image")));
               if (item.ContainsKey("type")) get.Add(GetButtonRaw($"§l{item.Value<string>("type")}\n§o子菜单==>"));
               else
               {
                   decimal price = 0; try { price = item.Value<decimal>("price"); } catch (Exception) { }
                   get.Add(GetButton($"#{item.Value<string>("order")}§l{item.Value<string>("name")}\n{(price % 1 > 0 ? price.ToString() + '+' : Math.Round(price).ToString())}像素币/个"
                 , item.Value<string>("image")));
               }
           }
           //WriteLine(get.ToString(Newtonsoft.Json.Formatting.None));
           return get;
       };
        public static Func<JArray, JArray> GetRecycle = (items) =>
        {
            JArray get = new JArray() { GetButtonRaw("<==返回上级菜单") };
            foreach (JObject item in items)
            {
                if (item.ContainsKey("type")) get.Add(GetButtonRaw($"§l{item.Value<string>("type")}\n§o子菜单==>"));
                else
                {
                    decimal award = 0; try { award = item.Value<decimal>("award"); } catch (Exception) { }
                    get.Add(GetButton($"#{item.Value<string>("order")}§l{item.Value<string>("name")}\n{(award % 1 > 0 ? award.ToString() + '-' : Math.Round(award).ToString())}像素币/个"
                  , item.Value<string>("image")));
                }
            }
            return get;
        };
        #endregion
        #region 方便调用的方法
        public static void SendMain(string playername)
        {
            SendForm(new FormINFO(playername, FormType.Simple, FormTag.Main) { title = "商店", content = "来干点什么？", buttons = new JArray { "出售商店", "回收商店", "偏好设置" } });
        }
        #endregion
        #endregion
        #region 配置
        //商店信息
        public static JObject shopdata = new JObject();
        public static string shopdataPath = Path.GetFullPath("plugins\\pfshop\\shopdata.json");
        public static void SaveShopdata() => File.WriteAllText(shopdataPath, shopdata.ToString());
        //偏好设定
        public static JObject preference = new JObject();
        public static string preferencePath = Path.GetFullPath("plugins\\pfshop\\preference.json");
        public static void SavePreference() => File.WriteAllText(preferencePath, preference.ToString());
        #endregion
        public static void init(MCCSAPI base_api)
        {
            _ = Task.Run(() =>
            {
                Thread.Sleep(10000);
                api.runcmd("scoreboard objectives add money dummy §b像素币");
            });
            try
            {
                #region 加载
                api = base_api;
                Console.OutputEncoding = Encoding.UTF8;
                WriteLine("████████████████████" +
                    "\n正在裝載PFSHOP" +
                    "\n作者       \tgxh2004" +
                    "\n版本信息\tv" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() +
                    "\n适用于bds1.16.1(CSR1.16.1v4)" +
                    "\n如版本不同可能存在问题" +
                    //"\n發佈日期\t" + System.Reflection.Assembly.GetExecutingAssembly().GetName()..ToString() +
                    "\n基於C#+WPF窗體" +
                    "\n当前CSRunnerAPI版本:" + api.VERSION +
                    "\n控制台输入\"pf\"即可打开配置窗体(未完工)" +
                    "\n████████████████████");
                #region 读取配置
                //商店信息
                try
                {
                    if (!Directory.Exists(Path.GetDirectoryName(shopdataPath))) Directory.CreateDirectory(Path.GetDirectoryName(shopdataPath));
                    if (File.Exists(shopdataPath)) shopdata = JObject.Parse(File.ReadAllText(shopdataPath)); else SaveShopdata();
                }
                catch (Exception) { SaveShopdata(); }
                //偏好设定
                try
                {
                    if (!Directory.Exists(Path.GetDirectoryName(preferencePath))) Directory.CreateDirectory(Path.GetDirectoryName(preferencePath));
                    if (File.Exists(preferencePath)) preference = JObject.Parse(File.ReadAllText(preferencePath)); else SavePreference();
                }
                catch (Exception) { SavePreference(); }
                #endregion
                #endregion
                // 表单选择监听
                api.addAfterActListener(EventKey.onFormSelect, x =>
                {
                    try
                    {
                        var e = BaseEvent.getFrom(x) as FormSelectEvent;
                        int index = FormQueue.FindIndex(l => l.id == e.formid);
                        if (index == -1) return false;
                        var receForm = FormQueue[index];
                        FormQueue.RemoveAt(index);
                        if (e.selected == "null")
                        {
                            if (receForm.Tag == FormTag.recycleMain || receForm.Tag == FormTag.sellMain || receForm.Tag == FormTag.preferenceMain)
                            {
                                SendMain(e.playername);
                            }
                            else
                            {
                                Feedback(e.playername, "§7表单已关闭，未收到操作");
                            }
                        }
                        else
                        {
                            switch (receForm.Tag)
                            {
                                case FormTag.Main:
                                    if (e.selected == "0")
                                        SendForm(new FormINFO(e.playername, FormType.SimpleIMG, FormTag.sellMain) { title = "购买", content = "点击购买", buttons = GetSell(shopdata["sell"] as JArray), domain = shopdata["sell"] });
                                    else if (e.selected == "1")
                                        SendForm(new FormINFO(e.playername, FormType.SimpleIMG, FormTag.recycleMain) { title = "回收站", content = "点击选择你想要回收的物品", buttons = GetRecycle(shopdata["recycle"] as JArray), domain = shopdata["recycle"] });
                                    else if (e.selected == "2")
                                    {
                                        var sizelist = new JArray() { "8", "16", "32", "64", "128", "256" };
                                        SendForm(new FormINFO(e.playername, FormType.Custom, FormTag.preferenceMain)
                                        {
                                            title = "个人偏好设置",
                                            content = new JArray
                                        {
                                            new JObject
                                            {
                                                new JProperty("type","step_slider"),
                                                new JProperty("text","●出售商店\n    数量输入方式"),
                                                new JProperty("steps",new JArray{ "游标滑块","文本框手动输入"}),
                                                new JProperty("default",preference[e.playername]["sell"].Value<int>("input_type"))
                                            },
                                            new JObject
                                            {
                                                new JProperty("type","dropdown"),
                                                new JProperty("text","    使用游标滑块输入时的最大值"),
                                                new JProperty("options", sizelist),
                                                new JProperty("default",preference[e.playername]["sell"].Value<int>("slide_size_i"))
                                            }  ,     new JObject
                                            {
                                                new JProperty("type","step_slider"),
                                                new JProperty("text","●回收商店\n    数量输入方式"),
                                                new JProperty("steps",new JArray{ "游标滑块","文本框手动输入"}),
                                                new JProperty("default",preference[e.playername]["recycle"].Value<int>("input_type"))
                                            },
                                            new JObject
                                            {
                                                new JProperty("type","dropdown"),
                                                new JProperty("text","    使用游标滑块输入时的最大值"),
                                                new JProperty("options",sizelist),
                                                new JProperty("default",preference[e.playername]["recycle"].Value<int>("slide_size_i"))
                                            }
                                        }
                                        });
                                    }
                                    break;
                                case FormTag.sellMain:
                                    try
                                    {
                                        if (e.selected == "0")
                                        {
                                            if (receForm.domain.Path == "sell")
                                                SendMain(e.playername);
                                            else
                                                SendForm(new FormINFO(e.playername, FormType.SimpleIMG, FormTag.sellMain) { title = "点击选择你想要购买的物品", content = "", buttons = GetSell((JArray)receForm.domain.Parent.Parent.Parent), domain = receForm.domain.Parent.Parent.Parent });
                                        }
                                        else
                                        {
                                            JObject selitem = (JObject)receForm.domain[int.Parse(e.selected) - 1];
                                            if (selitem.ContainsKey("type"))
                                                SendForm(new FormINFO(e.playername, FormType.SimpleIMG, FormTag.sellMain) { title = "点击选择你想要购买的物品", content = "", buttons = GetSell((JArray)selitem["content"]), domain = selitem["content"] });
                                            else
                                            {
                                                WriteLine("test1--" + selitem.ToString(Newtonsoft.Json.Formatting.None));
                                                JObject pre = (JObject)preference[e.playername]["sell"];
                                                WriteLine("test2--" + pre.ToString(Newtonsoft.Json.Formatting.None));
                                                var content = new JObject { new JProperty("text", "\n您已选择 " + selitem.Value<string>("name") + "\n\n" + (pre.Value<int>("input_type") == 0 ? "拖动滑块选择购买数量" : "在文本框中输入购买数量") + "\n单价: " + selitem.Value<string>("price") + "\n      §l§5X" + (pre.Value<int>("input_type") == 0 ? "§r\n数量" : "")) };
                                                if (pre.Value<int>("input_type") == 0)
                                                {
                                                    content.Add("type", "slider");
                                                    content.Add("min", 1);
                                                    content.Add("max", pre.Value<int>("slide_size"));
                                                    content.Add("step", 1);
                                                    content.Add("default", 1);
                                                }
                                                else
                                                {
                                                    content.Add("type", "input");
                                                    content.Add("default", "");
                                                    content.Add("placeholder", "购买数量");
                                                }
                                                SendForm(new FormINFO(e.playername, FormType.Custom, FormTag.confirmSell) { title = "输入购买数量", content = new JArray { content }, domain = selitem });
                                            }
                                        }
                                    }
                                    catch (Exception err) { WriteLine("sellMain ERROR\n" + err.ToString()); }
                                    break;
                                case FormTag.recycleMain:
                                    try
                                    {
                                        if (e.selected == "0")
                                        {
                                            if (receForm.domain.Path == "recycle")
                                                SendMain(e.playername);
                                            else
                                                SendForm(new FormINFO(e.playername, FormType.SimpleIMG, FormTag.recycleMain) { title = "点击选择你想要回收的物品", content = "", buttons = GetRecycle((JArray)receForm.domain.Parent.Parent.Parent), domain = receForm.domain.Parent.Parent.Parent });
                                        }
                                        else
                                        {
                                            JObject selitem = (JObject)receForm.domain[int.Parse(e.selected) - 1];
                                            if (selitem.ContainsKey("type"))
                                                SendForm(new FormINFO(e.playername, FormType.SimpleIMG, FormTag.recycleMain) { title = "点击选择你想要回收的物品", content = "", buttons = GetRecycle((JArray)selitem["content"]), domain = selitem["content"] });
                                            else
                                            {
                                                JObject pre = (JObject)preference[e.playername]["recycle"];
                                                var content = new JObject { new JProperty("text", "\n您已选择 " + selitem.Value<string>("name") + "\n\n" + (pre.Value<int>("input_type") == 0 ? "拖动滑块选择回收数量" : "在文本框中输入回收数量") + "\n单个收益: " + selitem.Value<string>("price") + "\n          §l§5X§r" + (pre.Value<int>("input_type") == 0 ? "§r\n 数  量 " : "")) };
                                                if (pre.Value<int>("input_type") == 0)
                                                {
                                                    content.Add("type", "slider");
                                                    content.Add("min", 1);
                                                    content.Add("max", pre.Value<int>("slide_size"));
                                                    content.Add("step", 1);
                                                    content.Add("default", 1);
                                                }
                                                else
                                                {
                                                    content.Add("type", "input");
                                                    content.Add("default", "");
                                                    content.Add("placeholder", "回收数量");
                                                }
                                                SendForm(new FormINFO(e.playername, FormType.Custom, FormTag.confirmRecycle) { title = "输入回收数量", content = new JArray { content }, domain = selitem });
                                            }
                                        }
                                    }
                                    catch (Exception err) { WriteLine("recycleMain ERROR\n" + err.ToString()); }
                                    break;
                                case FormTag.confirmSell:
                                    try
                                    {
                                        //receForm.domain;//选择项
                                        int count = JArray.Parse(e.selected).Value<int>(0);
                                        int total = (int)Math.Ceiling(receForm.domain.Value<decimal>("price") * count);
                                        if (total > 0)
                                        {
                                            SendForm(new FormINFO(e.playername, FormType.Model, FormTag.confirmedSell)
                                            {
                                                title = "确认购买",
                                                content = $"购买信息:\n  名称: {receForm.domain.Value<string>("name")}\n  数量: {count}\n  总价: {total}\n\n点击确认即可发送购买请求",
                                                buttons = new JArray { "确认购买", "我再想想" },
                                                domain = new JObject { new JProperty("item", receForm.domain), new JProperty("count", count), new JProperty("total", total), }
                                            });
                                            //ConfirmForm(je.playername, 'confirmedSell', '确认购买', '购买信息:', new Array(item, count, total))
                                        }
                                        else
                                        { Feedback(e.playername, "数值无效！"); }
                                    }
                                    catch (Exception err) { WriteLine("confirmSell ERROR\n" + err.ToString()); }
                                    break;
                                case FormTag.confirmRecycle:
                                    try
                                    {
                                        //receForm.domain;//选择项
                                        int count = JArray.Parse(e.selected).Value<int>(0);
                                        int total = (int)Math.Floor(receForm.domain.Value<decimal>("award") * count);
                                        if (total > 0)
                                        {
                                            SendForm(new FormINFO(e.playername, FormType.Model, FormTag.confirmedSell)
                                            {
                                                title = "确认回收",
                                                content = $"回收信息:\n  名称: {receForm.domain.Value<string>("name")}\n  数量: {count}\n  收益: {total}\n\n点击确认即可发送回收请求",
                                                buttons = new JArray { "确认回收", "我再想想" },
                                                domain = new JObject { new JProperty("item", receForm.domain), new JProperty("count", count), new JProperty("total", total), }
                                            });
                                            //ConfirmForm(je.playername, 'confirmedSell', '确认购买', '购买信息:', new Array(item, count, total))
                                        }
                                        else { Feedback(e.playername, "数值无效！"); }
                                    }
                                    catch (Exception err) { WriteLine("confirmSell ERROR\n" + err.ToString()); }
                                    break;
                                case FormTag.confirmedSell:
                                    try
                                    {
                                        if (e.selected == "true")
                                        {
                                            JObject item = receForm.domain.Value<JObject>("item");
                                            int total = receForm.domain.Value<int>("total");
                                            int count = receForm.domain.Value<int>("count");
                                            ExecuteCMD(e.playername, $"tag @s[scores={{money=..{total}}}] remove buy_success");
                                            ExecuteCMD(e.playername, $"tag @s[scores={{money={total}..}}] add buy_success");
                                            ExecuteCMD(e.playername, "titleraw @s times 5 25 10");
                                            //success
                                            ExecuteCMD(e.playername, $"give @s[tag=buy_success] {item.Value<string>("id")} {count} {item.Value<string>("damage")}");
                                            ExecuteCMD(e.playername, $"scoreboard players remove @s[tag=buy_success] money {total}");
                                            ExecuteCMD(e.playername, "titleraw @s[tag=buy_success] title {\"rawtext\":[{\"text\":\"\\n\\n\\n§b购买成功\"}]}");
                                            ExecuteCMD(e.playername, $"titleraw @s[tag=buy_success] subtitle {{\"rawtext\":[{{\"text\":\"已花费 {total} 像素币\\n购买 {count} 个 {item.Value<string>("name")} \"}}]}}");
                                            //fail
                                            ExecuteCMD(e.playername, "titleraw @s[tag=!buy_success] title {\"rawtext\":[{\"text\":\"\\n\\n\\n§c购买失败！\"}]}");
                                            ExecuteCMD(e.playername, $"titleraw @s[tag=!buy_success] subtitle {{\"rawtext\":[{{\"text\":\"\\n购买 {count} 个 {item.Value<string>("name")} 需要 {total} 像素币\"}}]}}");
                                            //-END
                                        }
                                        else
                                        {
                                            Feedback(e.playername, "购买已取消");
                                        }
                                    }
                                    catch (Exception err) { WriteLine("confirmedSell ERROR\n" + err.ToString()); }
                                    break;
                                case FormTag.confirmedRecycle:

                                    break;
                                case FormTag.preferenceMain:
                                    try
                                    {
                                        var set = JArray.Parse(e.selected).ToList().ConvertAll(l => int.Parse(l.ToString()));
                                        int[] size = new int[] { 8, 16, 32, 64, 128, 256 };
                                        preference[e.playername]["sell"]["input_type"] = set[0];
                                        preference[e.playername]["sell"]["slide_size_i"] = set[1];
                                        preference[e.playername]["sell"]["slide_size"] = size[set[1]];
                                        preference[e.playername]["recycle"]["input_type"] = set[2];
                                        preference[e.playername]["recycle"]["slide_size_i"] = set[3];
                                        preference[e.playername]["recycle"]["slide_size"] = size[set[3]];
                                        SavePreference();
                                        Feedback(e.playername, "个人设置保存成功");
                                    }
                                    catch (Exception err) { WriteLine("preferenceMain ERROR\n" + err.ToString()); }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        WriteLine("出错>位于onFormSelect\n" + err.Message);
                    }

                    return false;
                });

                #region 事件
                #region 控制台命令
                base_api.addBeforeActListener(EventKey.onServerCmd, x =>
                {
                    try
                    {
                        //Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                        var e = BaseEvent.getFrom(x) as ServerCmdEvent;
                        if (e != null)
                        {
                            if (e.cmd.ToLower() == "pf")
                            {
                                WriteLine("正在打开窗体");
                                ShowSettingWindow();
                                return false;
                            }
                        }
                    }
                    catch (Exception err) { WriteLine("出错>位于onServerCmd\n" + err.Message); }
                    return true;
                });
                #endregion
                #region 服务器指令
                // 输入指令监听
                api.setCommandDescribe("shop", "§r§ePixelFaramitaSHOP商店插件主菜单");
                //api.setCommandDescribeEx("shopi", "商店插件详细信息", MCCSAPI.CommandPermissionLevel.Admin, 0, 0);
                base_api.addBeforeActListener(EventKey.onInputCommand, x =>
                {
                    try
                    {
                        //Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                        var e = BaseEvent.getFrom(x) as InputCommandEvent;
                        if (e != null)
                        {
                            switch (e.cmd)
                            {
                                case "/shop":
                                    if (!preference.ContainsKey(e.playername))
                                    {
                                        preference.Add(e.playername, JObject.Parse("{\"sell\":{\"input_type\":0,\"slide_size_i\":1,\"slide_size\":16},\"recycle\":{\"input_type\":0,\"slide_size_i\":3,\"slide_size\":64}}"));
                                        SavePreference();
                                    }
                                    SendMain(e.playername);
                                    return false;
                                default:
                                    break;
                            }
                            //Console.WriteLine(" <{0}> {1}", e.playername, e.cmd);
                        }
                    }
                    catch (Exception err) { WriteLine("出错>位于onInputCommand\n" + err.Message); }
                    return true;
                });
                #endregion
                #endregion
                #region MODEL

                //// 后台指令输出监听
                //api.addBeforeActListener(EventKey.onServerCmdOutput, x => {
                //	Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                //	var se = BaseEvent.getFrom(x) as ServerCmdOutputEvent;
                //	if (se != null) {
                //		Console.WriteLine("后台指令输出={0}", se.output);
                //	}
                //	return true;
                //});

                //// 使用物品监听
                //api.addAfterActListener(EventKey.onUseItem, x => {
                //	Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                //	var ue = BaseEvent.getFrom(x) as UseItemEvent;
                //	if (ue != null && ue.RESULT) {
                //		Console.WriteLine("玩家 {0} 在 {1} 的 ({2}, {3}, {4})" +
                //			" 处使用了 {5} 物品。", ue.playername, ue.dimension, ue.position.x, ue.position.y, ue.position.z, ue.itemname);
                //	}
                //	return true;
                //});
                //// 放置方块监听
                //api.addAfterActListener(EventKey.onPlacedBlock, x => {
                //	Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                //	var ue = BaseEvent.getFrom(x) as PlacedBlockEvent;
                //	if (ue != null && ue.RESULT) {
                //		Console.WriteLine("玩家 {0} 在 {1} 的 ({2}, {3}, {4})" +
                //			" 处放置了 {5} 方块。", ue.playername, ue.dimension, ue.position.x, ue.position.y, ue.position.z, ue.blockname);
                //	}
                //	return true;
                //});
                //// 破坏方块监听
                //api.addBeforeActListener(EventKey.onDestroyBlock, x => {
                //	Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                //	var ue = BaseEvent.getFrom(x) as DestroyBlockEvent;
                //	if (ue != null) {
                //		Console.WriteLine("玩家 {0} 试图在 {1} 的 ({2}, {3}, {4})" +
                //			" 处破坏 {5} 方块。", ue.playername, ue.dimension, ue.position.x, ue.position.y, ue.position.z, ue.blockname);
                //	}
                //	return true;
                //});
                //// 开箱监听
                //api.addBeforeActListener(EventKey.onStartOpenChest, x =>
                //{
                //    Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                //    var ue = BaseEvent.getFrom(x) as StartOpenChestEvent;
                //    if (ue != null)
                //    {
                //        Console.WriteLine("玩家 {0} 试图在 {1} 的 ({2}, {3}, {4})" +
                //            " 处打开 {5} 箱子。", ue.playername, ue.dimension, ue.position.x, ue.position.y, ue.position.z, ue.blockname);
                //    }
                //    return false;
                //});
                //// 开桶监听
                //api.addBeforeActListener(EventKey.onStartOpenBarrel, x =>
                //{
                //    Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                //    var ue = BaseEvent.getFrom(x) as StartOpenBarrelEvent;
                //    if (ue != null)
                //    {
                //        Console.WriteLine("玩家 {0} 试图在 {1} 的 ({2}, {3}, {4})" +
                //            " 处打开 {5} 木桶。", ue.playername, ue.dimension, ue.position.x, ue.position.y, ue.position.z, ue.blockname);
                //    }
                //    return false;
                //});
                //// 关箱监听
                //api.addAfterActListener(EventKey.onStopOpenChest, x => {
                //	Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                //	var ue = BaseEvent.getFrom(x) as StopOpenChestEvent;
                //	if (ue != null) {
                //		Console.WriteLine("玩家 {0} 在 {1} 的 ({2}, {3}, {4})" +
                //			" 处关闭 {5} 箱子。", ue.playername, ue.dimension, ue.position.x, ue.position.y, ue.position.z, ue.blockname);
                //	}
                //	return true;
                //});
                //// 关桶监听
                //api.addAfterActListener(EventKey.onStopOpenBarrel, x => {
                //	Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                //	var ue = BaseEvent.getFrom(x) as StopOpenBarrelEvent;
                //	if (ue != null) {
                //		Console.WriteLine("玩家 {0} 在 {1} 的 ({2}, {3}, {4})" +
                //			" 处关闭 {5} 木桶。", ue.playername, ue.dimension, ue.position.x, ue.position.y, ue.position.z, ue.blockname);
                //	}
                //	return true;
                //});
                //// 放入取出监听
                //api.addAfterActListener(EventKey.onSetSlot, x => {
                //	Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                //	var e = BaseEvent.getFrom(x) as SetSlotEvent;
                //	if (e != null) {
                //		if (e.itemcount > 0)
                //			Console.WriteLine("玩家 {0} 在 {1} 槽放入了 {2} 个 {3} 物品。",
                //				e.playername, e.slot, e.itemcount, e.itemname);
                //		else
                //			Console.WriteLine("玩家 {0} 在 {1} 槽取出了物品。",
                //				e.playername, e.slot);
                //	}
                //	return true;
                //});
                //// 切换维度监听
                //api.addAfterActListener(EventKey.onChangeDimension, x => {
                //	Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                //	var e = BaseEvent.getFrom(x) as ChangeDimensionEvent;
                //	if (e != null && e.RESULT) {
                //			Console.WriteLine("玩家 {0} {1} 切换维度至 {2} 的 ({3},{4},{5}) 处。",
                //				e.playername, e.isstand?"":"悬空地", e.dimension, e.XYZ.x, e.XYZ.y, e.XYZ.z);
                //	}
                //	return true;
                //});
                //// 生物死亡监听
                //api.addAfterActListener(EventKey.onMobDie, x => {
                //	Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                //	var e = BaseEvent.getFrom(x) as MobDieEvent;
                //	if (e != null && !string.IsNullOrEmpty(e.mobname)) {
                //			Console.WriteLine(" {0} 在 {1} ({2:F2},{3:F2},{4:F2}) 处被 {5} 杀死了。",
                //				e.mobname, e.dimension, e.XYZ.x, e.XYZ.y, e.XYZ.z, e.srcname);
                //	}
                //	return true;
                //});
                //// 玩家重生监听
                //api.addAfterActListener(EventKey.onRespawn, x => {
                //	Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                //	var e = BaseEvent.getFrom(x) as RespawnEvent;
                //	if (e != null && e.RESULT) {
                //			Console.WriteLine("玩家 {0} 已于 {1} 的 ({2:F2},{3:F2},{4:F2}) 处重生。",
                //				e.playername, e.dimension, e.XYZ.x, e.XYZ.y, e.XYZ.z);
                //	}
                //	return true;
                //});
                //// 聊天监听
                //api.addAfterActListener(EventKey.onChat, x => {
                //	Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                //	var e = BaseEvent.getFrom(x) as ChatEvent;
                //	if (e != null) {
                //		Console.WriteLine(" {0} {1} 说：{2}", e.playername,
                //			!string.IsNullOrEmpty(e.target) ? "悄悄地对 " + e.target : "", e.msg);
                //	}
                //	return true;
                //});
                //// 输入文本监听
                //api.addBeforeActListener(EventKey.onInputText, x => {
                //	Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                //	var e = BaseEvent.getFrom(x) as InputTextEvent;
                //	if (e != null) {
                //		Console.WriteLine(" <{0}> {1}", e.playername, e.msg);
                //	}
                //	return true;
                //});


                //// 世界范围爆炸监听，拦截
                //api.addBeforeActListener(EventKey.onLevelExplode, x => {
                //	Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                //	var e = BaseEvent.getFrom(x) as LevelExplodeEvent;
                //	if (e != null) {
                //		Console.WriteLine("位于 {0} ({1},{2},{3}) 的 {4} 试图发生强度 {5} 的爆炸。",
                //			e.dimension, e.position.x, e.position.y, e.position.z,
                //			string.IsNullOrEmpty(e.entity) ? e.blockname : e.entity, e.explodepower);
                //	}
                //	return false;
                //});
                ///*
                //// 玩家移动监听
                //api.addAfterActListener(EventKey.onMove, x => {
                //	var e = BaseEvent.getFrom(x) as MoveEvent;
                //	if (e != null) {
                //		Console.WriteLine("玩家 {0} {1} 移动至 {2} ({3},{4},{5}) 处。",
                //			e.playername, (e.isstand) ? "":"悬空地", e.dimension,
                //			e.XYZ.x, e.XYZ.y, e.XYZ.z);
                //	}
                //	return false;
                //});
                //*/
                //// 玩家加入游戏监听
                //api.addAfterActListener(EventKey.onLoadName, x => {
                //	Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                //	var ue = BaseEvent.getFrom(x) as LoadNameEvent;
                //	if (ue != null) {
                //		Console.WriteLine("玩家 {0} 加入了游戏，xuid={1}", ue.playername, ue.xuid);
                //	}
                //	return true;
                //});
                //// 玩家离开游戏监听
                //api.addAfterActListener(EventKey.onPlayerLeft, x => {
                //	Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                //	var ue = BaseEvent.getFrom(x) as PlayerLeftEvent;
                //	if (ue != null) {
                //		Console.WriteLine("玩家 {0} 离开了游戏，xuid={1}", ue.playername, ue.xuid);
                //	}
                //	return true;
                //});

                //// 攻击监听
                //// API 方式注册监听器
                //api.addAfterActListener(EventKey.onAttack, x => {
                //	Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                //	AttackEvent ae = BaseEvent.getFrom(x) as AttackEvent;
                //	if (ae != null) {
                //		string str = "玩家 " + ae.playername + " 在 (" + ae.XYZ.x.ToString("F2") + "," +
                //			ae.XYZ.y.ToString("F2") + "," + ae.XYZ.z.ToString("F2") + ") 处攻击了 " + ae.actortype + " 。";
                //		Console.WriteLine(str);
                //		//Console.WriteLine("list={0}", api.getOnLinePlayers());
                //		string ols = api.getOnLinePlayers();
                //		if (!string.IsNullOrEmpty(ols))
                //                 {
                //			JavaScriptSerializer ser = new JavaScriptSerializer();
                //			ArrayList al = ser.Deserialize<ArrayList>(ols);
                //			object uuid = null;
                //			foreach (Dictionary<string, object> p in al)
                //			{
                //				object name;
                //				if (p.TryGetValue("playername", out name))
                //				{
                //					if ((string)name == ae.playername)
                //					{
                //						// 找到
                //						p.TryGetValue("uuid", out uuid);
                //						break;
                //					}
                //				}
                //			}
                //			if (uuid != null)
                //			{
                //				var id = api.sendSimpleForm((string)uuid,
                //								   "致命选项",
                //								   "test choose:",
                //								   "[\"生存\",\"死亡\",\"求助\"]");
                //				Console.WriteLine("创建需自行保管的表单，id={0}", id);
                //				//api.transferserver((string)uuid, "www.xiafox.com", 19132);
                //			}
                //		}
                //	} else {
                //		Console.WriteLine("Event convent fail.");
                //	}
                //	return true;
                //});
                //#region 非社区部分内容
                //if (api.COMMERCIAL)
                //{
                //	// 生物伤害监听
                //	api.addBeforeActListener(EventKey.onMobHurt, x => {
                //		Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                //		var e = BaseEvent.getFrom(x) as MobHurtEvent;
                //		if (e != null && !string.IsNullOrEmpty(e.mobname))
                //		{
                //			Console.WriteLine(" {0} 在 {1} ({2:F2},{3:F2},{4:F2}) 即将受到来自 {5} 的 {6} 点伤害，类型 {7}",
                //				e.mobname, e.dimension, e.XYZ.x, e.XYZ.y, e.XYZ.z, e.srcname, e.dmcount, e.dmtype);
                //		}
                //		return true;
                //	});
                //	// 命令块执行指令监听，拦截
                //	api.addBeforeActListener(EventKey.onBlockCmd, x => {
                //		Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                //		var e = BaseEvent.getFrom(x) as BlockCmdEvent;
                //		if (e != null)
                //		{
                //			Console.WriteLine("位于 {0} ({1},{2},{3}) 的 {4} 试图执行指令 {5}",
                //				e.dimension, e.position.x, e.position.y, e.position.z, e.name, e.cmd);
                //		}
                //		return false;
                //	});
                //	// NPC执行指令监听，拦截
                //	api.addBeforeActListener(EventKey.onNpcCmd, x => {
                //		Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                //		var e = BaseEvent.getFrom(x) as NpcCmdEvent;
                //		if (e != null)
                //		{
                //			Console.WriteLine("位于 {0} ({1},{2},{3}) 的 {4} 试图执行第 {5} 条指令，指令集\n{6}",
                //				e.dimension, e.position.x, e.position.y, e.position.z, e.npcname, e.actionid, e.actions);
                //		}
                //		return false;
                //	});
                //	// 更新命令方块监听
                //	api.addBeforeActListener(EventKey.onCommandBlockUpdate, x => {
                //		Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                //		var e = BaseEvent.getFrom(x) as CommandBlockUpdateEvent;
                //		if (e != null)
                //		{
                //			Console.WriteLine(" {0} 试图修改位于 {1} ({2},{3},{4}) 的 {5} 的命令为 {6}",
                //				e.playername, e.dimension, e.position.x, e.position.y, e.position.z,
                //				e.isblock ? "命令块" : "命令矿车", e.cmd);
                //		}
                //		return true;
                //	});
                //}
                //         #endregion

                #endregion


                // 高级玩法，硬编码方式注册hook

                //THook.init(api);
            }
            catch (Exception err)
            {
                WriteLine("插件遇到严重错误，无法继续运行\n错误信息:" + err.Message);
            }
        }
    }
}

namespace CSR
{
    partial class Plugin
    {
        /// <summary>
        /// 通用调用接口，需用户自行实现
        /// </summary>
        /// <param name="api">MC相关调用方法</param>
        public static void onStart(MCCSAPI api)
        {
            try
            {
                // TODO 此接口为必要实现
                CSRDemo.Program.init(api);
            }
            catch (Exception err)
            { Console.WriteLine(err.ToString()); }
        }
    }
}