//#define FromUrl
using System;
using System.Collections.Generic;
using System.Text;
using CSR;
using System.Threading.Tasks;
using ManageWindow;
using System.Threading;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.IO;
using System.Text.RegularExpressions;
using PFShop;
using static PFShop.FormINFO;
using System.Collections;
using Ookii.Dialogs.Wpf;
using System.Diagnostics;
using System.Net.Sockets;
using Timer = System.Timers.Timer;
using System.Windows.Threading;
//using PFShop;

namespace PFShop
{
    internal class Program
    {
        private static MCCSAPI api = null;
        #region console
        internal static void WriteLine(object content)
        {
            Console.Write($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss} ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("PFSHOP");
            Console.ForegroundColor = defaultForegroundColor;
            Console.Write("]");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("[Main] ");
            ResetConsoleColor();
            Console.WriteLine(content);
        }
        internal static void WriteLineERR(object type, object content)
        {
            Console.Write($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss} ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("PFSHOP");
            Console.ForegroundColor = defaultForegroundColor;
            Console.Write("]");
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("[ERROR] ");
            Console.BackgroundColor = ConsoleColor.DarkRed;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write($">{type}<");
            ResetConsoleColor();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(content);
            ResetConsoleColor();
        }
        private static ConsoleColor defaultForegroundColor = ConsoleColor.White;
        private static ConsoleColor defaultBackgroundColor = ConsoleColor.Black;
        private static void ResetConsoleColor()
        {
            Console.ForegroundColor = defaultForegroundColor;
            Console.BackgroundColor = defaultBackgroundColor;
        }
        #endregion        //internal static Task<T> StartSTATask<T>(Func<T> func)
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
        //        private static Thread windowthread = null;
        //        private static ManualResetEvent manualResetEvent = null;
        //        private static bool windowOpened = false;
        //        private static void StartWindowThread()
        //        {
        //            try
        //            {
        //                WriteLine("正在加载WPF库");
        //                while (true)
        //                {
        //                    try
        //                    {
        //                        windowOpened = true;
        //                        new MainWindow().ShowDialog();
        //                        windowOpened = false;
        //                        manualResetEvent = new ManualResetEvent(false);
        //                        GC.Collect();
        //#if DEBUG
        //                        WriteLine("窗体线程manualResetEvent返回:" +
        //#endif
        //                        manualResetEvent.WaitOne()
        //#if DEBUG
        //                                    )
        //#endif
        //                                    ;
        //                        manualResetEvent.Reset();
        //                    }
        //                    catch (Exception err) { WriteLine("窗体执行过程中发生错误\n信息" + err.ToString()); }
        //                }
        //            }
        //            catch (Exception err) { WriteLine("窗体线程发生严重错误\n信息" + err.ToString()); windowthread = null; }
        //        }
        //        private static void ShowSettingWindow()
        //        {
        //            try
        //            {
        //                if (windowthread == null || (!windowthread.IsAlive))
        //                {
        //                    windowthread = new Thread(StartWindowThread);
        //                    windowthread.SetApartmentState(ApartmentState.STA);
        //                    windowthread.Start();
        //                }
        //                else
        //                { if (windowOpened) WriteLine("窗体已经打开"); else manualResetEvent.Set(); }
        //            }
        //            catch (Exception
        //#if DEBUG
        //            err
        //#endif
        //            )
        //            {
        //#if DEBUG
        //                WriteLine(err.ToString());
        //#endif
        //            }
        //        }
        //private static Thread WindowThread = null;
        private static Dispatcher WindowDispatcher = null;
        private static bool windowOpened = false;
        private static void ShowSettingWindow()
        {
            if (windowOpened) WriteLine("窗体已经打开");
            else
            {
                #region Method1
                if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
                {
                    Thread.CurrentThread.SetApartmentState(ApartmentState.STA);
                    WriteLine("为加载UI,已设置当前运行线程为" + Thread.CurrentThread.GetApartmentState() + "线程");
                }
                try
                {
                    WriteLine("正在打开窗体");
                    Console.Beep();
                    var height = Console.WindowHeight;
                    var width = Console.WindowWidth;
                    windowOpened = true;
                    new MainWindow().ShowDialog();
                    GC.Collect();
                    Console.SetWindowSize(width, height);
                    windowOpened = false;
                }
                catch (Exception err) { WriteLine("窗体线程发生严重错误\n信息" + err.ToString()); windowOpened = false; }
                try
                {
                    if (WindowDispatcher == null)
                    {
                        Thread WindowThread = new Thread((ThreadStart)delegate { });
                        WindowThread.SetApartmentState(ApartmentState.STA);
                        WriteLine("为加载UI,已设置当前插件运行线程为" + WindowThread.GetApartmentState() + "线程");
                        WindowThread.Start();
                        WindowDispatcher = Dispatcher.FromThread(WindowThread);
                        //WindowDispatcher.Thread.Start();
                    }
                }
                catch (Exception errr) { WriteLineERR("", errr); }

                #endregion
                #region Method2
                //if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
                //{
                //    Thread.CurrentThread.SetApartmentState(ApartmentState.STA);
                //    WriteLine("为加载UI,已设置当前运行线程为" + Thread.CurrentThread.GetApartmentState() + "线程");
                //}
                //try
                //{
                //    Dispatcher.CurrentDispatcher.Invoke((Action)delegate
                //    {
                //        try
                //        {
                //            WriteLine("正在打开窗体");
                //            Console.Beep();
                //            var height = Console.WindowHeight;
                //            var width = Console.WindowWidth;
                //            windowOpened = true;
                //            new MainWindow().ShowDialog();
                //            GC.Collect();
                //            Console.SetWindowSize(width, height);
                //            windowOpened = false;
                //        }
                //        catch (Exception err) { WriteLine("窗体线程发生严重错误\n信息" + err.ToString()); windowOpened = false; }
                //    }, DispatcherPriority.Send);
                //}
                //catch (Exception err) { WriteLine("获取窗体线程时发生故障\n信息" + err.ToString()); windowOpened = false; }
                #endregion
            }
        }
        #region API方法补充
        internal static string GetUUID(string name) => JArray.Parse(api.getOnLinePlayers()).First(l => l.Value<string>("playername") == name).Value<string>("uuid");
        internal static void Feedback(string name, string text) => ExecuteCMD(name, $"tellraw @s {{\"rawtext\":[{{\"text\":\"§e§l[PFSHOP]§r§b{StringToUnicode(text)}\"}}]}}");
        internal static string StringToUnicode(string s)//字符串转UNICODE代码
        {
            char[] charbuffers = s.ToCharArray();
            byte[] buffer;
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < charbuffers.Length; i++)
            {
                buffer = Encoding.Unicode.GetBytes(charbuffers[i].ToString());
                sb.Append(string.Format("\\u{0:X2}{1:X2}", buffer[1], buffer[0]));
            }
            return sb.ToString();
        }
        #region CMD
        private static int cmdCount = 0;
        internal static Timer ServerCmdOutputTimer = new Timer(3000);
        private static void ServerCmdOutputTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
#if DEBUG
            WriteLine(cmdCount);
#endif
            ServerCmdOutputDetect();
        }
        internal static bool ServerCmdOutputDetect()
        {
            if (cmdCount > 0)
            {
                DqCmd();
                return false;
            }
            return true;
        }
        internal static bool ServerCmdOutputDetect(Events e)
        {
            try
            {
                return ServerCmdOutputDetect();
            }
            catch (Exception) { return true; }
        }
        private static void EqCmd()
        {
            if (cmdCount == 0)
            {
                api.addBeforeActListener(EventKey.onServerCmdOutput, ServerCmdOutputDetect);
                if (!ServerCmdOutputTimer.Enabled) ServerCmdOutputTimer.Start();
            }
            cmdCount++;
#if DEBUG
            WriteLine("EQ" + cmdCount);
#endif
        }
        private static void DqCmd()
        {
            cmdCount--;
            if (cmdCount == 0)
            {
                api.removeBeforeActListener(EventKey.onServerCmdOutput, ServerCmdOutputDetect);
                if (ServerCmdOutputTimer.Enabled) ServerCmdOutputTimer.Stop();
            }
#if DEBUG
            WriteLine("DQ" + cmdCount);
#endif
        }
        internal static void ExecuteCMD(string name, string cmd)
        {
            try
            {
                if (!(cmd.StartsWith("say") || cmd.StartsWith("tellraw"))) EqCmd();
            }
            catch (Exception) { }
            api.runcmd($"execute \"{name}\" ~~~ {cmd}");
        }
        #endregion
        #endregion
        #region 表单方法
        internal static void LoadFormTip(string playername)
        {
            ExecuteCMD(playername, "titleraw @s times 0 20 10");
            //ExecuteCMD(playername, "titleraw @s title {\"rawtext\":[{\"text\":\"\n\n\n\n\"}]}");
            ExecuteCMD(playername, "titleraw @s title {\"rawtext\":[{\"text\":\"§a   Loading    §a\"}]}");
            _ = Task.Run(() =>
            {
                Thread.Sleep(100);
                ExecuteCMD(playername, "titleraw @s title {\"rawtext\":[{\"text\":\"§a   Loading..   §a\"}]}");
                Thread.Sleep(100);
                ExecuteCMD(playername, "titleraw @s title {\"rawtext\":[{\"text\":\"§a   Loading....  §a\"}]}");
                Thread.Sleep(100);
                ExecuteCMD(playername, "titleraw @s title {\"rawtext\":[{\"text\":\"§a   Loading...... §a\"}]}");
                Thread.Sleep(100);
                ExecuteCMD(playername, "titleraw @s clear");
            });
        }
        internal static List<FormINFO> FormQueue = new List<FormINFO>();
        internal static void SendForm(FormINFO form)
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
                                        if (btsou["image"].Type == JTokenType.String)
                                        {
                                            button.Add("image", new JObject{
                                                new JProperty("type",Regex.IsMatch(btsou.Value<string>("image"),@"[a-zA-Z0-9][-a-zA-Z0-9]{0,62}(\.[a-zA-Z0-9][-a-zA-Z0-9]{0,62})+\.?")   ? "url" : "path"),
                                                new JProperty("data",btsou.Value<string>("image"))
                                            });
                                        }
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
        internal static JObject GetButton(string text, string image) => new JObject { new JProperty("text", text), new JProperty("image", image), };
        internal static JObject GetButtonRaw(string text) => new JObject { new JProperty("text", text) };
        internal static Func<JArray, JArray> GetSell = (items) =>
       {
           JArray get = new JArray() { GetButtonRaw(lang.sellPreviousList) };
           foreach (JObject item in items)
           {
               //WriteLine(GetButton($"#{item.Value<string>("order")}§l{item.Value<string>("name")}\n{(item.Value<decimal>("price") % 1 > 0 ? item["price"].ToString() + '+' : Math.Round(item.Value<decimal>("price")).ToString())}像素币/个"
               //    , item.Value<string>("image")));
               if (item.ContainsKey("type")) get.Add(GetButton(string.Format(lang.sellSubList, item.Value<string>("type")), item.Value<string>("image")));
               else
               {
                   decimal price = 0; try { price = item.Value<decimal>("price"); } catch (Exception) { }
                   get.Add(GetButton(string.Format(lang.sellListItem, item.Value<string>("order"), item.Value<string>("name"), price % 1 > 0 ? price.ToString() + '+' : Math.Round(price).ToString())
                 , item.Value<string>("image")));
               }
           }
           //WriteLine(get.ToString(Newtonsoft.Json.Formatting.None));
           return get;
       };
        internal static Func<JArray, JArray> GetRecycle = (items) =>
        {
            JArray get = new JArray() { GetButtonRaw(lang.recyclePreviousList) };
            foreach (JObject item in items)
            {
                if (item.ContainsKey("type")) get.Add(GetButton(string.Format(lang.recycleSubList, item.Value<string>("type")), item.Value<string>("image")));
                else
                {
                    decimal award = 0; try { award = item.Value<decimal>("award"); } catch (Exception) { }
                    get.Add(GetButton(string.Format(lang.recycleListItem, item.Value<string>("order"), item.Value<string>("name"), award % 1 > 0 ? award.ToString() + '-' : Math.Round(award).ToString())
                  , item.Value<string>("image")));
                }
            }
            return get;
        };
        #endregion
        #region 方便调用的方法
        internal static void SendMain(string playername)
        {
            SendForm(new FormINFO(playername, FormType.Simple, FormTag.Main) { title = lang.ShopMainTitle, content = lang.ShopMainContent, buttons = new JArray { lang.ShopMainSell, lang.ShopMainRecycle, lang.ShopMainPref } });
        }
        #endregion
        #endregion
        #region 配置
        ////插件设置
        //internal static
        //商店信息
        internal static JObject shopdata = new JObject();
        internal static string shopdataPath = Path.GetFullPath("plugins\\pfshop\\shopdata.json");
        internal static void SaveShopdata() => File.WriteAllText(shopdataPath, shopdata.ToString());
        //偏好设定
        internal static JObject preference = new JObject();
        internal static string preferencePath = Path.GetFullPath("plugins\\pfshop\\preference.json");
        internal static void SavePreference() => File.WriteAllText(preferencePath, preference.ToString());
        #endregion
        #region URLParse
