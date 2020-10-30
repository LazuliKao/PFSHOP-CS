'#define FromUrl
Imports System
Imports System.Collections.Generic
Imports System.Text
Imports System.Threading.Tasks
Imports ManageWindow
Imports System.Threading
Imports Newtonsoft.Json.Linq
Imports System.Linq
Imports System.IO
Imports System.Text.RegularExpressions
Imports PFShop
Imports PFShop.FormINFO
Imports Ookii.Dialogs.Wpf
Imports System.Diagnostics
Imports Timer = System.Timers.Timer
Imports System.Windows.Threading
'using PFShop;

Namespace PFShop
    Friend Class Program
        Private Shared api As CSR.MCCSAPI = DirectCast(Nothing, CSR.MCCSAPI)
#Region "console"
        Friend Shared Sub WriteLine(ByVal content As Object)
            Console.Write($"[{Date.Now:yyyy-MM-dd HH:mm:ss} ")
            Console.ForegroundColor = ConsoleColor.Yellow
            Console.Write("PFSHOP")
            Console.ForegroundColor = defaultForegroundColor
            Console.Write("]")
            Console.ForegroundColor = ConsoleColor.Cyan
            Console.Write("[Main] ")
            ResetConsoleColor()
            Console.WriteLine(content)
        End Sub

        Friend Shared Sub WriteLineERR(ByVal type As Object, ByVal content As Object)
            Console.Write($"[{Date.Now:yyyy-MM-dd HH:mm:ss} ")
            Console.ForegroundColor = ConsoleColor.Yellow
            Console.Write("PFSHOP")
            Console.ForegroundColor = defaultForegroundColor
            Console.Write("]")
            Console.ForegroundColor = ConsoleColor.Red
            Console.Write("[ERROR] ")
            Console.BackgroundColor = ConsoleColor.DarkRed
            Console.ForegroundColor = ConsoleColor.White
            Console.Write($">{type}<")
            ResetConsoleColor()
            Console.ForegroundColor = ConsoleColor.Red
            Console.WriteLine(content)
            ResetConsoleColor()
        End Sub

        Private Shared defaultForegroundColor As ConsoleColor = ConsoleColor.White
        Private Shared defaultBackgroundColor As ConsoleColor = ConsoleColor.Black

        Private Shared Sub ResetConsoleColor()
            Console.ForegroundColor = defaultForegroundColor
            Console.BackgroundColor = defaultBackgroundColor
        End Sub
#End Region
        '{
        '    var tcs = new TaskCompletionSource<T>();
        '    var thread = new Thread(() =>
        '    {
        '        try
        '        {
        '            tcs.SetResult(func());
        '        }
        '        catch (Exception e)
        '        {
        '            tcs.SetException(e);
        '        }
        '    });
        '    thread.SetApartmentState(ApartmentState.STA);
        '    thread.Start();
        '    return tcs.Task;
        '} 
        'private static Task windowTask = null;
        '        private static Thread windowthread = null;
        '        private static ManualResetEvent manualResetEvent = null;
        '        private static bool windowOpened = false;
        '        private static void StartWindowThread()
        '        {
        '            try
        '            {
        '                WriteLine("正在加载WPF库");
        '                while (true)
        '                {
        '                    try
        '                    {
        '                        windowOpened = true;
        '                        new MainWindow().ShowDialog();
        '                        windowOpened = false;
        '                        manualResetEvent = new ManualResetEvent(false);
        '                        GC.Collect();
        '#if DEBUG
        '                        WriteLine("窗体线程manualResetEvent返回:" +
        '#endif
        '                        manualResetEvent.WaitOne()
        '#if DEBUG
        '                                    )
        '#endif
        '                                    ;
        '                        manualResetEvent.Reset();
        '                    }
        '                    catch (Exception err) { WriteLine("窗体执行过程中发生错误\n信息" + err.ToString()); }
        '                }
        '            }
        '            catch (Exception err) { WriteLine("窗体线程发生严重错误\n信息" + err.ToString()); windowthread = null; }
        '        }
        '        private static void ShowSettingWindow()
        '        {
        '            try
        '            {
        '                if (windowthread == null || (!windowthread.IsAlive))
        '                {
        '                    windowthread = new Thread(StartWindowThread);
        '                    windowthread.SetApartmentState(ApartmentState.STA);
        '                    windowthread.Start();
        '                }
        '                else
        '                { if (windowOpened) WriteLine("窗体已经打开"); else manualResetEvent.Set(); }
        '            }
        '            catch (Exception
        '#if DEBUG
        '            err
        '#endif
        '            )
        '            {
        '#if DEBUG
        '                WriteLine(err.ToString());
        '#endif
        '            }
        '        }
        'private static Thread WindowThread = null;
        Private Shared WindowDispatcher As Dispatcher = Nothing
        Private Shared windowOpened As Boolean = False

        Private Shared Sub ShowSettingWindow()
            If windowOpened Then
                WriteLine("窗体已经打开")
            Else
#Region "Method1"
                If Thread.CurrentThread.GetApartmentState() <> ApartmentState.STA Then
                    Thread.CurrentThread.SetApartmentState(ApartmentState.STA)
                    WriteLine("为加载UI,已设置当前运行线程为" & Thread.CurrentThread.GetApartmentState() & "线程")
                End If

                Try
                    WriteLine("正在打开窗体")
                    Console.Beep()
                    Dim height = Console.WindowHeight
                    Dim width = Console.WindowWidth
                    windowOpened = True
                    Call New MainWindow().ShowDialog()
                    GC.Collect()
                    Console.SetWindowSize(width, height)
                    windowOpened = False
                Catch err As Exception
                    Program.WriteLine("窗体线程发生严重错误" & Microsoft.VisualBasic.Constants.vbLf & "信息" & err.ToString())
                    windowOpened = False
                End Try

                Try

                    If WindowDispatcher Is Nothing Then
                        Dim WindowThread As Thread = New Thread(Sub()
                                                                End Sub)
                        WindowThread.SetApartmentState(ApartmentState.STA)
                        WriteLine("为加载UI,已设置当前插件运行线程为" & WindowThread.GetApartmentState() & "线程")
                        WindowThread.Start()
                        WindowDispatcher = Dispatcher.FromThread(WindowThread)
                        'WindowDispatcher.Thread.Start();
                    End If

                Catch errr As Exception
                    WriteLineERR("", errr)
                End Try

#End Region
#Region "Method2"
                'if (Thread.CurrentThread.GetApartmentState() != ApartmentState.STA)
                '{
                '    Thread.CurrentThread.SetApartmentState(ApartmentState.STA);
                '    WriteLine("为加载UI,已设置当前运行线程为" + Thread.CurrentThread.GetApartmentState() + "线程");
                '}
                'try
                '{
                '    Dispatcher.CurrentDispatcher.Invoke((Action)delegate
                '    {
                '        try
                '        {
                '            WriteLine("正在打开窗体");
                '            Console.Beep();
                '            var height = Console.WindowHeight;
                '            var width = Console.WindowWidth;
                '            windowOpened = true;
                '            new MainWindow().ShowDialog();
                '            GC.Collect();
                '            Console.SetWindowSize(width, height);
                '            windowOpened = false;
                '        }
                '        catch (Exception err) { WriteLine("窗体线程发生严重错误\n信息" + err.ToString()); windowOpened = false; }
                '    }, DispatcherPriority.Send);
                '}
                'catch (Exception err) { WriteLine("获取窗体线程时发生故障\n信息" + err.ToString()); windowOpened = false; }
#End Region
            End If
        End Sub
#Region "API方法补充"
        Friend Shared Function GetUUID(ByVal name As String) As String
            Return JArray.Parse(api.getOnLinePlayers()).First(Function(l) Equals(l.Value(Of String)("playername"), name)).Value(Of String)("uuid")
        End Function

        Friend Shared Sub Feedback(ByVal name As String, ByVal text As String)
            ExecuteCMD(name, $"tellraw @s {{""rawtext"":[{{""text"":""§e§l[PFSHOP]§r§b{StringToUnicode(text)}""}}]}}")
        End Sub

        Friend Shared Function StringToUnicode(ByVal s As String) As String '字符串转UNICODE代码
            Dim charbuffers As Char() = s.ToCharArray()
            Dim buffer As Byte()
            Dim sb As StringBuilder = New StringBuilder()

            For i = 0 To charbuffers.Length - 1
                buffer = Encoding.Unicode.GetBytes(charbuffers(i).ToString())
                sb.Append(String.Format("\u{0:X2}{1:X2}", buffer(1), buffer(0)))
            Next

            Return sb.ToString()
        End Function
#Region "CMD"
        Private Shared cmdCount As Integer = 0
        Friend Shared ServerCmdOutputTimer As Timer = New Timer(3000)

        Private Shared Sub ServerCmdOutputTimer_Elapsed(ByVal sender As Object, ByVal e As Timers.ElapsedEventArgs)
#If DEBUG
            WriteLine(cmdCount);
