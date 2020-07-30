using BilibiliDM_PluginFramework;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;

#pragma warning disable CA1031 // Do not catch general exception types
namespace ExtendLogging
{
    public class MainProgram : DMPlugin
    {
        private static IDictionary<string, string> Titles { get; } = new Dictionary<string, string>
        {
            { "title-4-1", "超·年糕团长" },
            { "title-9-1", "真·圣诞爸爸" },
            { "title-10-1", "圣·尼古拉斯" },
            { "title-39-1", "7th.Annv" },
            { "title-46-1", "甘すぎる" },
            { "title-47-1", "King" },
            { "title-58-1", "夜空花火" },
            { "title-62-1", "[小电视]应援" },
            { "title-63-1", "[22]应援" },
            { "title-64-1", "[33]应援" },
            { "title-65-1", "(黄)STAR" },
            { "title-66-1", "(紫)STAR" },
            { "title-67-1", "(蓝)STAR" },
            { "title-68-1", "(青)STAR" },
            { "title-69-1", "(红)STAR" },
            { "title-70-1", "(黄)SUPERSTAR" },
            { "title-71-1", "(紫)SUPERSTAR" },
            { "title-72-1", "(蓝)SUPERSTAR" },
            { "title-73-1", "(青)SUPERSTAR" },
            { "title-74-1", "(红)SUPERSTAR" },
            { "title-77-1", "Miss 椛" },
            { "title-80-1", "SANTA☆CLAUS" },
            { "title-92-1", "年兽" },
            { "title-93-1", "注孤生" },
            { "title-99-1", "神域阐释者" },
            { "title-107-1", "神州" },
            { "title-111-1", "bilibili link" },
            { "title-113-1", "[小电视]应援(复刻)" },
            { "title-114-1", "[22]应援(复刻)" },
            { "title-115-1", "[33]应援(复刻)" },
            { "title-119-1", "五魅首" },
            { "title-128-2", "唯望若安" },
            { "title-139-1", "雷狮海盗" },
            { "title-147-1", "LPL2018" },
            { "title-156-1", "一本满足(复刻)" },
            { "title-157-1", "吃瓜群众(复刻)" },
            { "title-164-1", "PK" },
            { "title-165-1", "最佳助攻" },
            { "title-166-1", "Cantus Knight" },
            { "title-167-1", "Rhythm Saber" },
            { "title-179-1", "时光守护" },
            { "title-190-1", "锦鲤" },
            { "title-201-1", "震惊" },
            { "title-224-1", "Kizuner(初级)" },
            { "title-225-1", "Kizuner(高级)" },
            { "title-241-1", "BilibiliWorld" },
            { "title-243-1", "治愈笑颜" },
            { "title-245-1", "mikufans/245" },
            { "title-263-1", "打电话" },
            { "title-270-1", "FFF团员(复刻)" },
            { "title-281-1", "2019-IVC" },
            { "title-284-1", "音·轨迹" },
            { "title-285-1", "mikufans/285" },
            { "title-290-1", "蘑菇♥1219" },
            { "title-293-1", "LPL2020" },
            { "title-296-1", "奥利给" },
            { "title-301-1", "OWL2020" },
            { "title-308-1", "这把算我赢" },
            { "title-310-1", "2020KPL" },
            { "title-315-1", "镜出动计划" },
            { "title-319-1", "COA-3" },
            { "title-324-1", "蹦迪玩家" },
            { "title-326-1", "星玩家" },
        };