#if FromUrl
        private static void SetFromUrl(string url)
        {
            Task.Run(() =>
            {
                try
                {
                    Native.Tool.Http.HttpWebClient webClient = new Native.Tool.Http.HttpWebClient();
                    webClient.Encoding = Encoding.UTF8;
                    //JObject shopdata =;
                    JArray shopBuy = new JArray();
                    JArray shopRecycle = new JArray();
                    List<string> buyPath = new List<string>();
                    List<string> recyclePath = new List<string>();
                    JArray getContent(JArray data, List<string> path)
                    {
                        if (path.Count == 0) return data;
                        string pathi = path.First();
                        path.RemoveAt(0);
                        if (!data.Any(l => l.Value<string>("type") == pathi))
                        {
                            //var i8 = shop_array[index - 1].split('\t')[8].replace(/ (\r |\n) */ g, '')
                            //i.image = i8 == '' ? false : i8
                            data.Add(new JObject {
                                            { "type", pathi },
                                            {"order",""},
                                            {"image", null },
                                            {"content",new JArray()}
                                        });
                        }
                        return getContent((JArray)data.First(l => l.Value<string>("type") == pathi)["content"], path);
                    }
                    foreach (var shopItem in webClient.DownloadString(url).Split('>'))
                    {
                        var cells = shopItem.Split('\t');
                        if (cells.Length == 16)
                        {
        #region 回收 
                            try { _ = Math.Round(double.Parse(Regex.Replace(cells[7], "[^\\d.]", ""))); } catch { cells[7] = "-1"; }
                            if (cells[2] == "✔")
                            {
                                JObject addObj = new JObject {
                                    {"order", cells[1]},
                                    {"name",cells[3] },
                                    {"id",  cells[4]},
                                    {"damage",  cells[5]},
                                    {"award", Math.Round(double.Parse(Regex.Replace(cells[7], "[^\\d.-]", "")))},
                                    {"image",string.IsNullOrWhiteSpace(cells[8] ) ? null : cells[8]}
                                };
                                if (!string.IsNullOrWhiteSpace(cells[6])) addObj.Add("regex", cells[6]);
                                getContent(shopRecycle, recyclePath.ToList()).Add(addObj);
                            }
                            //分割
                            else if (cells[2] == "═")
                            {
                                if (cells[4].Length == recyclePath.Count)
                                    recyclePath[recyclePath.Count - 1] = cells[3];
                                else if (cells[4].Length < recyclePath.Count)
                                    recyclePath.RemoveAt(recyclePath.Count - 1);
                                else
                                    recyclePath.Add(cells[3]);
                            }
        #endregion
        #region 购买
                            try { _ = Math.Round(double.Parse(Regex.Replace(cells[14], "[^\\d.]", ""))); } catch { cells[14] = "-1"; }
                            if (cells[10] == "✔")
                            {
                                getContent(shopBuy, buyPath.ToList()).Add(new JObject {
                                    {"order", cells[1]},
                                    {"name",cells[11] },
                                    {"id",  cells[12]},
                                    {"damage",  cells[13]},
                                    {"price", Math.Round(double.Parse(Regex.Replace(cells[14], "[^\\d.-]", "")))},
                                    {"image",string.IsNullOrWhiteSpace(cells[15] ) ? null : (cells[15]).Replace("\r\n","")}
                                });
                            }
                            //分割
                            else if (cells[10] == "═")
                            {
                                if (cells[12].Length == buyPath.Count)
                                    buyPath[buyPath.Count - 1] = cells[11];
                                else if (cells[12].Length < buyPath.Count)
                                    buyPath.RemoveAt(buyPath.Count - 1);
                                else
                                    buyPath.Add(cells[11]);
                            }
        #endregion

                            //                                i.type = indexP
                            //                                i.order = cells[1]
                            //                                let i15 = shop_array[index - 1].split('\t')[15].replace(/ (\r |\n) */ g, '')
                            //                                i.image = i15 == '' ? false : i15
                            //         
                            //                            i.order = cells[1]
                            //                            i.name = cells[11]
                            //                            i.id = cells[12]
                            //                            i.damage = cells[13]
                            //                            i.price = cells[14].split(' ')[0].replace(',', '')
                            //                            let i15 = cells[15].replace(/ (\r |\n) */ g, '')
                            //                            i.image = i15 == '' ? false : i15
                            //                            obj.push(i)
                            //                        }
                            //    return obj
                            //                    }
                            //let sell_copy = new Array()
                            //                    shopdata.sell.forEach(l => sell_copy.push(l))
                            //                    shopdata.sell = GetContent(sell_copy)

                            //                } else if (cells[10] == '═') {
                            //                    if (cells[12].length == path_sell.length) {
                            //                        path_sell[path_sell.length - 1] = cells[11]
                            //                    } else if (cells[12].length<path_sell.length) {
                            //                        path_sell.pop()
                            //                    } else {
                            //                        path_sell.push(cells[11])
                            //                    }
                            //                }
                            //            }
                        }
                    }
                    shopdata = new JObject { { "sell", shopBuy }, { "recycle", shopRecycle } };
                    SaveShopdata();
                }
                catch (Exception err) { WriteLineERR("网络读取失败", url + "\n" + err); }
            });
        }