#End If
            ServerCmdOutputDetect()
        End Sub

        Friend Shared Function ServerCmdOutputDetect() As Boolean
            If cmdCount > 0 Then
                DqCmd()
                Return False
            End If

            Return True
        End Function

        Friend Shared Function ServerCmdOutputDetect(ByVal e As CSR.Events) As Boolean
            Try
                Return ServerCmdOutputDetect()
            Catch __unusedException1__ As Exception
                Return True
            End Try
        End Function

        Private Shared Sub EqCmd()
            If cmdCount = 0 Then
                api.addBeforeActListener(CSR.EventKey.onServerCmdOutput, New CSR.MCCSAPI.EventCab(AddressOf Program.ServerCmdOutputDetect))
                If Not ServerCmdOutputTimer.Enabled Then ServerCmdOutputTimer.Start()
            End If

            cmdCount += 1
#If DEBUG
            WriteLine("EQ" + cmdCount);
#End If
        End Sub

        Private Shared Sub DqCmd()
            cmdCount -= 1

            If cmdCount = 0 Then
                api.removeBeforeActListener(CSR.EventKey.onServerCmdOutput, New CSR.MCCSAPI.EventCab(AddressOf Program.ServerCmdOutputDetect))
                If ServerCmdOutputTimer.Enabled Then ServerCmdOutputTimer.Stop()
            End If
#If DEBUG
            WriteLine("DQ" + cmdCount);
#End If
        End Sub

        Friend Shared Sub ExecuteCMD(ByVal name As String, ByVal cmd As String)
            Try
                If Not (cmd.StartsWith("say") OrElse cmd.StartsWith("tellraw")) Then EqCmd()
            Catch __unusedException1__ As Exception
            End Try

            api.runcmd($"execute ""{name}"" ~~~ {cmd}")
        End Sub
#End Region
#End Region
#Region "表单方法"
        Friend Shared Sub LoadFormTip(ByVal playername As String)
            ExecuteCMD(playername, "titleraw @s times 0 20 10")
            'ExecuteCMD(playername, "titleraw @s title {\"rawtext\":[{\"text\":\"\n\n\n\n\"}]}");
            ExecuteCMD(playername, "titleraw @s title {""rawtext"":[{""text"":""§a   Loading    §a""}]}")
            __ = Task.Run(Sub()
                              Thread.Sleep(100)
                              ExecuteCMD(playername, "titleraw @s title {""rawtext"":[{""text"":""§a   Loading..   §a""}]}")
                              Thread.Sleep(100)
                              ExecuteCMD(playername, "titleraw @s title {""rawtext"":[{""text"":""§a   Loading....  §a""}]}")
                              Thread.Sleep(100)
                              ExecuteCMD(playername, "titleraw @s title {""rawtext"":[{""text"":""§a   Loading...... §a""}]}")
                              Thread.Sleep(100)
                              ExecuteCMD(playername, "titleraw @s clear")
                          End Sub)
        End Sub

        Friend Shared FormQueue As List(Of PFShop.FormINFO) = New List(Of PFShop.FormINFO)()

        Friend Shared Sub SendForm(ByVal form As PFShop.FormINFO)
            Try
                Program.LoadFormTip(form.playername)
                Task.Run(Sub()
                             Try
                                 Thread.Sleep(250)

                                 Select Case form.Type
                                     Case PFShop.FormINFO.FormType.Simple
                                         'WriteLine(form.buttons.ToString()); 
                                         form.id = api.sendSimpleForm(form.playeruuid, form.title, form.content.ToString(), form.buttons.ToString())
                                     Case PFShop.FormINFO.FormType.SimpleIMG
                                         Dim buttons As JArray = New JArray()

                                         For Each btsou As JObject In form.buttons
                                             Dim button As JObject = New JObject() From {
                                                 New JProperty("text", btsou.Value(Of String)("text"))
                                             }

                                             If btsou.ContainsKey("image") Then
                                                 If btsou(CStr("image")).Type = JTokenType.String Then
                                                     button.Add("image", New JObject From {
                                                         New JProperty("type", If(Regex.IsMatch(btsou.Value(Of String)("image"), "[a-zA-Z0-9][-a-zA-Z0-9]{0,62}(\.[a-zA-Z0-9][-a-zA-Z0-9]{0,62})+\.?"), "url", "path")),
                                                         New JProperty("data", btsou.Value(Of String)("image"))
                                                     })
                                                 End If
                                             End If

                                             buttons.Add(button)
                                         Next

                                         Dim content = New JObject() From {
                                             New JProperty("content", form.content.ToString()),
                                             New JProperty("type", "form"),
                                             New JProperty("title", form.title),
                                             New JProperty("buttons", buttons)
                                         }
                                         form.id = api.sendCustomForm(form.playeruuid, content.ToString())
                                     Case PFShop.FormINFO.FormType.Custom
                                         form.id = api.sendCustomForm(form.playeruuid, New JObject From {
                                             New JProperty(CStr("type"), CObj("custom_form")),
                                             New JProperty(CStr("title"), CObj(form.title)),
                                             New JProperty(CStr("content"), CObj(form.content))
                                         }.ToString())
                                     Case PFShop.FormINFO.FormType.Model
                                         form.id = api.sendModalForm(form.playeruuid, form.title, form.content.ToString(), form.buttons(CInt(0)).ToString(), form.buttons(CInt(1)).ToString())
                                     Case Else
                                 End Select

                                 If form.id <> 0 Then
                                     FormQueue.Add(form)
                                 Else
                                     WriteLine("表单发送失败!")
                                 End If

                             Catch err As Exception
                                 Program.WriteLine("表单发送失败!" & Microsoft.VisualBasic.Constants.vbLf & err.ToString())
                             End Try
                         End Sub)
            Catch err As Exception
                Program.WriteLine("表单发送失败!" & Microsoft.VisualBasic.Constants.vbLf & err.ToString())
            End Try
        End Sub
#Region "序列化信息"
        Friend Shared Function GetButton(ByVal text As String, ByVal image As String) As JObject
            Return New JObject From {
                New JProperty("text", text),
                New JProperty("image", image)
            }
        End Function

        Friend Shared Function GetButtonRaw(ByVal text As String) As JObject
            Return New JObject From {
                New JProperty("text", text)
            }
        End Function

        Friend Shared GetSell As Func(Of JArray, JArray) = Function(items)
                                                               Dim [get] As JArray = New JArray() From {
                                                                   Program.GetButtonRaw(lang.sellPreviousList)
                                                               }

                                                               For Each item As JObject In items
                                                                   'WriteLine(GetButton($"#{item.Value<string>("order")}§l{item.Value<string>("name")}\n{(item.Value<decimal>("price") % 1 > 0 ? item["price"].ToString() + '+' : Math.Round(item.Value<decimal>("price")).ToString())}像素币/个"
                                                                   '    , item.Value<string>("image")));
                                                                   If item.ContainsKey("type") Then
                                                                       [get].Add(Program.GetButton(String.Format(lang.sellSubList, item.Value(Of String)("type")), item.Value(Of String)("image")))
                                                                   Else
                                                                       Dim price As Decimal = 0

                                                                       Try
                                                                           price = item.Value(Of Decimal)("price")
                                                                       Catch __unusedException1__ As Exception
                                                                       End Try

                                                                       [get].Add(Program.GetButton(String.Format(lang.sellListItem, item.Value(Of String)("order"), item.Value(Of String)("name"), If(price Mod 1 > 0, price.ToString() & "+"c, Math.Round(CDec(price)).ToString())), item.Value(Of String)("image")))
                                                                   End If
                                                               Next
                                                               'WriteLine(get.ToString(Newtonsoft.Json.Formatting.None));
                                                               Return [get]
                                                           End Function

        Friend Shared GetRecycle As Func(Of JArray, JArray) = Function(items)
                                                                  Dim [get] As JArray = New JArray() From {
                                                                      Program.GetButtonRaw(lang.recyclePreviousList)
                                                                  }

                                                                  For Each item As JObject In items

                                                                      If item.ContainsKey("type") Then
                                                                          [get].Add(Program.GetButton(String.Format(lang.recycleSubList, item.Value(Of String)("type")), item.Value(Of String)("image")))
                                                                      Else
                                                                          Dim award As Decimal = 0

                                                                          Try
                                                                              award = item.Value(Of Decimal)("award")
                                                                          Catch __unusedException1__ As Exception
                                                                          End Try

                                                                          [get].Add(Program.GetButton(String.Format(lang.recycleListItem, item.Value(Of String)("order"), item.Value(Of String)("name"), If(award Mod 1 > 0, award.ToString() & "-"c, Math.Round(CDec(award)).ToString())), item.Value(Of String)("image")))
                                                                      End If
                                                                  Next

                                                                  Return [get]
                                                              End Function
#End Region
#Region "方便调用的方法"
        Friend Shared Sub SendMain(ByVal playername As String)
            Program.SendForm(New PFShop.FormINFO(playername, PFShop.FormINFO.FormType.Simple, PFShop.FormINFO.FormTag.Main) With {
                .title = lang.ShopMainTitle,
                .content = lang.ShopMainContent,
                .buttons = New JArray From {
                    lang.ShopMainSell,
                    lang.ShopMainRecycle,
                    lang.ShopMainPref
                }
            })
        End Sub