        public static string ConfigPath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), @"弹幕姬\Plugins\ExtendLogging");

        private static VersionChecker VChecker { get; } = new VersionChecker("ExtendLogging");

        private PluginSettings PSettings { get; }
        
        private SettingsWindow SettingsWnd { get; }

        #region DmjReflections
        private Queue<DanmakuModel> DanmakuQueue { get; } = new Queue<DanmakuModel>();

        private object Static { get; }

        private PropertyInfo DanmakuCountShow { get; }

        private FieldInfo EnabledShowError { get; }

        private FieldInfo EnabledIgnoreSpam { get; }

        private MethodInfo AddUser { get; }

        private MethodInfo Logging { get; }

        private MethodInfo AddDMText { get; }

        private MethodInfo SendSSP { get; }

        private MethodInfo BaseProcDanmaku { get; }

        public Window DmjWnd { get; }
        #endregion

        public MainProgram()
        {
            if (!Directory.Exists(ConfigPath))
            {
                Directory.CreateDirectory(ConfigPath);
            }
            string filePath = Path.Combine(ConfigPath, "Config.cfg");
            PSettings = new PluginSettings(filePath);
            try
            {
                PSettings.LoadConfig();
            }
            catch (Exception e)
            {
                new FileInfo(filePath).MoveTo(Path.Combine(ConfigPath, "BrokenConfig.cfg"));
                PSettings.SaveConfig();
                MessageBox.Show($"载入配置文件失败:{e}\n损坏的配置文件已保存至BrokenConfig.cfg,新的配置文件已生成", "更多日志", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            if (PSettings.Enabled)
            {
                this.Start();
            }
            SettingsWnd = new SettingsWindow(PSettings);
            DmjWnd = Application.Current.MainWindow;
            this.PluginName = "更多日志";
            try
            {
                this.PluginAuth = BiliUtils.GetUserNameByUserId(35744708);
            }
            catch
            {
                this.PluginAuth = "Executor丶";
            }
            this.PluginVer = Assembly.GetExecutingAssembly().GetName().Version.ToString(3);
            this.PluginDesc = "管理弹幕姬日志输出行为";
            this.PluginCont = "847529602@qq.com";
            Type dmjType = DmjWnd.GetType();
            DanmakuQueue = (Queue<DanmakuModel>)dmjType.GetField("_danmakuQueue", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic).GetValue(DmjWnd);
            Logging = dmjType.GetMethod("logging", BindingFlags.Instance | BindingFlags.Public);
            AddDMText = dmjType.GetMethod("AddDMText", BindingFlags.Instance | BindingFlags.Public);
            SendSSP = dmjType.GetMethod("SendSSP", BindingFlags.Instance | BindingFlags.Public);
            BaseProcDanmaku = dmjType.GetMethod("ProcDanmaku", BindingFlags.Instance | BindingFlags.NonPublic);
            Static = dmjType.GetField("Static", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic).GetValue(DmjWnd);
            AddUser = Static.GetType().GetMethod("AddUser", BindingFlags.Instance | BindingFlags.Public);
            DanmakuCountShow = Static.GetType().GetProperty("DanmakuCountShow", BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.Public);
            EnabledShowError = dmjType.GetField("showerror_enabled", BindingFlags.Instance | BindingFlags.NonPublic);
            EnabledIgnoreSpam = dmjType.GetField("ignorespam_enabled", BindingFlags.Instance | BindingFlags.NonPublic);
            ((Thread)dmjType.GetField("ProcDanmakuThread", BindingFlags.GetField | BindingFlags.Instance | BindingFlags.NonPublic).GetValue(DmjWnd)).Abort();
            new Thread(() =>
            {
                while (true)
                {
                    lock (DanmakuQueue)
                    {
                        var count = 0;
                        if (DanmakuQueue.Any())
                        {
                            count = (int)Math.Ceiling(DanmakuQueue.Count / 30.0);
                        }
                        for (var i = 0; i < count; i++)
                        {
                            if (DanmakuQueue.Any())
                            {
                                var danmaku = DanmakuQueue.Dequeue();
                                ProcDanmaku(danmaku);
                                if (danmaku.MsgType == MsgTypeEnum.Comment)
                                {
                                    lock (Static)
                                    {
                                        AddDanmakuCountShow();
                                        AddUser.Invoke(Static, new object[] { danmaku.UserName });
                                    }
                                }
                            }
                        }
                    }
                    Thread.Sleep(30);
                }
            }){ IsBackground = true }.Start();
        }

        public void AddDanmakuCountShow()
        {
            long x = (long)DanmakuCountShow.GetValue(Static);
            DanmakuCountShow.SetValue(Static, ++x);
        }

        private void ProcDanmaku(DanmakuModel danmakuModel)
        {
            try
            {
                if (this.Status)
                {
                    if (danmakuModel.MsgType == MsgTypeEnum.Comment)
                    {
                        int UserLevel = danmakuModel.RawDataJToken["info"][4][0].ToObject<int>();
                        if ((!PSettings.EnableShieldLevel || UserLevel >= PSettings.ShieldLevel) && (!(bool)EnabledIgnoreSpam.GetValue(DmjWnd) || !danmakuModel.RawDataJToken["info"][0][9].ToObject<bool>()))
                        {
                            int UserMedalLevel = 0;
                            string UserMedalName = null;
                            string UserTitle = danmakuModel.RawDataJToken["info"][5].HasValues ? danmakuModel.RawDataJToken["info"][5][1].ToString() : null;
                            if (UserTitle != null)
                            {
                                if (Titles.ContainsKey(UserTitle))
                                {
                                    UserTitle = Titles[UserTitle];
                                }
                                else
                                {
                                    try
                                    {
                                        UpdateTitles();
                                    }
                                    catch
                                    {

                                    }
                                    if (Titles.ContainsKey(UserTitle))
                                    {
                                        UserTitle = Titles[UserTitle];
                                    }
                                    else
                                    {
                                        UserTitle = null;
                                    }
                                }
                            }
                            if (danmakuModel.RawDataJToken["info"][3].HasValues)
                            {
                                UserMedalLevel = danmakuModel.RawDataJToken["info"][3][0].ToObject<int>();
                                UserMedalName = danmakuModel.RawDataJToken["info"][3][1].ToString();
                            }
                            string prefix = $"{(danmakuModel.isAdmin ? "[管]" : "")}{(danmakuModel.UserGuardLevel == 3 ? "[舰]" : danmakuModel.UserGuardLevel == 2 ? "[提]" : danmakuModel.UserGuardLevel == 1 ? "[总]" : null)}{(danmakuModel.isVIP ? "[爷]" : "")}{(PSettings.LogMedal && !string.IsNullOrEmpty(UserMedalName) ? $"{{{UserMedalName},{UserMedalLevel}}}" : null)}{(PSettings.LogTitle && !string.IsNullOrEmpty(UserTitle) ? $"[{UserTitle}]" : "")}{(PSettings.LogLevel ? $"(UL {UserLevel})" : "")}{danmakuModel.UserName}";
                            Logging.Invoke(DmjWnd, new object[] { $"收到彈幕:{prefix} 說: {danmakuModel.CommentText}" });
                            AddDMText.Invoke(DmjWnd, new object[] { prefix, danmakuModel.CommentText, false, false, null });
                            SendSSP.Invoke(DmjWnd, new object[] { string.Format(@"\_q{0}\n\_q\f[height,20]{1}", prefix, danmakuModel.CommentText) });
                        }
                    }
                    else
                    {
                        switch (danmakuModel.MsgType)
                        {
                            case MsgTypeEnum.LiveStart when PSettings.LogExternInfo:
                            case MsgTypeEnum.Unknown when PSettings.LogExternInfo:
                            case MsgTypeEnum.LiveEnd when PSettings.LogExternInfo:
                                {
                                    switch (danmakuModel.RawDataJToken["cmd"].ToString())
                                    {
                                        case "ROOM_SILENT_ON":
                                            {
                                                string type = danmakuModel.RawDataJToken["data"]["type"].ToString();
                                                int endTimeStamp = danmakuModel.RawDataJToken["data"]["second"].ToObject<int>();
                                                string toLog = $"主播开启了房间禁言.类型:{(type == "member" ? "全体用户" : type == "medal" ? "粉丝勋章" : "用户等级")};{(type != "member" ? $"等级:{danmakuModel.RawDataJToken["data"]["level"]};" : "")}时间:{(endTimeStamp == -1 ? "直到下播" : $"直到{new DateTime(1970, 1, 1).AddSeconds(endTimeStamp).ToLocalTime():yyyy-MM-dd HH:mm:ss}")}";
                                                ThreeAction(toLog);
                                                break;
                                            }
                                        case "ROOM_SILENT_OFF":
                                            {
                                                string toLog = "主播取消了房间禁言";
                                                ThreeAction(toLog);
                                                break;
                                            }
                                        case "ROOM_BLOCK_MSG":
                                            {
                                                string toLog = $"用户 {danmakuModel.RawDataJToken["uname"]}[{danmakuModel.RawDataJToken["uid"]}] 已被房管禁言";
                                                ThreeAction(toLog);
                                                break;
                                            }
                                        case "WARNING":
                                            {
                                                string toLog = $"直播间被警告:{danmakuModel.RawDataJToken["msg"]}";
                                                ThreeAction(toLog);
                                                break;
                                            }
                                        case "CUT_OFF":
                                            {
                                                string toLog = "当前直播间被直播管理员切断";
                                                ThreeAction(toLog);
                                                break;
                                            }
                                        case "ROOM_LOCK":
                                            {
                                                string toLog = "当前直播间被直播管理员关闭";
                                                ThreeAction(toLog);
                                                break;
                                            }
                                        case "LIVE":
                                            {
                                                string toLog = "主播已开播";
                                                ThreeAction(toLog);
                                                break;
                                            }
                                        case "PREPARING":
                                            {
                                                string toLog = "主播已下播";
                                                ThreeAction(toLog);
                                                break;
                                            }
                                        default:
                                            {
                                                BaseProcDanmaku.Invoke(DmjWnd, new object[] { danmakuModel });
                                                break;
                                            }
                                    }
                                    break;
                                }
                            case MsgTypeEnum.Unknown when danmakuModel.RawDataJToken["cmd"].ToString() == "INTERACT_WORD":
                                {
                                    int msgType = danmakuModel.RawDataJToken["data"]["msg_type"].ToObject<int>();
                                    if (PSettings.LogEnter && msgType == 1)
                                    {
                                        string userName = danmakuModel.RawDataJToken["data"]["uname"].ToString();
                                        Logging.Invoke(DmjWnd, new object[] { $"进房提示: {userName} 进入直播间" });
                                        DmjWnd.Dispatcher.Invoke(() => AddDMText.Invoke(DmjWnd, new object[] { "关注提示", $"{userName} 进入直播间", true, false, null }));
                                    }
                                    else if (PSettings.LogFollow && msgType == 2)
                                    {
                                        string userName = danmakuModel.RawDataJToken["data"]["uname"].ToString();
                                        Logging.Invoke(DmjWnd, new object[] { $"关注提示: {userName} 关注了直播间" });
                                        DmjWnd.Dispatcher.Invoke(() => AddDMText.Invoke(DmjWnd, new object[] { "关注提示", $"{userName} 关注了直播间", true, false, null }));
                                    }
                                    break;
                                }
                            default:
                                {
                                    BaseProcDanmaku.Invoke(DmjWnd, new object[] { danmakuModel });
                                    break;
                                }
                        }
                    }
                }
                else
                {
                    BaseProcDanmaku.Invoke(DmjWnd, new object[] { danmakuModel });
                }
            }
            catch
            {

            }
        }

        private void ThreeAction(string toLog)
        {
            Logging.Invoke(DmjWnd, new object[] { $"系统通知:{toLog}" });
            if ((bool)EnabledShowError.GetValue(DmjWnd))
            {
                DmjWnd.Dispatcher.Invoke(() => AddDMText.Invoke(DmjWnd, new object[] { "系统通知", toLog, true, false, null }));
            }
            else
            {
                DmjWnd.Dispatcher.Invoke(() => AddDMText.Invoke(DmjWnd, new object[] { "系统通知", toLog, false, false, null }));
            }
            SendSSP.Invoke(DmjWnd, new object[] { string.Format(@"\_q{0}\n\_q\f[height,20]{1}", "系统通知", toLog) });
        }

        public override void Inited()
        {
            Assembly dmAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(p => p.FullName.StartsWith("Bililive_dm,"));
            Type appType = dmAssembly.ExportedTypes.FirstOrDefault(p => p.FullName == "Bililive_dm.App");
            //Plugins = (ObservableCollection<DMPlugin>)appType.GetField("Plugins", BindingFlags.GetField | BindingFlags.Static | BindingFlags.Public).GetValue(null);
            //Type utilsType = dmAssembly.ExportedTypes.FirstOrDefault(p => p.FullName == "Bililive_dm.Utils");
            if (!VChecker.FetchInfo())
            {
                Log("版本检查失败 : " + VChecker.LastException.Message);
                return;
            }
            if (VChecker.HasNewVersion(this.PluginVer))
            {
                Log("有新版本啦~最新版本 : " + VChecker.Version + "\n                " + VChecker.UpdateDescription);
                Log("下载地址 : " + VChecker.DownloadUrl);
                Log("插件页面 : " + VChecker.WebPageUrl);
            }
        }

        public override void DeInit()
        {
            SettingsWnd.Closing -= SettingsWnd.Window_Closing;
            SettingsWnd.Close();
        }

        public override void Admin()
        {
            SettingsWnd.Show();
            SettingsWnd.Topmost = true;
            SettingsWnd.Topmost = false;
        }

        public override void Start()
        {
            PSettings.Enabled = true;
            base.Start();
        }

        public override void Stop()
        {
            PSettings.Enabled = false;
            base.Stop();
        }
        
        public static void UpdateTitles()
        {
            string json = HttpHelper.HttpGet("https://api.live.bilibili.com/rc/v1/Title/webTitles", 5);
            JObject j = JObject.Parse(json);
            int code = j["code"].ToObject<int>();
            if (code == 0)
            {
                foreach (JToken jt in j["data"].Where(p => !Titles.ContainsKey(p["identification"].ToString())))
                {
                    Titles.Add(jt["identification"].ToString(), jt["name"].ToString().Trim());
                }
            }
        }
    }
}