#endif
        #endregion
        //语言文件
        private static Language lang = new Language();
        internal static void Init(MCCSAPI base_api)
        {
            ServerCmdOutputTimer.Elapsed += ServerCmdOutputTimer_Elapsed;
            WriteLine("test");
            _ = Task.Run(() =>
            {
                Thread.Sleep(11000);
                api.runcmd("scoreboard objectives add money dummy §b像素币");
#if DEBUG
                //for (int i = 0; i < 20; i++)
                //{
                //    api.runcmd("say test");
                //    EqCmd();
                //}
#endif
                EqCmd();
            });
            try
            {
#if DEBUG
                //base_api.addBeforeActListener(EventKey.onServerCmdOutput, (e) =>
                //{
                //    var ev = ServerCmdOutputEvent.getFrom(e);
                //    WriteLine(ev.output);
                //    return false;
                //});
                //base_api.addBeforeActListener(EventKey.onMove, (e) =>
                //{
                //    var ev = MoveEvent.getFrom(e);
                //    WriteLine(string.Format("{0}: {1} {2} {3}", ev.playername, ev.XYZ.x, ev.XYZ.y, ev.XYZ.z));
                //    return true;
                //});
#endif
                #region 加载
                api = base_api;
                Console.OutputEncoding = Encoding.UTF8;
                defaultForegroundColor = Console.ForegroundColor;
                defaultBackgroundColor = Console.BackgroundColor;
                #region INFO
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.BackgroundColor = ConsoleColor.Magenta;
                try
                {

                    string[] authorsInfo = new string[] {
                        "███████████████████████████" ,
                        "正在裝載PFSHOP",
                        "作者           gxh2004",
                        "版本信息    v" + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString() ,
                        "适用于bds1.16(CSRV0.1.16.40.2编译)"  ,
                        "如版本不同可能存在问题" ,
                        "基於C#+WPF窗體"  ,
                        "当前CSRunnerAPI版本:" + api.VERSION  ,
                        "控制台輸入\"pf\"即可打開快速配置窗體(未完善)"  ,
                        "███████████████████████████"
                    };
                    Func<string, int> GetLength = (input) => { return Encoding.GetEncoding("GBK").GetByteCount(input); };
                    int infoLength = 0;
                    foreach (var line in authorsInfo) infoLength = Math.Max(infoLength, GetLength(line));
                    for (int i = 0; i < authorsInfo.Length; i++)
                    {
                        while (GetLength(authorsInfo[i]) < infoLength)
                        {
                            authorsInfo[i] += " ";
                        }
                        Console.WriteLine("█" + authorsInfo[i] + "█");
                    }

                }
                catch (Exception) { }
                ResetConsoleColor();
                #endregion
                #region 读取配置
                //语言文件
                try
                {
                    string languagePath = Path.GetFullPath("plugins\\pfshop\\lang.json");
                    //JObject language = new JObject();
                    if (!Directory.Exists(Path.GetDirectoryName(shopdataPath))) Directory.CreateDirectory(Path.GetDirectoryName(shopdataPath));
                    if (File.Exists(languagePath))
                    {
                        lang = Newtonsoft.Json.JsonConvert.DeserializeObject<Language>(File.ReadAllText(languagePath));
                        //lang = JObject.Parse(File.ReadAllText(languagePath)).ToObject<Language>();
                    }
                    else { WriteLineERR(lang.CantFindLanguage, string.Format(lang.SaveDefaultLanguageTo, languagePath)); }
                    File.WriteAllText(languagePath, JObject.FromObject(lang).ToString());
                }
                catch (Exception err) { WriteLineERR("Lang", string.Format(lang.LanguageFileLoadFailed, err.ToString())); }
#if !DEBUG
                #region EULA 
                int height = 0, width = 0; string title = null;
                try
                {
                    height = Console.WindowHeight;
                    width = Console.WindowWidth; 
                    title = Console.Title;
                }
                catch (Exception) { }
                //set
                Dispatcher.CurrentDispatcher.Invoke(() =>
                {
                    try
                    {
                        if (!Directory.Exists(Path.GetDirectoryName(shopdataPath))) Directory.CreateDirectory(Path.GetDirectoryName(shopdataPath));
                        string eulaPath = Path.GetDirectoryName(shopdataPath) + "\\EULA";
                        string version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                        JObject eulaINFO = new JObject { new JProperty("author", "gxh"), new JProperty("version", version) };
                        try
                        {
                            if (File.Exists(eulaPath))
                            {
                                if (Encoding.UTF32.GetString(File.ReadAllBytes(eulaPath)) != StringToUnicode(eulaINFO.ToString()).GetHashCode().ToString())
                                {
                                    WriteLineERR("EULA", "使用条款需要更新!");
                                    File.Delete(eulaPath);
                                    throw new Exception();
                                }
                            }
                            else throw new Exception();
                        }
                        catch (Exception)
                        {
                            try
                            {
                                Console.Beep(); Console.SetWindowSize(Console.WindowWidth, 3);
                                WriteLine("请同意使用条款"); Console.Title = "当前控制台会无法操作，请同意使用条款即可恢复";
                            }
                            catch { }
                            using (TaskDialog dialog = new TaskDialog())
                            {
                                dialog.WindowTitle = "接受食用条款";
                                dialog.MainInstruction = "假装下面是本插件的食用条款";
                                dialog.Content =
                                    "1.请在遵守CSRunner前置使用协议的前提下使用本插件\n" +
                                    "2.不保证本插件不会影响服务器正常运行，如使用本插件造成服务端奔溃等问题，均与作者无瓜\n" +
                                    "3.严厉打击插件倒卖等行为，共同维护良好的开源环境";
                                dialog.ExpandedInformation = "点开淦嘛,没东西[doge]";
                                dialog.Footer = "本插件 <a href=\"https://github.com/littlegao233/PFShop-CS\">GitHub开源地址</a>.";
                                dialog.HyperlinkClicked += new EventHandler<HyperlinkClickedEventArgs>((sender, e) => { Process.Start("https://github.com/littlegao233/PFShop-CS"); });
                                dialog.FooterIcon = TaskDialogIcon.Information;
                                dialog.EnableHyperlinks = true;
                                TaskDialogButton acceptButton = new TaskDialogButton("Accept");
                                dialog.Buttons.Add(acceptButton);
                                TaskDialogButton refuseButton = new TaskDialogButton("拒绝并关闭本插件");
                                dialog.Buttons.Add(refuseButton);
                                if (dialog.ShowDialog() == refuseButton)
                                    throw new Exception("---尚未接受食用条款，本插件加载失败---");
                            }
                            File.WriteAllBytes(eulaPath, Encoding.UTF32.GetBytes(StringToUnicode(eulaINFO.ToString()).GetHashCode().ToString()));
                        }
                    }
                    catch (Exception err) { WriteLineERR("条款获取出错", err); }
                });
                //recover  
                try
                {
                    if (title != null)
                    {
                        Console.Title = title;
                        Console.SetWindowSize(width, height);
                    }
                }
                catch (Exception) { }
                #endregion
#endif
                //商店信息
                try
                {
                    if (!Directory.Exists(Path.GetDirectoryName(shopdataPath))) Directory.CreateDirectory(Path.GetDirectoryName(shopdataPath));
                    if (File.Exists(shopdataPath)) shopdata = JObject.Parse(File.ReadAllText(shopdataPath));
                    else
                    {
                        shopdata = JObject.Parse(lang.DefaultShopData);
                        SaveShopdata();
                        WriteLineERR(lang.CantFindConfig, string.Format(lang.SaveDefaultConfigTo, shopdataPath));
                    }
#if FromUrl
                    SetFromUrl("https://gitee.com/Pixel_Faramita/webAPI/raw/master/survival/shop.txt");
#endif
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
                        if (index == -1) return true;
                        var receForm = FormQueue[index];
                        FormQueue.RemoveAt(index);
                        if (e.selected == "null")
                        {
                            if (receForm.Tag == FormTag.recycleMain || receForm.Tag == FormTag.sellMain || receForm.Tag == FormTag.preferenceMain)
                                SendMain(e.playername);
                            else if (receForm.Tag == FormTag.InputSellDetail)
                                SendForm(new FormINFO(e.playername, FormType.SimpleIMG, FormTag.sellMain) { title = lang.sellMainTitle, content = lang.sellMainContent, buttons = GetSell((JArray)receForm.domain_source), domain = receForm.domain.Parent.Parent.Parent });
                            else if (receForm.Tag == FormTag.InputRecycleDetail)
                                SendForm(new FormINFO(e.playername, FormType.SimpleIMG, FormTag.recycleMain) { title = lang.recycleMainTitle, content = lang.recycleMainContent, buttons = GetRecycle((JArray)receForm.domain_source), domain = receForm.domain.Parent.Parent.Parent });
                            else
                                Feedback(e.playername, lang.ClosedFormWithoutAction);
                        }
                        else
                        {
                            switch (receForm.Tag)
                            {
                                case FormTag.Main:
                                    if (e.selected == "0")
                                        SendForm(new FormINFO(e.playername, FormType.SimpleIMG, FormTag.sellMain) { title = lang.sellMainTitle, content = lang.sellMainContent, buttons = GetSell(shopdata["sell"] as JArray), domain = shopdata["sell"] });
                                    else if (e.selected == "1")
                                        SendForm(new FormINFO(e.playername, FormType.SimpleIMG, FormTag.recycleMain) { title = lang.recycleMainTitle, content = lang.recycleMainContent, buttons = GetRecycle(shopdata["recycle"] as JArray), domain = shopdata["recycle"] });
                                    else if (e.selected == "2")
                                    {
                                        var sizelist = new JArray() { "8", "16", "32", "64", "128", "256" };
                                        SendForm(new FormINFO(e.playername, FormType.Custom, FormTag.preferenceMain)
                                        {
                                            title = lang.preferenceMainTitle,
                                            content = new JArray
                                        {
                                            new JObject
                                            {
                                                new JProperty("type","step_slider"),
                                                new JProperty("text", lang.preferenceMainSellText),
                                                new JProperty("steps",new JArray{ lang.preferenceMainUsingSlider, lang.preferenceMainUsingTextbox}),
                                                new JProperty("default",preference[e.playername]["sell"].Value<int>("input_type"))
                                            },
                                            new JObject
                                            {
                                                new JProperty("type","dropdown"),
                                                new JProperty("text",lang.preferenceMainMaxValueTip),
                                                new JProperty("options", sizelist),
                                                new JProperty("default",preference[e.playername]["sell"].Value<int>("slide_size_i"))
                                            }  ,     new JObject
                                            {
                                                new JProperty("type","step_slider"),
                                                new JProperty("text",lang.preferenceMainRecycleText),
                                                new JProperty("steps",new JArray{ lang.preferenceMainUsingSlider, lang.preferenceMainUsingTextbox}),
                                                new JProperty("default",preference[e.playername]["recycle"].Value<int>("input_type"))
                                            },
                                            new JObject
                                            {
                                                new JProperty("type","dropdown"),
                                                new JProperty("text",lang.preferenceMainMaxValueTip),
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
                                            if (receForm.domain.Path == "sell") SendMain(e.playername);
                                            else SendForm(new FormINFO(e.playername, FormType.SimpleIMG, FormTag.sellMain) { title = lang.sellMainTitle, content = lang.sellMainContent, buttons = GetSell((JArray)receForm.domain.Parent.Parent.Parent), domain = receForm.domain.Parent.Parent.Parent });
                                        }
                                        else
                                        {
                                            JObject selitem = (JObject)receForm.domain[int.Parse(e.selected) - 1];
                                            if (selitem.ContainsKey("type"))
                                                SendForm(new FormINFO(e.playername, FormType.SimpleIMG, FormTag.sellMain) { title = lang.sellMainTitle, content = lang.sellMainContent, buttons = GetSell((JArray)selitem["content"]), domain = selitem["content"] });
                                            else
                                            {
                                                //WriteLine("test1--" + selitem.ToString(Newtonsoft.Json.Formatting.None));
                                                JObject pre = (JObject)preference[e.playername]["sell"];
                                                //WriteLine("test2--" + pre.ToString(Newtonsoft.Json.Formatting.None));
                                                var content = new JObject();
                                                if (pre.Value<int>("input_type") == 0)
                                                {
                                                    content.Add("text", string.Format(lang.InputSellDetailContentWhenUseSlider, selitem.Value<string>("name"), selitem.Value<string>("price")));
                                                    content.Add("type", "slider");
                                                    content.Add("min", 1);
                                                    content.Add("max", pre.Value<int>("slide_size"));
                                                    content.Add("step", 1);
                                                    content.Add("default", 1);
                                                }
                                                else
                                                {
                                                    content.Add("text", string.Format(lang.InputSellDetailContentWhenUseTextbox, selitem.Value<string>("name"), selitem.Value<string>("price")));
                                                    content.Add("type", "input");
                                                    content.Add("default", "");
                                                    content.Add("placeholder", lang.InputSellDetailContentTextboxPlaceholder);
                                                }
                                                SendForm(new FormINFO(e.playername, FormType.Custom, FormTag.InputSellDetail) { title = lang.InputSellDetailTitle, content = new JArray { content }, domain = selitem });
                                            }
                                        }
                                    }
                                    catch (Exception err) { WriteLineERR("sellMain", err.ToString()); }
                                    break;
                                case FormTag.recycleMain:
                                    try
                                    {
                                        if (e.selected == "0")
                                        {
                                            if (receForm.domain.Path == "recycle") SendMain(e.playername);
                                            else SendForm(new FormINFO(e.playername, FormType.SimpleIMG, FormTag.recycleMain) { title = lang.recycleMainTitle, content = lang.recycleMainContent, buttons = GetRecycle((JArray)receForm.domain.Parent.Parent.Parent), domain = receForm.domain.Parent.Parent.Parent });
                                        }
                                        else
                                        {
                                            JObject selitem = (JObject)receForm.domain[int.Parse(e.selected) - 1];
                                            if (selitem.ContainsKey("type"))
                                                SendForm(new FormINFO(e.playername, FormType.SimpleIMG, FormTag.recycleMain) { title = lang.recycleMainTitle, content = lang.recycleMainContent, buttons = GetRecycle((JArray)selitem["content"]), domain = selitem["content"] });
                                            else
                                            {
                                                JObject pre = (JObject)preference[e.playername]["recycle"];
                                                var content = new JObject();
                                                if (pre.Value<int>("input_type") == 0)
                                                {
                                                    content.Add("text", string.Format(lang.InputRecycleDetailContentWhenUseSlider, selitem.Value<string>("name"), selitem.Value<string>("award")));
                                                    content.Add("type", "slider");
                                                    content.Add("min", 1);
                                                    content.Add("max", pre.Value<int>("slide_size"));
                                                    content.Add("step", 1);
                                                    content.Add("default", 1);
                                                }
                                                else
                                                {
                                                    content.Add("text", string.Format(lang.InputRecycleDetailContentWhenUseTextbox, selitem.Value<string>("name"), selitem.Value<string>("award")));
                                                    content.Add("type", "input");
                                                    content.Add("default", "");
                                                    content.Add("placeholder", lang.InputRecycleDetailContentTextboxPlaceholder);
                                                }
                                                SendForm(new FormINFO(e.playername, FormType.Custom, FormTag.InputRecycleDetail) { title = lang.InputRecycleDetailTitle, content = new JArray { content }, domain = selitem });
                                            }
                                        }
                                    }
                                    catch (Exception err) { WriteLineERR("recycleMain", err.ToString()); }
                                    break;
                                case FormTag.InputSellDetail:
                                    try
                                    {
                                        //receForm.domain;//选择项 
                                        int count = Convert.ToInt32(JArray.Parse(e.selected)[0]);
                                        if (count > short.MaxValue) throw new Exception();
                                        int total = (int)Math.Ceiling(receForm.domain.Value<decimal>("price") * count);
                                        if (total > 0 && count > 0)
                                        {
                                            SendForm(new FormINFO(e.playername, FormType.Model, FormTag.confirmedSell)
                                            {
                                                title = lang.confirmSellTitle,
                                                content = string.Format(lang.confirmSellContent, receForm.domain.Value<string>("name"), count, total),
                                                buttons = new JArray { lang.confirmSellAccept, lang.confirmSellCancel },
                                                domain = new JObject { new JProperty("item", receForm.domain), new JProperty("count", count), new JProperty("total", total), },
                                                domain_source = (JArray)receForm.domain.Parent
                                            });
                                        }
                                        else
                                        { Feedback(e.playername, lang.InputRecycleDetailValueInvalid); }
                                    }
                                    catch (Exception err) { WriteLineERR("InputSellDetail", err.ToString()); }
                                    break;
                                case FormTag.InputRecycleDetail:
                                    try
                                    {    //receForm.domain;//选择项
                                        int count = Convert.ToInt32(JArray.Parse(e.selected)[0]);
                                        if (count > short.MaxValue) throw new Exception();
                                        int total = (int)Math.Floor(receForm.domain.Value<decimal>("award") * count);
                                        if (total > 0 && count > 0)
                                        {
                                            SendForm(new FormINFO(e.playername, FormType.Model, FormTag.confirmedRecycle)
                                            {
                                                title = lang.confirmRecycleTitle,
                                                content = string.Format(lang.confirmRecycleContent, receForm.domain.Value<string>("name"), count, total),
                                                buttons = new JArray { lang.confirmRecycleAccept, lang.confirmRecycleCancel },
                                                domain = new JObject { new JProperty("item", receForm.domain), new JProperty("count", count), new JProperty("total", total), },
                                                domain_source = (JArray)receForm.domain.Parent
                                            });
                                        }
                                        else { Feedback(e.playername, lang.InputSellDetailValueInvalid); }
                                    }
                                    catch (Exception err) { WriteLineERR("InputRecycleDetail", err.ToString()); }
                                    break;
                                case FormTag.confirmedSell:
                                    try
                                    {
                                        if (e.selected == "true")
                                        {
                                            JObject item = (JObject)receForm.domain["item"];
                                            int total = receForm.domain.Value<int>("total");
                                            int count = receForm.domain.Value<int>("count");
                                            ExecuteCMD(e.playername, $"tag @s[scores={{money=..{total}}}] remove buy_success");
                                            ExecuteCMD(e.playername, $"tag @s[scores={{money={total}..}}] add buy_success");
                                            ExecuteCMD(e.playername, "titleraw @s times 5 25 10");
                                            //success
                                            ExecuteCMD(e.playername, $"scoreboard players remove @s[tag=buy_success] money {total}");
                                            ExecuteCMD(e.playername, "titleraw @s[tag=buy_success] title {\"rawtext\":[{\"text\":\"" + StringToUnicode(lang.buySuccessfullyTitle) + "\"}]}");
                                            ExecuteCMD(e.playername, $"titleraw @s[tag=buy_success] subtitle {{\"rawtext\":[{{\"text\":\"{StringToUnicode(string.Format(lang.buySuccessfullySubtitle, total, count, item.Value<string>("name")))}\"}}]}}");
                                            //fail
                                            ExecuteCMD(e.playername, "titleraw @s[tag=!buy_success] title {\"rawtext\":[{\"text\":\"" + StringToUnicode(lang.buyFailedTitle) + "\"}]}");
                                            ExecuteCMD(e.playername, $"titleraw @s[tag=!buy_success] subtitle {{\"rawtext\":[{{\"text\":\"{StringToUnicode(string.Format(lang.buyFailedSubtitle, total, count, item.Value<string>("name")))}\"}}]}}");
                                            //-END
                                            //send
                                            while (count > 0)
                                            {
                                                int per_count = Math.Min(36, count);
                                                count -= per_count;
                                                ExecuteCMD(e.playername, $"give @s[tag=buy_success] {item.Value<string>("id")} {per_count} {item.Value<string>("damage")}");
                                            }
                                        }
                                        else
                                        {
                                            Feedback(e.playername, lang.confirmSellCanceled);
                                        }
                                    }
                                    catch (Exception err) { WriteLineERR("confirmedSell", err.ToString()); }
                                    break;
                                case FormTag.confirmedRecycle:
                                    try
                                    {
                                        if (e.selected == "true")
                                        {
                                            JObject item = receForm.domain.Value<JObject>("item");
                                            int total = receForm.domain.Value<int>("total");
                                            int count = receForm.domain.Value<int>("count");
                                            //WriteLine("-------TEST------");
                                            //string uuid = GetUUID(e.playername);
                                            //// internal static string GetUUID(string name) => JArray.Parse(api.getOnLinePlayers()).First(l => l.Value<string>("playername") == name).Value<string>("uuid");
                                            //WriteLine(uuid);
                                            //WriteLine(api.getPlayerItems(uuid));
                                            //WriteLine("-------TEST------");
                                            //File.WriteAllText("plugins\\pfshop\\test.json", ); 
                                            string getItemsRaw = api.getPlayerItems(GetUUID(e.playername));
                                            if (string.IsNullOrEmpty(getItemsRaw))
                                            {
                                                WriteLineERR(lang.recycleGetItemApiFailed, lang.recycleGetItemApiFailedDetail);
                                                Feedback(e.playername, lang.recycleGetItemApiFailedDetail);
                                            }
                                            else
                                            {
                                                JArray inventory = (JArray)JObject.Parse(api.getPlayerItems(GetUUID(e.playername)))["Inventory"]["tv"];
#if DEBUG
                                                File.WriteAllText("plugins\\pfshop\\test.json", inventory.ToString());
#endif
                                                int totalcount = 0;
                                                foreach (JObject slotbase in inventory)
                                                {
                                                    try
                                                    {
                                                        var slot = slotbase["tv"].ToList();
                                                        int name_i = slot.FindIndex(l => l["ck"].ToString() == "Name");
                                                        string get_item_name = slot[name_i]["cv"]["tv"].ToString();
                                                        if (get_item_name == item["id"].ToString() || get_item_name == ("minecraft:" + item["id"]))
                                                        {
                                                            bool block_matched = true;
                                                            if (item["damage"].ToString() != "-1" && item.ContainsKey("regex"))
                                                            {
#if DEBUG
                                                                WriteLine(slotbase["tv"].ToString(Newtonsoft.Json.Formatting.None));
#endif
                                                                block_matched = Regex.IsMatch(slotbase["tv"].ToString(Newtonsoft.Json.Formatting.None), item["regex"].ToString());
                                                            }
                                                            if (block_matched)
                                                            {
                                                                int count_i = slot.FindIndex(l => l["ck"].ToString() == "Count");
                                                                totalcount += int.Parse(slot[count_i]["cv"]["tv"].ToString());
                                                            }
                                                        }
                                                    }
                                                    catch (Exception) { }
                                                }
                                                ExecuteCMD(e.playername, "titleraw @s times 5 25 10");
                                                if (totalcount >= count)
                                                {
                                                    ExecuteCMD(e.playername, $"clear @s {item["id"]} {item["damage"]} {count}");
                                                    ExecuteCMD(e.playername, "scoreboard players add @s money " + total);
                                                    ExecuteCMD(e.playername, "titleraw @s title {\"rawtext\":[{\"text\":\"" + StringToUnicode(lang.recycleSuccessfullyTitle) + "\"}]}");
                                                    ExecuteCMD(e.playername, $"titleraw @s subtitle {{\"rawtext\":[{{\"text\":\"{StringToUnicode(string.Format(lang.recycleSuccessfullySubtitle, total, count, item.Value<string>("name")))}\"}}]}}");
                                                }
                                                else
                                                {
                                                    ExecuteCMD(e.playername, "titleraw @s title {\"rawtext\":[{\"text\":\"" + StringToUnicode(lang.recycleFailedTitle) + "\"}]}");
                                                    ExecuteCMD(e.playername, $"titleraw @s subtitle {{\"rawtext\":[{{\"text\":\"{StringToUnicode(string.Format(lang.recycleFailedSubtitle, total, count, item.Value<string>("name"), totalcount))}\"}}]}}");
                                                }
                                            }
                                        }
                                        else { Feedback(e.playername, lang.confirmRecycleCanceled); }
                                    }
                                    catch (Exception err) { WriteLineERR("confirmedRecycle", err.ToString()); }
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
                                        Feedback(e.playername, lang.preferenceSaved);
                                    }
                                    catch (Exception err) { WriteLineERR("preferenceMain", err.ToString()); }
                                    break;
                                default:
                                    break;
                            }
                        }
                    }
                    catch (Exception err)
                    {
                        WriteLineERR("EVENT-onFormSelect", err.Message);
                        return true;
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
                            switch (e.cmd.ToLower())
                            {
                                case "pf":
                                    {
                                        ShowSettingWindow();
                                        return false;
                                    }
                                case "shop reload":
                                    {
#if FromUrl
                                        SetFromUrl("https://gitee.com/Pixel_Faramita/webAPI/raw/master/survival/shop.txt");
#else
                                        shopdata = JObject.Parse(File.ReadAllText(shopdataPath));
#endif
                                        WriteLine("商店配置重新读取成功成功");
                                        return false;
                                    }
                            }
                        }
                    }
                    catch (Exception err) { WriteLineERR("EVENT-onServerCmd", err.Message); }
                    return true;
                });
                #endregion
                #region 服务器指令
                // 输入指令监听
                api.setCommandDescribeEx("shop", "§r§ePixelFaramitaSHOP商店插件主菜单", MCCSAPI.CommandPermissionLevel.Any, 0x40, 1);
                api.setCommandDescribeEx("shop reload", "§r§e商店信息重载", MCCSAPI.CommandPermissionLevel.GameMasters, 0x40, 1);
                //api.setCommandDescribeEx("shopi", "商店插件详细信息", MCCSAPI.CommandPermissionLevel.Admin, 0, 0);
                base_api.addBeforeActListener(EventKey.onInputCommand, x =>
                {
                    try
                    {
                        //Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                        var e = BaseEvent.getFrom(x) as InputCommandEvent;
                        if (e != null)
                        {
                            switch (e.cmd.Substring(1).Trim())
                            {
                                case "shop":
                                    if (!preference.ContainsKey(e.playername))
                                    {
                                        preference.Add(e.playername, JObject.Parse("{\"sell\":{\"input_type\":0,\"slide_size_i\":1,\"slide_size\":16},\"recycle\":{\"input_type\":0,\"slide_size_i\":3,\"slide_size\":64}}"));
                                        SavePreference();
                                    }
                                    SendMain(e.playername);
                                    return false;
                                case "shop reload":
                                    string PermissionRaw = api.getPlayerPermissionAndGametype(GetUUID(e.playername));
                                    if (string.IsNullOrEmpty(PermissionRaw)) return true;
                                    JObject permission = JObject.Parse(PermissionRaw);
                                    if (permission.Value<int>("permission") > 1)
                                    {
#if FromUrl
                                        SetFromUrl("https://gitee.com/Pixel_Faramita/webAPI/raw/master/survival/shop.txt");
#else
                                        shopdata = JObject.Parse(File.ReadAllText(shopdataPath));
#endif
                                        Feedback(e.playername, "商店配置重新读取成功成功");
                                    }
                                    else
                                        Feedback(e.playername, "无权限执行该命令");
                                    return false;
                                default:
                                    break;
                            }
                            //Console.WriteLine(" <{0}> {1}", e.playername, e.cmd);
                        }
                    }
                    catch (Exception err) { WriteLineERR("EVENT-onInputCommand", err.Message); }
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
                WriteLineERR("插件遇到严重错误，无法继续运行", err.Message);
            }
        }
    }
}

namespace CSR
{
    partial class Plugin
    {
        internal static Thread pluginThread = null;
        internal static void SetupPluginThread(MCCSAPI api)
        {
            pluginThread = new Thread(() =>
            {
                try
                {
                    Program.Init(api);
                }
                catch (Exception err)
                {
                    Console.WriteLine("[PFSHOP崩了]（10s后自动重载...）\n错误信息:" + err.ToString());
                    _ = Task.Run(() =>
                    {
                        Thread.Sleep(10000);
                        SetupPluginThread(api);
                    });
                }
            });
            pluginThread.SetApartmentState(ApartmentState.STA);
            pluginThread.Start();
        }
        internal static void onStart(MCCSAPI api) { SetupPluginThread(api); }
    }
}