#End Region
#End Region
#Region "配置"
        '''插件设置
        'internal static
        '商店信息
        Friend Shared shopdata As JObject = New JObject()
        Friend Shared shopdataPath As String = Path.GetFullPath("plugins\pfshop\shopdata.json")

        Friend Shared Sub SaveShopdata()
            File.WriteAllText(shopdataPath, shopdata.ToString())
        End Sub
        '偏好设定
        Friend Shared preference As JObject = New JObject()
        Friend Shared preferencePath As String = Path.GetFullPath("plugins\pfshop\preference.json")

        Friend Shared Sub SavePreference()
            File.WriteAllText(preferencePath, preference.ToString())
        End Sub
#End Region
#Region "URLParse"
#If FromUrl
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
#Region "回收 "
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
#End Region
#Region "购买"
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
#End Region

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
#End If
#End Region
        '语言文件
        Private Shared lang As PFShop.Language = New PFShop.Language()

        Friend Shared Sub Init(ByVal base_api As CSR.MCCSAPI)
            AddHandler ServerCmdOutputTimer.Elapsed, AddressOf ServerCmdOutputTimer_Elapsed
            'WriteLine("test");
            __ = Task.Run(Sub()
                              Thread.Sleep(11000)
                              api.runcmd("scoreboard objectives add money dummy §bMoney")
#If DEBUG
                //for (int i = 0; i < 20; i++)
                //{
                //    api.runcmd("say test");
                //    EqCmd();
                //}
#End If
                              EqCmd()
                          End Sub)

            Try
#If DEBUG
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
#End If
#Region "加载"
                api = base_api
                Console.OutputEncoding = Encoding.UTF8
                defaultForegroundColor = Console.ForegroundColor
                defaultBackgroundColor = Console.BackgroundColor
#Region "INFO"
                Console.ForegroundColor = ConsoleColor.Yellow
                Console.BackgroundColor = ConsoleColor.Magenta

                Try
                    Dim authorsInfo As String() = New String() {"███████████████████████████", "正在裝載PFSHOP", "作者           gxh2004", "版本信息    v" & Assembly.GetExecutingAssembly().GetName().Version.ToString(), "适用于bds1.16(CSRV0.1.16.40.2v3编译)", "如版本不同可能存在问题", "基於C#+WPF窗體", "当前CSRunnerAPI版本:" & api.VERSION, "控制台輸入""pf""即可打開快速配置窗體(未完善)", "███████████████████████████"}
                    Dim GetLength As Func(Of String, Integer) = Function(input) Encoding.GetEncoding("GBK").GetByteCount(input)
                    Dim infoLength = 0

                    For Each line In authorsInfo
                        infoLength = Math.Max(infoLength, GetLength(line))
                    Next

                    For i = 0 To authorsInfo.Length - 1

                        While GetLength(authorsInfo(i)) < infoLength
                            authorsInfo(i) += " "
                        End While

                        Console.WriteLine("█" & authorsInfo(i) & "█")
                    Next

                Catch __unusedException1__ As Exception
                End Try

                ResetConsoleColor()
#End Region
#Region "读取配置"
                '语言文件
                Try
                    Dim languagePath = Path.GetFullPath("plugins\pfshop\lang.json")
                    'JObject language = new JObject();
                    If Not Directory.Exists(Path.GetDirectoryName(shopdataPath)) Then Directory.CreateDirectory(Path.GetDirectoryName(shopdataPath))

                    If File.Exists(languagePath) Then
                        'lang = JObject.Parse(File.ReadAllText(languagePath)).ToObject<Language>();
                        lang = Newtonsoft.Json.JsonConvert.DeserializeObject(Of PFShop.Language)(File.ReadAllText(languagePath))
                    Else
                        Program.WriteLineERR(lang.CantFindLanguage, String.Format(lang.SaveDefaultLanguageTo, languagePath))
                    End If

                    File.WriteAllText(languagePath, JObject.FromObject(CObj(lang)).ToString())
                Catch err As Exception
                    Program.WriteLineERR("Lang", String.Format(lang.LanguageFileLoadFailed, err.ToString()))
                End Try
#If Not DEBUG
#Region "EULA "
                Dim height = 0, width = 0
                Dim title As String = Nothing

                Try
                    height = Console.WindowHeight
                    width = Console.WindowWidth
                    title = Console.Title
                Catch __unusedException1__ As Exception
                End Try
                'set
                Dispatcher.CurrentDispatcher.Invoke(Sub()
                                                        Try
                                                            If Not Directory.Exists(Path.GetDirectoryName(shopdataPath)) Then Directory.CreateDirectory(Path.GetDirectoryName(shopdataPath))
                                                            Dim eulaPath = Path.GetDirectoryName(shopdataPath) & "\EULA"
                                                            Dim version As String = Assembly.GetExecutingAssembly().GetName().Version.ToString()
                                                            Dim eulaINFO As JObject = New JObject From {
                                                                New JProperty("author", "gxh"),
                                                                New JProperty("version", version)
                                                            }

                                                            Try

                                                                If File.Exists(eulaPath) Then
                                                                    If Not Equals(Encoding.UTF32.GetString(File.ReadAllBytes(eulaPath)), StringToUnicode(CStr(eulaINFO.ToString())).GetHashCode().ToString()) Then
                                                                        WriteLineERR("EULA", "使用条款需要更新!")
                                                                        File.Delete(eulaPath)
                                                                        Throw New Exception()
                                                                    End If
                                                                Else
                                                                    Throw New Exception()
                                                                End If

                                                            Catch __unusedException1__ As Exception

                                                                Try
                                                                    Console.Beep()
                                                                    Console.SetWindowSize(Console.WindowWidth, 3)
                                                                    WriteLine("请同意使用条款")
                                                                    Console.Title = "当前控制台会无法操作，请同意使用条款即可恢复"
                                                                Catch
                                                                End Try

                                                                Using dialog As TaskDialog = New TaskDialog()
                                                                    dialog.WindowTitle = "接受食用条款"
                                                                    dialog.MainInstruction = "假装下面是本插件的食用条款"
                                                                    dialog.Content = "1.请在遵守CSRunner前置使用协议的前提下使用本插件" & Microsoft.VisualBasic.Constants.vbLf & "2.不保证本插件不会影响服务器正常运行，如使用本插件造成服务端奔溃等问题，均与作者无瓜" & Microsoft.VisualBasic.Constants.vbLf & "3.严厉打击插件倒卖等行为，共同维护良好的开源环境"
                                                                    dialog.ExpandedInformation = "点开淦嘛,没东西[doge]"
                                                                    dialog.Footer = "本插件 <a href=""https://github.com/littlegao233/PFShop-CS"">GitHub开源地址</a>."
                                                                    AddHandler dialog.HyperlinkClicked, New EventHandler(Of HyperlinkClickedEventArgs)(Sub(sender, e) Process.Start("https://github.com/littlegao233/PFShop-CS"))
                                                                    dialog.FooterIcon = TaskDialogIcon.Information
                                                                    dialog.EnableHyperlinks = True
                                                                    Dim acceptButton As TaskDialogButton = New TaskDialogButton("Accept")
                                                                    dialog.Buttons.Add(acceptButton)
                                                                    Dim refuseButton As TaskDialogButton = New TaskDialogButton("拒绝并关闭本插件")
                                                                    dialog.Buttons.Add(refuseButton)
                                                                    If dialog.ShowDialog() Is refuseButton Then Throw New Exception("---尚未接受食用条款，本插件加载失败---")
                                                                End Using

                                                                File.WriteAllBytes(eulaPath, Encoding.UTF32.GetBytes(StringToUnicode(CStr(eulaINFO.ToString())).GetHashCode().ToString()))
                                                            End Try

                                                        Catch err As Exception
                                                            WriteLineERR("条款获取出错", err)
                                                        End Try
                                                    End Sub)
                'recover  
                Try

                    If Not Equals(title, Nothing) Then
                        Console.Title = title
                        Console.SetWindowSize(width, height)
                    End If

                Catch __unusedException1__ As Exception
                End Try
#End Region
#End If
                '商店信息
                Try
                    If Not Directory.Exists(Path.GetDirectoryName(shopdataPath)) Then Directory.CreateDirectory(Path.GetDirectoryName(shopdataPath))

                    If File.Exists(shopdataPath) Then
                        shopdata = JObject.Parse(File.ReadAllText(shopdataPath))
                    Else
                        shopdata = JObject.Parse(lang.DefaultShopData)
                        SaveShopdata()
                        Program.WriteLineERR(lang.CantFindConfig, String.Format(lang.SaveDefaultConfigTo, shopdataPath))
#If FromUrl
                    SetFromUrl("https://gitee.com/Pixel_Faramita/webAPI/raw/master/survival/shop.txt");
#End If
                    End If

                Catch __unusedException1__ As Exception
                    SaveShopdata()
                End Try
                '偏好设定
                Try
                    If Not Directory.Exists(Path.GetDirectoryName(preferencePath)) Then Directory.CreateDirectory(Path.GetDirectoryName(preferencePath))

                    If File.Exists(preferencePath) Then
                        preference = JObject.Parse(File.ReadAllText(preferencePath))
                    Else
                        SavePreference()
                    End If

                Catch __unusedException1__ As Exception
                    SavePreference()
                End Try
#End Region
#End Region
                ' 表单选择监听
                api.addAfterActListener(CSR.EventKey.onFormSelect, Function(x)
                                                                       Try
                                                                           Dim e = TryCast(CSR.BaseEvent.getFrom(x), CSR.FormSelectEvent)
                                                                           Dim index = FormQueue.FindIndex(Function(l) l.id = e.formid)
                                                                           If index = -1 Then Return True
                                                                           Dim receForm = FormQueue(index)
                                                                           FormQueue.RemoveAt(index)

                                                                           If Equals(e.selected, "null") Then
                                                                               If receForm.Tag = PFShop.FormINFO.FormTag.recycleMain OrElse receForm.Tag = PFShop.FormINFO.FormTag.sellMain OrElse receForm.Tag = PFShop.FormINFO.FormTag.preferenceMain Then
                                                                                   Program.SendMain(e.playername)
                                                                               ElseIf receForm.Tag = PFShop.FormINFO.FormTag.InputSellDetail Then
                                                                                   Program.SendForm(New PFShop.FormINFO(e.playername, PFShop.FormINFO.FormType.SimpleIMG, PFShop.FormINFO.FormTag.sellMain) With {
                                                                                       .title = lang.sellMainTitle,
                                                                                       .content = lang.sellMainContent,
                                                                                       .buttons = GetSell(CType(receForm.domain_source, JArray)),
                                                                                       .domain = receForm.domain.Parent.Parent.Parent
                                                                                   })
                                                                               ElseIf receForm.Tag = PFShop.FormINFO.FormTag.InputRecycleDetail Then
                                                                                   Program.SendForm(New PFShop.FormINFO(e.playername, PFShop.FormINFO.FormType.SimpleIMG, PFShop.FormINFO.FormTag.recycleMain) With {
                                                                                       .title = lang.recycleMainTitle,
                                                                                       .content = lang.recycleMainContent,
                                                                                       .buttons = GetRecycle(CType(receForm.domain_source, JArray)),
                                                                                       .domain = receForm.domain.Parent.Parent.Parent
                                                                                   })
                                                                               Else
                                                                                   Program.Feedback(e.playername, lang.ClosedFormWithoutAction)
                                                                               End If
                                                                           Else

                                                                               Select Case receForm.Tag
                                                                                   Case PFShop.FormINFO.FormTag.Main

                                                                                       If Equals(e.selected, "0") Then
                                                                                           Program.SendForm(New PFShop.FormINFO(e.playername, PFShop.FormINFO.FormType.SimpleIMG, PFShop.FormINFO.FormTag.sellMain) With {
                                                                                               .title = lang.sellMainTitle,
                                                                                               .content = lang.sellMainContent,
                                                                                               .buttons = GetSell(TryCast(shopdata("sell"), JArray)),
                                                                                               .domain = shopdata("sell")
                                                                                           })
                                                                                       ElseIf Equals(e.selected, "1") Then
                                                                                           Program.SendForm(New PFShop.FormINFO(e.playername, PFShop.FormINFO.FormType.SimpleIMG, PFShop.FormINFO.FormTag.recycleMain) With {
                                                                                               .title = lang.recycleMainTitle,
                                                                                               .content = lang.recycleMainContent,
                                                                                               .buttons = GetRecycle(TryCast(shopdata("recycle"), JArray)),
                                                                                               .domain = shopdata("recycle")
                                                                                           })
                                                                                       ElseIf Equals(e.selected, "2") Then
                                                                                           Dim sizelist = New JArray() From {
                                                                                               "8",
                                                                                               "16",
                                                                                               "32",
                                                                                               "64",
                                                                                               "128",
                                                                                               "256"
                                                                                           }
                                                                                           Program.SendForm(New PFShop.FormINFO(e.playername, PFShop.FormINFO.FormType.Custom, PFShop.FormINFO.FormTag.preferenceMain) With {
                                                                                               .title = lang.preferenceMainTitle,
                                                                                               .content = New JArray From {
                                                                                                   New JObject From {
                                                                                                       New JProperty("type", "step_slider"),
                                                                                                       New JProperty("text", lang.preferenceMainSellText),
                                                                                                       New JProperty("steps", New JArray From {
                                                                                                           lang.preferenceMainUsingSlider,
                                                                                                           lang.preferenceMainUsingTextbox
                                                                                                       }),
                                                                                                       New JProperty("default", preference(e.playername)("sell").Value(Of Integer)("input_type"))
                                                                                                   },
                                                                                                   New JObject From {
                                                                                                       New JProperty("type", "dropdown"),
                                                                                                       New JProperty("text", lang.preferenceMainMaxValueTip),
                                                                                                       New JProperty("options", sizelist),
                                                                                                       New JProperty("default", preference(e.playername)("sell").Value(Of Integer)("slide_size_i"))
                                                                                                   },
                                                                                                   New JObject From {
                                                                                                       New JProperty("type", "step_slider"),
                                                                                                       New JProperty("text", lang.preferenceMainRecycleText),
                                                                                                       New JProperty("steps", New JArray From {
                                                                                                           lang.preferenceMainUsingSlider,
                                                                                                           lang.preferenceMainUsingTextbox
                                                                                                       }),
                                                                                                       New JProperty("default", preference(e.playername)("recycle").Value(Of Integer)("input_type"))
                                                                                                   },
                                                                                                   New JObject From {
                                                                                                       New JProperty("type", "dropdown"),
                                                                                                       New JProperty("text", lang.preferenceMainMaxValueTip),
                                                                                                       New JProperty("options", sizelist),
                                                                                                       New JProperty("default", preference(e.playername)("recycle").Value(Of Integer)("slide_size_i"))
                                                                                                   }
                                                                                               }
                                                                                           })
                                                                                       End If

                                                                                   Case PFShop.FormINFO.FormTag.sellMain

                                                                                       Try

                                                                                           If Equals(e.selected, "0") Then
                                                                                               If Equals(receForm.domain.Path, "sell") Then
                                                                                                   Program.SendMain(e.playername)
                                                                                               Else
                                                                                                   Program.SendForm(New PFShop.FormINFO(e.playername, PFShop.FormINFO.FormType.SimpleIMG, PFShop.FormINFO.FormTag.sellMain) With {
                                                                                                       .title = lang.sellMainTitle,
                                                                                                       .content = lang.sellMainContent,
                                                                                                       .buttons = GetSell(CType(receForm.domain.Parent.Parent.Parent, JArray)),
                                                                                                       .domain = receForm.domain.Parent.Parent.Parent
                                                                                                   })
                                                                                               End If
                                                                                           Else
                                                                                               Dim selitem As JObject = CType(receForm.domain(Integer.Parse(e.selected) - 1), JObject)

                                                                                               If selitem.ContainsKey("type") Then
                                                                                                   Program.SendForm(New PFShop.FormINFO(e.playername, PFShop.FormINFO.FormType.SimpleIMG, PFShop.FormINFO.FormTag.sellMain) With {
                                                                                                       .title = lang.sellMainTitle,
                                                                                                       .content = lang.sellMainContent,
                                                                                                       .buttons = GetSell(CType(selitem("content"), JArray)),
                                                                                                       .domain = selitem("content")
                                                                                                   })
                                                                                               Else
                                                                                                   'WriteLine("test1--" + selitem.ToString(Newtonsoft.Json.Formatting.None));
                                                                                                   Dim pre As JObject = CType(preference(e.playername)("sell"), JObject)
                                                                                                   'WriteLine("test2--" + pre.ToString(Newtonsoft.Json.Formatting.None));
                                                                                                   Dim content = New JObject()

                                                                                                   If pre.Value(Of Integer)("input_type") = 0 Then
                                                                                                       content.Add("text", String.Format(lang.InputSellDetailContentWhenUseSlider, selitem.Value(Of String)("name"), selitem.Value(Of String)("price")))
                                                                                                       content.Add("type", "slider")
                                                                                                       content.Add("min", 1)
                                                                                                       content.Add("max", pre.Value(Of Integer)("slide_size"))
                                                                                                       content.Add("step", 1)
                                                                                                       content.Add("default", 1)
                                                                                                   Else
                                                                                                       content.Add("text", String.Format(lang.InputSellDetailContentWhenUseTextbox, selitem.Value(Of String)("name"), selitem.Value(Of String)("price")))
                                                                                                       content.Add("type", "input")
                                                                                                       content.Add("default", "")
                                                                                                       content.Add("placeholder", lang.InputSellDetailContentTextboxPlaceholder)
                                                                                                   End If

                                                                                                   Program.SendForm(New PFShop.FormINFO(e.playername, PFShop.FormINFO.FormType.Custom, PFShop.FormINFO.FormTag.InputSellDetail) With {
                                                                                                       .title = lang.InputSellDetailTitle,
                                                                                                       .content = New JArray From {
                                                                                                           content
                                                                                                       },
                                                                                                       .domain = selitem
                                                                                                   })
                                                                                               End If
                                                                                           End If

                                                                                       Catch err As Exception
                                                                                           WriteLineERR("sellMain", err.ToString())
                                                                                       End Try

                                                                                   Case PFShop.FormINFO.FormTag.recycleMain

                                                                                       Try

                                                                                           If Equals(e.selected, "0") Then
                                                                                               If Equals(receForm.domain.Path, "recycle") Then
                                                                                                   Program.SendMain(e.playername)
                                                                                               Else
                                                                                                   Program.SendForm(New PFShop.FormINFO(e.playername, PFShop.FormINFO.FormType.SimpleIMG, PFShop.FormINFO.FormTag.recycleMain) With {
                                                                                                       .title = lang.recycleMainTitle,
                                                                                                       .content = lang.recycleMainContent,
                                                                                                       .buttons = GetRecycle(CType(receForm.domain.Parent.Parent.Parent, JArray)),
                                                                                                       .domain = receForm.domain.Parent.Parent.Parent
                                                                                                   })
                                                                                               End If
                                                                                           Else
                                                                                               Dim selitem As JObject = CType(receForm.domain(Integer.Parse(e.selected) - 1), JObject)

                                                                                               If selitem.ContainsKey("type") Then
                                                                                                   Program.SendForm(New PFShop.FormINFO(e.playername, PFShop.FormINFO.FormType.SimpleIMG, PFShop.FormINFO.FormTag.recycleMain) With {
                                                                                                       .title = lang.recycleMainTitle,
                                                                                                       .content = lang.recycleMainContent,
                                                                                                       .buttons = GetRecycle(CType(selitem("content"), JArray)),
                                                                                                       .domain = selitem("content")
                                                                                                   })
                                                                                               Else
                                                                                                   Dim pre As JObject = CType(preference(e.playername)("recycle"), JObject)
                                                                                                   Dim content = New JObject()

                                                                                                   If pre.Value(Of Integer)("input_type") = 0 Then
                                                                                                       content.Add("text", String.Format(lang.InputRecycleDetailContentWhenUseSlider, selitem.Value(Of String)("name"), selitem.Value(Of String)("award")))
                                                                                                       content.Add("type", "slider")
                                                                                                       content.Add("min", 1)
                                                                                                       content.Add("max", pre.Value(Of Integer)("slide_size"))
                                                                                                       content.Add("step", 1)
                                                                                                       content.Add("default", 1)
                                                                                                   Else
                                                                                                       content.Add("text", String.Format(lang.InputRecycleDetailContentWhenUseTextbox, selitem.Value(Of String)("name"), selitem.Value(Of String)("award")))
                                                                                                       content.Add("type", "input")
                                                                                                       content.Add("default", "")
                                                                                                       content.Add("placeholder", lang.InputRecycleDetailContentTextboxPlaceholder)
                                                                                                   End If

                                                                                                   Program.SendForm(New PFShop.FormINFO(e.playername, PFShop.FormINFO.FormType.Custom, PFShop.FormINFO.FormTag.InputRecycleDetail) With {
                                                                                                       .title = lang.InputRecycleDetailTitle,
                                                                                                       .content = New JArray From {
                                                                                                           content
                                                                                                       },
                                                                                                       .domain = selitem
                                                                                                   })
                                                                                               End If
                                                                                           End If

                                                                                       Catch err As Exception
                                                                                           WriteLineERR("recycleMain", err.ToString())
                                                                                       End Try

                                                                                   Case PFShop.FormINFO.FormTag.InputSellDetail

                                                                                       Try
                                                                                           'receForm.domain;//选择项 
                                                                                           Dim count As Integer = Convert.ToInt32(JArray.Parse(e.selected)(0))
                                                                                           If count > Short.MaxValue Then Throw New Exception()
                                                                                           Dim total As Integer = CInt(Math.Ceiling(receForm.domain.Value(Of Decimal)("price") * count))

                                                                                           If total > 0 AndAlso count > 0 Then
                                                                                               Program.SendForm(New PFShop.FormINFO(e.playername, PFShop.FormINFO.FormType.Model, PFShop.FormINFO.FormTag.confirmedSell) With {
                                                                                                   .title = lang.confirmSellTitle,
                                                                                                   .content = String.Format(lang.confirmSellContent, receForm.domain.Value(Of String)("name"), count, total),
                                                                                                   .buttons = New JArray From {
                                                                                                       lang.confirmSellAccept,
                                                                                                       lang.confirmSellCancel
                                                                                                   },
                                                                                                   .domain = New JObject From {
                                                                                                       New JProperty("item", receForm.domain),
                                                                                                       New JProperty("count", count),
                                                                                                       New JProperty("total", total)
                                                                                                   },
                                                                                                   .domain_source = CType(receForm.domain.Parent, JArray)
                                                                                               })
                                                                                           Else
                                                                                               Program.Feedback(e.playername, lang.InputRecycleDetailValueInvalid)
                                                                                           End If

                                                                                       Catch err As Exception
                                                                                           WriteLineERR("InputSellDetail", err.ToString())
                                                                                       End Try

                                                                                   Case PFShop.FormINFO.FormTag.InputRecycleDetail

                                                                                       Try    'receForm.domain;//选择项
                                                                                           Dim count As Integer = Convert.ToInt32(JArray.Parse(e.selected)(0))
                                                                                           If count > Short.MaxValue Then Throw New Exception()
                                                                                           Dim total As Integer = CInt(Math.Floor(receForm.domain.Value(Of Decimal)("award") * count))

                                                                                           If total > 0 AndAlso count > 0 Then
                                                                                               Program.SendForm(New PFShop.FormINFO(e.playername, PFShop.FormINFO.FormType.Model, PFShop.FormINFO.FormTag.confirmedRecycle) With {
                                                                                                   .title = lang.confirmRecycleTitle,
                                                                                                   .content = String.Format(lang.confirmRecycleContent, receForm.domain.Value(Of String)("name"), count, total),
                                                                                                   .buttons = New JArray From {
                                                                                                       lang.confirmRecycleAccept,
                                                                                                       lang.confirmRecycleCancel
                                                                                                   },
                                                                                                   .domain = New JObject From {
                                                                                                       New JProperty("item", receForm.domain),
                                                                                                       New JProperty("count", count),
                                                                                                       New JProperty("total", total)
                                                                                                   },
                                                                                                   .domain_source = CType(receForm.domain.Parent, JArray)
                                                                                               })
                                                                                           Else
                                                                                               Program.Feedback(e.playername, lang.InputSellDetailValueInvalid)
                                                                                           End If

                                                                                       Catch err As Exception
                                                                                           WriteLineERR("InputRecycleDetail", err.ToString())
                                                                                       End Try

                                                                                   Case PFShop.FormINFO.FormTag.confirmedSell

                                                                                       Try

                                                                                           If Equals(e.selected, "true") Then
                                                                                               Dim item As JObject = CType(receForm.domain("item"), JObject)
                                                                                               Dim total As Integer = receForm.domain.Value(Of Integer)("total")
                                                                                               Dim count As Integer = receForm.domain.Value(Of Integer)("count")
                                                                                               Program.ExecuteCMD(e.playername, $"tag @s[scores={{money=..{total}}}] remove buy_success")
                                                                                               Program.ExecuteCMD(e.playername, $"tag @s[scores={{money={total}..}}] add buy_success")
                                                                                               Program.ExecuteCMD(e.playername, "titleraw @s times 5 25 10")
                                                                                               'success
                                                                                               Program.ExecuteCMD(e.playername, $"scoreboard players remove @s[tag=buy_success] money {total}")
                                                                                               Program.ExecuteCMD(e.playername, "titleraw @s[tag=buy_success] title {""rawtext"":[{""text"":""" & Program.StringToUnicode(lang.buySuccessfullyTitle) & """}]}")
                                                                                               Program.ExecuteCMD(e.playername, $"titleraw @s[tag=buy_success] subtitle {{""rawtext"":[{{""text"":""{Program.StringToUnicode(String.Format(lang.buySuccessfullySubtitle, total, count, item.Value(Of String)("name")))}""}}]}}")
                                                                                               'fail
                                                                                               Program.ExecuteCMD(e.playername, "titleraw @s[tag=!buy_success] title {""rawtext"":[{""text"":""" & Program.StringToUnicode(lang.buyFailedTitle) & """}]}")
                                                                                               Program.ExecuteCMD(e.playername, $"titleraw @s[tag=!buy_success] subtitle {{""rawtext"":[{{""text"":""{Program.StringToUnicode(String.Format(lang.buyFailedSubtitle, total, count, item.Value(Of String)("name")))}""}}]}}")
                                                                                               '-END
                                                                                               'send
                                                                                               While count > 0
                                                                                                   Dim per_count As Integer = Math.Min(36, count)
                                                                                                   count -= per_count
                                                                                                   Program.ExecuteCMD(e.playername, $"give @s[tag=buy_success] {item.Value(Of String)("id")} {per_count} {item.Value(Of String)("damage")}")
                                                                                               End While
                                                                                           Else
                                                                                               Program.Feedback(e.playername, lang.confirmSellCanceled)
                                                                                           End If

                                                                                       Catch err As Exception
                                                                                           WriteLineERR("confirmedSell", err.ToString())
                                                                                       End Try

                                                                                   Case PFShop.FormINFO.FormTag.confirmedRecycle

                                                                                       Try

                                                                                           If Equals(e.selected, "true") Then
                                                                                               Dim item As JObject = receForm.domain.Value(Of JObject)("item")
                                                                                               Dim total As Integer = receForm.domain.Value(Of Integer)("total")
                                                                                               Dim count As Integer = receForm.domain.Value(Of Integer)("count")
                                                                                               'WriteLine("-------TEST------");
                                                                                               'string uuid = GetUUID(e.playername);
                                                                                               ''' internal static string GetUUID(string name) => JArray.Parse(api.getOnLinePlayers()).First(l => l.Value<string>("playername") == name).Value<string>("uuid");
                                                                                               'WriteLine(uuid);
                                                                                               'WriteLine(api.getPlayerItems(uuid));
                                                                                               'WriteLine("-------TEST------");
                                                                                               'File.WriteAllText("plugins\\pfshop\\test.json", ); 
                                                                                               Dim getItemsRaw As String = api.getPlayerItems(Program.GetUUID(e.playername))

                                                                                               If String.IsNullOrEmpty(getItemsRaw) Then
                                                                                                   Program.WriteLineERR(lang.recycleGetItemApiFailed, lang.recycleGetItemApiFailedDetail)
                                                                                                   Program.Feedback(e.playername, lang.recycleGetItemApiFailedDetail)
                                                                                               Else
                                                                                                   Dim inventory As JArray = CType(JObject.Parse(api.getPlayerItems(Program.GetUUID(e.playername)))("Inventory")("tv"), JArray)
#If DEBUG
                                                File.WriteAllText("plugins\\pfshop\\test.json", inventory.ToString());
#End If
                                                                                                   Dim totalcount As Integer = 0

                                                                                                   For Each slotbase As JObject In inventory

                                                                                                       Try
                                                                                                           Dim slot = slotbase("tv").ToList()
                                                                                                           Dim name_i As Integer = slot.FindIndex(Function(l) Equals(l("ck").ToString(), "Name"))
                                                                                                           Dim get_item_name As String = slot(name_i)("cv")("tv").ToString()

                                                                                                           If Equals(get_item_name, item("id").ToString()) OrElse Equals(get_item_name, "minecraft:" & item("id")) Then
                                                                                                               Dim block_matched As Boolean = True

                                                                                                               If Not Equals(item("damage").ToString(), "-1") AndAlso item.ContainsKey("regex") Then
#If DEBUG
                                                                WriteLine(slotbase["tv"].ToString(Newtonsoft.Json.Formatting.None));
#End If
                                                                                                                   block_matched = Regex.IsMatch(slotbase("tv").ToString(Newtonsoft.Json.Formatting.None), item("regex").ToString())
                                                                                                               End If

                                                                                                               If block_matched Then
                                                                                                                   Dim count_i As Integer = slot.FindIndex(Function(l) Equals(l("ck").ToString(), "Count"))
                                                                                                                   totalcount += Integer.Parse(slot(count_i)("cv")("tv").ToString())
                                                                                                               End If
                                                                                                           End If

                                                                                                       Catch __unusedException1__ As Exception
                                                                                                       End Try
                                                                                                   Next

                                                                                                   Program.ExecuteCMD(e.playername, "titleraw @s times 5 25 10")

                                                                                                   If totalcount >= count Then
                                                                                                       Program.ExecuteCMD(e.playername, $"clear @s {item("id")} {item("damage")} {count}")
                                                                                                       Program.ExecuteCMD(e.playername, "scoreboard players add @s money " & total)
                                                                                                       Program.ExecuteCMD(e.playername, "titleraw @s title {""rawtext"":[{""text"":""" & Program.StringToUnicode(lang.recycleSuccessfullyTitle) & """}]}")
                                                                                                       Program.ExecuteCMD(e.playername, $"titleraw @s subtitle {{""rawtext"":[{{""text"":""{Program.StringToUnicode(String.Format(lang.recycleSuccessfullySubtitle, total, count, item.Value(Of String)("name")))}""}}]}}")
                                                                                                   Else
                                                                                                       Program.ExecuteCMD(e.playername, "titleraw @s title {""rawtext"":[{""text"":""" & Program.StringToUnicode(lang.recycleFailedTitle) & """}]}")
                                                                                                       Program.ExecuteCMD(e.playername, $"titleraw @s subtitle {{""rawtext"":[{{""text"":""{Program.StringToUnicode(String.Format(lang.recycleFailedSubtitle, total, count, item.Value(Of String)("name"), totalcount))}""}}]}}")
                                                                                                   End If
                                                                                               End If
                                                                                           Else
                                                                                               Program.Feedback(e.playername, lang.confirmRecycleCanceled)
                                                                                           End If

                                                                                       Catch err As Exception
                                                                                           WriteLineERR("confirmedRecycle", err.ToString())
                                                                                       End Try

                                                                                   Case PFShop.FormINFO.FormTag.preferenceMain

                                                                                       Try
                                                                                           Dim [set] = JArray.Parse(e.selected).ToList().ConvertAll(Function(l) Integer.Parse(l.ToString()))
                                                                                           Dim size As Integer() = New Integer() {8, 16, 32, 64, 128, 256}
                                                                                           preference(e.playername)("sell")("input_type") = [set](0)
                                                                                           preference(e.playername)("sell")("slide_size_i") = [set](1)
                                                                                           preference(e.playername)("sell")("slide_size") = size([set](1))
                                                                                           preference(e.playername)("recycle")("input_type") = [set](2)
                                                                                           preference(e.playername)("recycle")("slide_size_i") = [set](3)
                                                                                           preference(e.playername)("recycle")("slide_size") = size([set](3))
                                                                                           SavePreference()
                                                                                           Program.Feedback(e.playername, lang.preferenceSaved)
                                                                                       Catch err As Exception
                                                                                           WriteLineERR("preferenceMain", err.ToString())
                                                                                       End Try

                                                                                   Case Else
                                                                               End Select
                                                                           End If

                                                                       Catch err As Exception
                                                                           WriteLineERR("EVENT-onFormSelect", err.Message)
                                                                           Return True
                                                                       End Try

                                                                       Return False
                                                                   End Function)

#Region "事件"
#Region "控制台命令"

                base_api.addBeforeActListener(CSR.EventKey.onServerCmd, Function(x)
                                                                            Try
                                                                                'Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                                                                                Dim e = TryCast(CSR.BaseEvent.getFrom(x), CSR.ServerCmdEvent)

                                                                                If e IsNot Nothing Then
                                                                                    Select Case e.cmd.ToLower()
                                                                                        Case "pf"
                                                                                            ShowSettingWindow()
                                                                                            Return False
                                                                                        Case "shop reload"
#If FromUrl
                                        SetFromUrl("https://gitee.com/Pixel_Faramita/webAPI/raw/master/survival/shop.txt");
#Else
                                                                                            shopdata = JObject.Parse(File.ReadAllText(shopdataPath))
#End If
                                                                                            WriteLine("商店配置重新读取成功成功")
                                                                                            Return False
                                                                                    End Select
                                                                                End If

                                                                            Catch err As Exception
                                                                                WriteLineERR("EVENT-onServerCmd", err.Message)
                                                                            End Try

                                                                            Return True
                                                                        End Function)
#End Region
#Region "服务器指令"
                ' 输入指令监听
                api.setCommandDescribeEx("shop", lang.CommandMain, CSR.MCCSAPI.CommandPermissionLevel.Any, &H40, 1)
                api.setCommandDescribeEx("shop reload", lang.CommandReload, CSR.MCCSAPI.CommandPermissionLevel.GameMasters, &H40, 1)
                'api.setCommandDescribeEx("shopi", "商店插件详细信息", MCCSAPI.CommandPermissionLevel.Admin, 0, 0);
                base_api.addBeforeActListener(CSR.EventKey.onInputCommand, Function(x)
                                                                               Try
                                                                                   'Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                                                                                   Dim e = TryCast(CSR.BaseEvent.getFrom(x), CSR.InputCommandEvent)

                                                                                   If e IsNot Nothing Then
                                                                                       Select Case e.cmd.Substring(1).Trim()
                                                                                           Case "shop"

                                                                                               If Not preference.ContainsKey(e.playername) Then
                                                                                                   preference.Add(e.playername, JObject.Parse("{""sell"":{""input_type"":0,""slide_size_i"":1,""slide_size"":16},""recycle"":{""input_type"":0,""slide_size_i"":3,""slide_size"":64}}"))
                                                                                                   SavePreference()
                                                                                               End If

                                                                                               Program.SendMain(e.playername)
                                                                                               Return False
                                                                                           Case "shop reload"
                                                                                               Dim PermissionRaw As String = api.getPlayerPermissionAndGametype(Program.GetUUID(e.playername))
                                                                                               If String.IsNullOrEmpty(PermissionRaw) Then Return True
                                                                                               Dim permission As JObject = JObject.Parse(PermissionRaw)

                                                                                               If permission.Value(Of Integer)("permission") > 1 Then
#If FromUrl
                                        SetFromUrl("https://gitee.com/Pixel_Faramita/webAPI/raw/master/survival/shop.txt");
#Else
                                                                                                   shopdata = JObject.Parse(File.ReadAllText(shopdataPath))
#End If
                                                                                                   Program.Feedback(e.playername, "商店配置重新读取成功成功")
                                                                                               Else
                                                                                                   Program.Feedback(e.playername, "无权限执行该命令")
                                                                                               End If

                                                                                               Return False
                                                                                           Case Else
                                                                                       End Select
                                                                                       'Console.WriteLine(" <{0}> {1}", e.playername, e.cmd);
                                                                                   End If

                                                                               Catch err As Exception
                                                                                   WriteLineERR("EVENT-onInputCommand", err.Message)
                                                                               End Try

                                                                               Return True
#End Region
#End Region
#Region "MODEL "
                                                                               ''' 后台指令输出监听
                                                                               'api.addBeforeActListener(EventKey.onServerCmdOutput, x => {
                                                                               '	Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                                                                               '	var se = BaseEvent.getFrom(x) as ServerCmdOutputEvent;
                                                                               '	if (se != null) {
                                                                               '		Console.WriteLine("后台指令输出={0}", se.output);
                                                                               '	}
                                                                               '	return true;
                                                                               '});

                                                                               ''' 使用物品监听
                                                                               'api.addAfterActListener(EventKey.onUseItem, x => {
                                                                               '	Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                                                                               '	var ue = BaseEvent.getFrom(x) as UseItemEvent;
                                                                               '	if (ue != null && ue.RESULT) {
                                                                               '		Console.WriteLine("玩家 {0} 在 {1} 的 ({2}, {3}, {4})" +
                                                                               '			" 处使用了 {5} 物品。", ue.playername, ue.dimension, ue.position.x, ue.position.y, ue.position.z, ue.itemname);
                                                                               '	}
                                                                               '	return true;
                                                                               '});
                                                                               ''' 放置方块监听
                                                                               'api.addAfterActListener(EventKey.onPlacedBlock, x => {
                                                                               '	Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                                                                               '	var ue = BaseEvent.getFrom(x) as PlacedBlockEvent;
                                                                               '	if (ue != null && ue.RESULT) {
                                                                               '		Console.WriteLine("玩家 {0} 在 {1} 的 ({2}, {3}, {4})" +
                                                                               '			" 处放置了 {5} 方块。", ue.playername, ue.dimension, ue.position.x, ue.position.y, ue.position.z, ue.blockname);
                                                                               '	}
                                                                               '	return true;
                                                                               '});
                                                                               ''' 破坏方块监听
                                                                               'api.addBeforeActListener(EventKey.onDestroyBlock, x => {
                                                                               '	Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                                                                               '	var ue = BaseEvent.getFrom(x) as DestroyBlockEvent;
                                                                               '	if (ue != null) {
                                                                               '		Console.WriteLine("玩家 {0} 试图在 {1} 的 ({2}, {3}, {4})" +
                                                                               '			" 处破坏 {5} 方块。", ue.playername, ue.dimension, ue.position.x, ue.position.y, ue.position.z, ue.blockname);
                                                                               '	}
                                                                               '	return true;
                                                                               '});
                                                                               ''' 开箱监听
                                                                               'api.addBeforeActListener(EventKey.onStartOpenChest, x =>
                                                                               '{
                                                                               '    Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                                                                               '    var ue = BaseEvent.getFrom(x) as StartOpenChestEvent;
                                                                               '    if (ue != null)
                                                                               '    {
                                                                               '        Console.WriteLine("玩家 {0} 试图在 {1} 的 ({2}, {3}, {4})" +
                                                                               '            " 处打开 {5} 箱子。", ue.playername, ue.dimension, ue.position.x, ue.position.y, ue.position.z, ue.blockname);
                                                                               '    }
                                                                               '    return false;
                                                                               '});
                                                                               ''' 开桶监听
                                                                               'api.addBeforeActListener(EventKey.onStartOpenBarrel, x =>
                                                                               '{
                                                                               '    Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                                                                               '    var ue = BaseEvent.getFrom(x) as StartOpenBarrelEvent;
                                                                               '    if (ue != null)
                                                                               '    {
                                                                               '        Console.WriteLine("玩家 {0} 试图在 {1} 的 ({2}, {3}, {4})" +
                                                                               '            " 处打开 {5} 木桶。", ue.playername, ue.dimension, ue.position.x, ue.position.y, ue.position.z, ue.blockname);
                                                                               '    }
                                                                               '    return false;
                                                                               '});
                                                                               ''' 关箱监听
                                                                               'api.addAfterActListener(EventKey.onStopOpenChest, x => {
                                                                               '	Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                                                                               '	var ue = BaseEvent.getFrom(x) as StopOpenChestEvent;
                                                                               '	if (ue != null) {
                                                                               '		Console.WriteLine("玩家 {0} 在 {1} 的 ({2}, {3}, {4})" +
                                                                               '			" 处关闭 {5} 箱子。", ue.playername, ue.dimension, ue.position.x, ue.position.y, ue.position.z, ue.blockname);
                                                                               '	}
                                                                               '	return true;
                                                                               '});
                                                                               ''' 关桶监听
                                                                               'api.addAfterActListener(EventKey.onStopOpenBarrel, x => {
                                                                               '	Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                                                                               '	var ue = BaseEvent.getFrom(x) as StopOpenBarrelEvent;
                                                                               '	if (ue != null) {
                                                                               '		Console.WriteLine("玩家 {0} 在 {1} 的 ({2}, {3}, {4})" +
                                                                               '			" 处关闭 {5} 木桶。", ue.playername, ue.dimension, ue.position.x, ue.position.y, ue.position.z, ue.blockname);
                                                                               '	}
                                                                               '	return true;
                                                                               '});
                                                                               ''' 放入取出监听
                                                                               'api.addAfterActListener(EventKey.onSetSlot, x => {
                                                                               '	Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                                                                               '	var e = BaseEvent.getFrom(x) as SetSlotEvent;
                                                                               '	if (e != null) {
                                                                               '		if (e.itemcount > 0)
                                                                               '			Console.WriteLine("玩家 {0} 在 {1} 槽放入了 {2} 个 {3} 物品。",
                                                                               '				e.playername, e.slot, e.itemcount, e.itemname);
                                                                               '		else
                                                                               '			Console.WriteLine("玩家 {0} 在 {1} 槽取出了物品。",
                                                                               '				e.playername, e.slot);
                                                                               '	}
                                                                               '	return true;
                                                                               '});
                                                                               ''' 切换维度监听
                                                                               'api.addAfterActListener(EventKey.onChangeDimension, x => {
                                                                               '	Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                                                                               '	var e = BaseEvent.getFrom(x) as ChangeDimensionEvent;
                                                                               '	if (e != null && e.RESULT) {
                                                                               '			Console.WriteLine("玩家 {0} {1} 切换维度至 {2} 的 ({3},{4},{5}) 处。",
                                                                               '				e.playername, e.isstand?"":"悬空地", e.dimension, e.XYZ.x, e.XYZ.y, e.XYZ.z);
                                                                               '	}
                                                                               '	return true;
                                                                               '});
                                                                               ''' 生物死亡监听
                                                                               'api.addAfterActListener(EventKey.onMobDie, x => {
                                                                               '	Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                                                                               '	var e = BaseEvent.getFrom(x) as MobDieEvent;
                                                                               '	if (e != null && !string.IsNullOrEmpty(e.mobname)) {
                                                                               '			Console.WriteLine(" {0} 在 {1} ({2:F2},{3:F2},{4:F2}) 处被 {5} 杀死了。",
                                                                               '				e.mobname, e.dimension, e.XYZ.x, e.XYZ.y, e.XYZ.z, e.srcname);
                                                                               '	}
                                                                               '	return true;
                                                                               '});
                                                                               ''' 玩家重生监听
                                                                               'api.addAfterActListener(EventKey.onRespawn, x => {
                                                                               '	Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                                                                               '	var e = BaseEvent.getFrom(x) as RespawnEvent;
                                                                               '	if (e != null && e.RESULT) {
                                                                               '			Console.WriteLine("玩家 {0} 已于 {1} 的 ({2:F2},{3:F2},{4:F2}) 处重生。",
                                                                               '				e.playername, e.dimension, e.XYZ.x, e.XYZ.y, e.XYZ.z);
                                                                               '	}
                                                                               '	return true;
                                                                               '});
                                                                               ''' 聊天监听
                                                                               'api.addAfterActListener(EventKey.onChat, x => {
                                                                               '	Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                                                                               '	var e = BaseEvent.getFrom(x) as ChatEvent;
                                                                               '	if (e != null) {
                                                                               '		Console.WriteLine(" {0} {1} 说：{2}", e.playername,
                                                                               '			!string.IsNullOrEmpty(e.target) ? "悄悄地对 " + e.target : "", e.msg);
                                                                               '	}
                                                                               '	return true;
                                                                               '});
                                                                               ''' 输入文本监听
                                                                               'api.addBeforeActListener(EventKey.onInputText, x => {
                                                                               '	Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                                                                               '	var e = BaseEvent.getFrom(x) as InputTextEvent;
                                                                               '	if (e != null) {
                                                                               '		Console.WriteLine(" <{0}> {1}", e.playername, e.msg);
                                                                               '	}
                                                                               '	return true;
                                                                               '});


                                                                               ''' 世界范围爆炸监听，拦截
                                                                               'api.addBeforeActListener(EventKey.onLevelExplode, x => {
                                                                               '	Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                                                                               '	var e = BaseEvent.getFrom(x) as LevelExplodeEvent;
                                                                               '	if (e != null) {
                                                                               '		Console.WriteLine("位于 {0} ({1},{2},{3}) 的 {4} 试图发生强度 {5} 的爆炸。",
                                                                               '			e.dimension, e.position.x, e.position.y, e.position.z,
                                                                               '			string.IsNullOrEmpty(e.entity) ? e.blockname : e.entity, e.explodepower);
                                                                               '	}
                                                                               '	return false;
                                                                               '});
                                                                               ''' *
                                                                               ''' 玩家移动监听
                                                                               'api.addAfterActListener(EventKey.onMove, x => {
                                                                               '	var e = BaseEvent.getFrom(x) as MoveEvent;
                                                                               '	if (e != null) {
                                                                               '		Console.WriteLine("玩家 {0} {1} 移动至 {2} ({3},{4},{5}) 处。",
                                                                               '			e.playername, (e.isstand) ? "":"悬空地", e.dimension,
                                                                               '			e.XYZ.x, e.XYZ.y, e.XYZ.z);
                                                                               '	}
                                                                               '	return false;
                                                                               '});
                                                                               '
                                                                               ''' 玩家加入游戏监听
                                                                               'api.addAfterActListener(EventKey.onLoadName, x => {
                                                                               '	Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                                                                               '	var ue = BaseEvent.getFrom(x) as LoadNameEvent;
                                                                               '	if (ue != null) {
                                                                               '		Console.WriteLine("玩家 {0} 加入了游戏，xuid={1}", ue.playername, ue.xuid);
                                                                               '	}
                                                                               '	return true;
                                                                               '});
                                                                               ''' 玩家离开游戏监听
                                                                               'api.addAfterActListener(EventKey.onPlayerLeft, x => {
                                                                               '	Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                                                                               '	var ue = BaseEvent.getFrom(x) as PlayerLeftEvent;
                                                                               '	if (ue != null) {
                                                                               '		Console.WriteLine("玩家 {0} 离开了游戏，xuid={1}", ue.playername, ue.xuid);
                                                                               '	}
                                                                               '	return true;
                                                                               '});

                                                                               ''' 攻击监听
                                                                               ''' API 方式注册监听器
                                                                               'api.addAfterActListener(EventKey.onAttack, x => {
                                                                               '	Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                                                                               '	AttackEvent ae = BaseEvent.getFrom(x) as AttackEvent;
                                                                               '	if (ae != null) {
                                                                               '		string str = "玩家 " + ae.playername + " 在 (" + ae.XYZ.x.ToString("F2") + "," +
                                                                               '			ae.XYZ.y.ToString("F2") + "," + ae.XYZ.z.ToString("F2") + ") 处攻击了 " + ae.actortype + " 。";
                                                                               '		Console.WriteLine(str);
                                                                               '		//Console.WriteLine("list={0}", api.getOnLinePlayers());
                                                                               '		string ols = api.getOnLinePlayers();
                                                                               '		if (!string.IsNullOrEmpty(ols))
                                                                               '                 {
                                                                               '			JavaScriptSerializer ser = new JavaScriptSerializer();
                                                                               '			ArrayList al = ser.Deserialize<ArrayList>(ols);
                                                                               '			object uuid = null;
                                                                               '			foreach (Dictionary<string, object> p in al)
                                                                               '			{
                                                                               '				object name;
                                                                               '				if (p.TryGetValue("playername", out name))
                                                                               '				{
                                                                               '					if ((string)name == ae.playername)
                                                                               '					{
                                                                               '						// 找到
                                                                               '						p.TryGetValue("uuid", out uuid);
                                                                               '						break;
                                                                               '					}
                                                                               '				}
                                                                               '			}
                                                                               '			if (uuid != null)
                                                                               '			{
                                                                               '				var id = api.sendSimpleForm((string)uuid,
                                                                               '								   "致命选项",
                                                                               '								   "test choose:",
                                                                               '								   "[\"生存\",\"死亡\",\"求助\"]");
                                                                               '				Console.WriteLine("创建需自行保管的表单，id={0}", id);
                                                                               '				//api.transferserver((string)uuid, "www.xiafox.com", 19132);
                                                                               '			}
                                                                               '		}
                                                                               '	} else {
                                                                               '		Console.WriteLine("Event convent fail.");
                                                                               '	}
                                                                               '	return true;
                                                                               '});
                                                                               '#region 非社区部分内容
                                                                               'if (api.COMMERCIAL)
                                                                               '{
                                                                               '	// 生物伤害监听
                                                                               '	api.addBeforeActListener(EventKey.onMobHurt, x => {
                                                                               '		Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                                                                               '		var e = BaseEvent.getFrom(x) as MobHurtEvent;
                                                                               '		if (e != null && !string.IsNullOrEmpty(e.mobname))
                                                                               '		{
                                                                               '			Console.WriteLine(" {0} 在 {1} ({2:F2},{3:F2},{4:F2}) 即将受到来自 {5} 的 {6} 点伤害，类型 {7}",
                                                                               '				e.mobname, e.dimension, e.XYZ.x, e.XYZ.y, e.XYZ.z, e.srcname, e.dmcount, e.dmtype);
                                                                               '		}
                                                                               '		return true;
                                                                               '	});
                                                                               '	// 命令块执行指令监听，拦截
                                                                               '	api.addBeforeActListener(EventKey.onBlockCmd, x => {
                                                                               '		Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                                                                               '		var e = BaseEvent.getFrom(x) as BlockCmdEvent;
                                                                               '		if (e != null)
                                                                               '		{
                                                                               '			Console.WriteLine("位于 {0} ({1},{2},{3}) 的 {4} 试图执行指令 {5}",
                                                                               '				e.dimension, e.position.x, e.position.y, e.position.z, e.name, e.cmd);
                                                                               '		}
                                                                               '		return false;
                                                                               '	});
                                                                               '	// NPC执行指令监听，拦截
                                                                               '	api.addBeforeActListener(EventKey.onNpcCmd, x => {
                                                                               '		Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                                                                               '		var e = BaseEvent.getFrom(x) as NpcCmdEvent;
                                                                               '		if (e != null)
                                                                               '		{
                                                                               '			Console.WriteLine("位于 {0} ({1},{2},{3}) 的 {4} 试图执行第 {5} 条指令，指令集\n{6}",
                                                                               '				e.dimension, e.position.x, e.position.y, e.position.z, e.npcname, e.actionid, e.actions);
                                                                               '		}
                                                                               '		return false;
                                                                               '	});
                                                                               '	// 更新命令方块监听
                                                                               '	api.addBeforeActListener(EventKey.onCommandBlockUpdate, x => {
                                                                               '		Console.WriteLine("[CS] type = {0}, mode = {1}, result= {2}", x.type, x.mode, x.result);
                                                                               '		var e = BaseEvent.getFrom(x) as CommandBlockUpdateEvent;
                                                                               '		if (e != null)
                                                                               '		{
                                                                               '			Console.WriteLine(" {0} 试图修改位于 {1} ({2},{3},{4}) 的 {5} 的命令为 {6}",
                                                                               '				e.playername, e.dimension, e.position.x, e.position.y, e.position.z,
                                                                               '				e.isblock ? "命令块" : "命令矿车", e.cmd);
                                                                               '		}
                                                                               '		return true;
                                                                               '	});
                                                                               '}
                                                                               '         #endregion

#End Region

                                                                               ' 高级玩法，硬编码方式注册hook

                                                                               'THook.init(api);
                                                                           End Function)
            Catch err As Exception
                WriteLineERR("插件遇到严重错误，无法继续运行", err.Message)
            End Try
        End Sub
    End Class
End Namespace

Namespace CSR
    Partial Class Plugin
        Friend Shared pluginThread As Thread = Nothing

        Friend Shared Sub SetupPluginThread(ByVal api As CSR.MCCSAPI)
            pluginThread = New Thread(Sub()
                                          Try
                                              Program.Init(api)
                                          Catch err As Exception
                                              Console.WriteLine("[PFSHOP崩了]（10s后自动重载...）" & Microsoft.VisualBasic.Constants.vbLf & "错误信息:" & err.ToString())
                                              __ = Task.Run(Sub()
                                                                Thread.Sleep(10000)
                                                                Plugin.SetupPluginThread(api)
                                                            End Sub)
                                          End Try
                                      End Sub)
            pluginThread.SetApartmentState(ApartmentState.STA)
            pluginThread.Start()
        End Sub

        Friend Shared Sub onStart(ByVal api As CSR.MCCSAPI)
            'Program.Init(api);
            Plugin.SetupPluginThread(api)
        End Sub
    End Class
End Namespace
