using BilibiliDM_PluginFramework;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Windows;

namespace ExtendLogging
{
    public class MainProgram : DMPlugin
    {
        private bool _LogLevel = true;
        public bool LogLevel { get => _LogLevel; set { if (_LogLevel != value) { _LogLevel = value; SaveConfig(); } } }
        private bool _LogMedal = true;
        public bool LogMedal { get => _LogMedal; set { if (_LogMedal != value) { _LogMedal = value; SaveConfig(); } } }
        private bool _LogTitle = true;
        public bool LogTitle { get => _LogTitle; set { if (_LogTitle != value) { _LogTitle = value; SaveConfig(); } } }
        private bool _LogExternInfo = true;
        public bool LogExternInfo { get => _LogExternInfo; set { if (_LogExternInfo != value) { _LogExternInfo = value; SaveConfig(); } } }

        private static System.Timers.Timer WriteConfigTimer { get; } = new System.Timers.Timer(2000) { AutoReset = false };

        private SettingsWindow SettingsWnd { get; }

        private Queue<DanmakuModel> DanmakuQueue { get; } = new Queue<DanmakuModel>();

        private object Static { get; }

        private PropertyInfo DanmakuCountShow { get; }

        //private ObservableCollection<DMPlugin> Plugins { get; set; }

        private MethodInfo AddUser { get; }

        private MethodInfo Logging { get; }

        private MethodInfo AddDMText { get; }

        private MethodInfo SendSSP { get; }

        private MethodInfo BaseProcDanmaku { get; }

        public static MainProgram Instance { get; private set; }

        public Window DmjWnd { get; }

        private static IDictionary<string, string> Titles { get; } = new Dictionary<string, string>
        {
            { "title-1-1", "糯米粉" },
            { "title-2-1", "年糕团" },
            { "title-3-1", "年糕团长" },
            { "title-4-1", "超·年糕团长"},
            { "title-5-1", "圣诞老人"},
            { "title-6-1", "圣诞中年人"},
            { "title-7-1", "圣诞青年"},
            { "title-8-1", "圣诞小天使" },
            { "title-9-1", "真·圣诞爸爸" },
            { "title-10-1", "圣·尼古拉斯" },
            { "title-11-1", "雪亲王" },
            { "title-12-1", "辅导员" },
            { "title-13-1", "班主任" },
            { "title-14-1", "教导主任" },
            { "title-15-1", "校长" },
            { "title-16-1", "资深老司机" },
            { "title-17-1", "月老" },
            { "title-18-1", "庇护之光" },
            { "title-19-1", "圣骑士" },
            { "title-20-1", "冒险家" },
            { "title-21-1", "旅人" },
            { "title-22-1", "甜心神话" },
            { "title-23-1", "甜心天使" },
            { "title-24-1", "甜心精灵" },
            { "title-25-1", "唱见神话" },
            { "title-26-1", "唱见天使" },
            { "title-27-1", "唱见精灵" },
            { "title-28-1", "姹紫嫣红" },
            { "title-29-1", "度年如日" },
            { "title-30-1", "追云逐月" },
            { "title-31-1", "神龙" },
            { "title-32-1", "菠菜" },
            { "title-33-1", "被窝" },
            { "title-34-1", "起来嗨" },
            { "title-35-1", "关灯" },
            { "title-36-1", "方得始终" },
            { "title-37-1", "久负盛名" },
            { "title-38-1", "超耐久" },
            { "title-39-1", "7th.Annv" },
            { "title-40-1", "甜咸无双" },
            { "title-41-1", "咸党" },
            { "title-42-1", "咸蛋超人" },
            { "title-43-1", "咸鱼皇" },
            { "title-44-1", "甜党" },
            { "title-45-1", "砂糖战士" },
            { "title-46-1", "甘すぎる" },
            { "title-47-1", "Kind" },
            { "title-48-1", "钻石星尘" },
            { "title-49-1", "绝对零度" },
            { "title-50-1", "钻石王老五" },
            { "title-51-1", "大神" },
            { "title-52-1", "高手" },
            { "title-53-1", "神七" },
            { "title-54-1", "暖心" },
            { "title-55-1", "全是套路" },
            { "title-56-1", "红叶祭" },
            { "title-56-2", "栖霞红枫" },
            { "title-56-3", "香山黄栌" },
            { "title-56-4", "秋之回忆" },
            { "title-57-1", "金闪闪" },
            { "title-58-1", "夜空花火" },
            { "title-59-1", "一本满足" },
            { "title-60-1", "百鬼夜行" },
            { "title-61-1", "吃瓜群众" },
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
            { "title-75-1", "见习舰长" },
            { "title-75-2", "代理提督" },
            { "title-75-3", "名誉总督" },
            { "title-76-1", "理事长" },
            { "title-77-1", "Miss 椛" },
            { "title-78-1", "FFF团员" },
            { "title-79-1", "FFF团长" },
            { "title-80-1", "SANTA☆CLAUS" },
            { "title-81-1", "新年快乐" },
            { "title-82-1", "御守" },
            { "title-82-2", "满分御守" },
            { "title-82-3", "桃花御守" },
            { "title-83-1", "功成名就" },
            { "title-84-1", "水滴石穿" },
            { "title-85-1", "孜孜不倦" },
            { "title-86-1", "季老" },
            { "title-87-1", "四季老" },
            { "title-88-1", "不负初心" },
            { "title-89-1", "爆竹" },
            { "title-89-2", "意大利炮" },
            { "title-89-3", "爆竹" },
            { "title-90-1", "财神爷" },
            { "title-91-1", "灶神" },
            { "title-92-1", "年兽" },
            { "title-93-1", "注孤生" },
            { "title-94-1", "酋长" },
            { "title-95-1", "丘比特" },
            { "title-95-2", "维纳斯" },
            { "title-96-1", "神曲终焉" },
            { "title-97-1", "无上至尊" },
            { "title-98-1", "至高骑士" },
            { "title-99-1", "神域阐释者" },
            { "title-100-1", "蒸汽" },
            { "title-101-1", "神州" },
            { "title-102-1", "骑士" },
            { "title-103-1", "神圣" },
            { "title-104-1", "蒸汽" },
            { "title-105-1", "神州" },
            { "title-106-1", "骑士" },
            { "title-107-1", "神州" },
            { "title-108-1", "演武者" },
            { "title-108-2", "武神" },
            { "title-109-1", "征服者" },
            { "title-110-1", "征服王" },
            { "title-111-1", "bilibili link" },
            { "title-112-1", "旅人" },
            { "title-113-1", "[小电视]应援(复刻)" },
            { "title-114-1", "[22]应援(复刻)" },
            { "title-115-1", "[33]应援(复刻)" },
            { "title-116-1", "庇护之光" },
            { "title-117-1", "圣骑士" },
            { "title-118-1", "冒险家" },
            { "title-119-1", "五魅首" },
            { "title-120-1", "门庭若市" },
            { "title-121-1", "百战雄狮" },
            { "title-122-1", "岁月如歌" },
            { "title-123-1", "一般社员" },
            { "title-123-2", "执行委员" },
            { "title-124-1", "大队长" },
            { "title-125-1", "在此饶舌" },
            { "title-126-1", "绚烂夏花" },
            { "title-126-2", "荷塘月色" },
            { "title-127-1", "自学高手" },
            { "title-127-2", "白学大师" },
            { "title-128-1", "青葱岁月" },
            { "title-128-2", "唯望若安" },
            { "title-129-1", "风花雪月" },
            { "title-130-1", "镜花水月" },
            { "title-131-1", "太阳骑士" },
            { "title-131-2", "天选之人" },
            { "title-133-1", "一叶知秋" },
            { "title-134-1", "全场最佳" },
            { "title-134-2", "王者之证" },
            { "title-135-1", "为所欲为" },
            { "title-136-1", "老哥稳" },
            { "title-137-1", "一缺三" },
            { "title-138-1", "总选之王" },
            { "title-139-1", "雷狮海盗" },
            { "title-140-1", "秋田君" },
            { "title-140-2", "年兽克星" },
            { "title-141-1", "在下福了" },
            { "title-142-1", "喜气之王" },
            { "title-143-1", "红线仙" },
            { "title-144-1", "一见倾心" },
            { "title-144-2", "花前月下" },
            { "title-145-1", "桃仙" },
            { "title-146-1", "理事长" },
            { "title-147-1", "LPL2018" },
            { "title-148-1", "头号歌迷" },
            { "title-149-1", "大吉大利" },
            { "title-150-1", "甜蜜双排" },
            { "title-151-1", "全靠浪" },
            { "title-152-1", "大冒险家" },
            { "title-153-1", "1号玩家" },
            { "title-153-2", "荣耀之巅" },
            { "title-154-1", "为爱而生" },
            { "title-155-1", "初号机" },
            { "title-156-1", "一本满足(复刻)" },
            { "title-157-1", "吃瓜群众(复刻)" },
            { "title-159-1", "王蜀黍" },
            { "title-160-1", "天秀凉皇" },
            { "title-161-1", "审判官" },
            { "title-162-1", "枸杞牛奶" },
            { "title-163-1", "首席应援" },
            { "title-164-1", "PK" },
            { "title-165-1", "最佳助攻" },
            { "title-166-1", "Cantus Knight" },
            { "title-167-1", "Rhythm Saber" },

        };

        private static string PluginPath { get; } = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), @"弹幕姬\plugins\");

        public MainProgram()
        {
            Instance = this;
            LoadConfig();
            DmjWnd = Application.Current.MainWindow;
            SettingsWnd = new SettingsWindow();
            WriteConfigTimer.Elapsed += WriteConfigTimer_Elapsed;
            this.PluginName = "更多日志";
            try
            {
                this.PluginAuth = GetUserNameByUserId(35744708);
            }
            catch
            {
                this.PluginAuth = "Executor丶";
            }
            this.PluginVer = "1.0.1";
            this.PluginDesc = "更详细的日志记录";
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
            }){ IsBackground = true }.Start(); // 接管弹幕姬线程
        }

        public void AddDanmakuCountShow()
        {
            long x = (long)DanmakuCountShow.GetValue(Static);
            DanmakuCountShow.SetValue(Static, ++x);
        }

        private void ProcDanmaku(DanmakuModel danmakuModel)
        {
            if (this.Status)
            {
                if ((LogLevel || LogMedal || LogTitle) && danmakuModel.MsgType == MsgTypeEnum.Comment)
                {
                    JObject j = JObject.Parse(danmakuModel.RawData);
                    int UserMedalLevel = 0;
                    string UserMedalName = null;
                    string UserTitle = j["info"][5].HasValues ? j["info"][5][1].ToString() : null;
                    int UserLevel = j["info"][4][0].ToObject<int>();
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
                    if (j["info"][3].HasValues)
                    {
                        UserMedalLevel = j["info"][3][0].ToObject<int>();
                        UserMedalName = j["info"][3][1].ToString();
                    }
                    string prefix = $"{(danmakuModel.isAdmin ? "[管]" : "")}{(danmakuModel.UserGuardLevel == 3 ? "[舰]" : danmakuModel.UserGuardLevel == 2 ? "[提]" : danmakuModel.UserGuardLevel == 1 ? "[总]" : null)}{(danmakuModel.isVIP ? "[爷]" : "")}{(LogMedal && !string.IsNullOrEmpty(UserMedalName) ? $"{{{UserMedalName},{UserMedalLevel}}}" : null)}{(LogTitle && !string.IsNullOrEmpty(UserTitle) ? $"[{UserTitle}]" : "")}{(LogLevel ? $"(UL {UserLevel})" : "")}{danmakuModel.UserName}";
                    Logging.Invoke(DmjWnd, new object[] { $"收到彈幕:{prefix} 說: {danmakuModel.CommentText}" });
                    AddDMText.Invoke(DmjWnd, new object[] { prefix, danmakuModel.CommentText, false, false });
                    SendSSP.Invoke(DmjWnd, new object[] { string.Format(@"\_q{0}\n\_q\f[height,20]{1}", prefix, danmakuModel.CommentText) });
                }
                else if (LogExternInfo)
                {
                    if (danmakuModel.MsgType == MsgTypeEnum.Unknown)
                    {
                        JObject j = JObject.Parse(danmakuModel.RawData);
                        string cmd = j["cmd"].ToString();
                        switch (cmd)
                        {
                            case "ROOM_SILENT_ON":
                                {
                                    string type = j["data"]["type"].ToString();
                                    int endTimeStamp = j["data"]["second"].ToObject<int>();
                                    string toLog = $"主播开启了房间禁言.类型:{(type == "member" ? "全体用户" : type == "medal" ? "粉丝勋章" : "用户等级")};{(type != "member" ? $"等级:{j["data"]["level"]};" : "")}时间:{(endTimeStamp == -1 ? "直到下播" : $"直到{new DateTime(1970, 1, 1).AddSeconds(endTimeStamp).ToLocalTime().ToString("yyyy-MM-dd HH:mm:ss")}")}";
                                    Logging.Invoke(DmjWnd, new object[] { $"系统通知:{toLog}" });
                                    AddDMText.Invoke(DmjWnd, new object[] { "系统通知", toLog, false, false });
                                    SendSSP.Invoke(DmjWnd, new object[] { string.Format(@"\_q{0}\n\_q\f[height,20]{1}", "系统通知", toLog) });
                                    break;
                                }
                            case "ROOM_SILENT_OFF":
                                {
                                    string toLog = "主播取消了房间禁言";
                                    Logging.Invoke(DmjWnd, new object[] { $"系统通知:{toLog}" });
                                    AddDMText.Invoke(DmjWnd, new object[] { "系统通知", toLog, false, false });
                                    SendSSP.Invoke(DmjWnd, new object[] { string.Format(@"\_q{0}\n\_q\f[height,20]{1}", "系统通知", toLog) });
                                    break;
                                }
                            case "ROOM_BLOCK_MSG":
                                {
                                    string toLog = $"用户 {j["uname"]}[{j["uid"]}] 已被管理员禁言";
                                    Logging.Invoke(DmjWnd, new object[] { $"系统通知:{toLog}" });
                                    AddDMText.Invoke(DmjWnd, new object[] { "系统通知", toLog, false, false });
                                    SendSSP.Invoke(DmjWnd, new object[] { string.Format(@"\_q{0}\n\_q\f[height,20]{1}", "系统通知", toLog) });
                                    break;
                                }
                            case "WARNING":
                                {
                                    string toLog = $"房间被警告:{j["msg"]}";
                                    Logging.Invoke(DmjWnd, new object[] { $"系统通知:{toLog}" });
                                    AddDMText.Invoke(DmjWnd, new object[] { "系统通知", toLog, false, false });
                                    SendSSP.Invoke(DmjWnd, new object[] { string.Format(@"\_q{0}\n\_q\f[height,20]{1}", "系统通知", toLog) });
                                    break;
                                }
                            case "CUT_OFF":
                                {
                                    string toLog = "当前直播间被管理员切断";
                                    Logging.Invoke(DmjWnd, new object[] { $"系统通知:{toLog}" });
                                    AddDMText.Invoke(DmjWnd, new object[] { "系统通知", toLog, false, false });
                                    SendSSP.Invoke(DmjWnd, new object[] { string.Format(@"\_q{0}\n\_q\f[height,20]{1}", "系统通知", toLog) });
                                    break;
                                }
                            case "ROOM_LOCK":
                                {
                                    string toLog = "当前直播间被管理员关闭";
                                    Logging.Invoke(DmjWnd, new object[] { $"系统通知:{toLog}" });
                                    AddDMText.Invoke(DmjWnd, new object[] { "系统通知", toLog, false, false });
                                    SendSSP.Invoke(DmjWnd, new object[] { string.Format(@"\_q{0}\n\_q\f[height,20]{1}", "系统通知", toLog) });
                                    break;
                                }
                            default:
                                {
                                    BaseProcDanmaku.Invoke(DmjWnd, new object[] { danmakuModel });
                                    break;
                                }
                        }
                    }
                    else if (danmakuModel.MsgType == MsgTypeEnum.LiveStart)
                    {
                        string toLog = "主播已开播";
                        Logging.Invoke(DmjWnd, new object[] { $"系统通知:{toLog}" });
                        AddDMText.Invoke(DmjWnd, new object[] { "系统通知", toLog, false, false });
                        SendSSP.Invoke(DmjWnd, new object[] { string.Format(@"\_q{0}\n\_q\f[height,20]{1}", "系统通知", toLog) });
                    }
                    else if (danmakuModel.MsgType == MsgTypeEnum.LiveEnd)
                    {
                        string toLog = "主播已下播";
                        Logging.Invoke(DmjWnd, new object[] { $"系统通知:{toLog}" });
                        AddDMText.Invoke(DmjWnd, new object[] { "系统通知", toLog, false, false });
                        SendSSP.Invoke(DmjWnd, new object[] { string.Format(@"\_q{0}\n\_q\f[height,20]{1}", "系统通知", toLog) });
                    }
                    else
                    {
                        BaseProcDanmaku.Invoke(DmjWnd, new object[] { danmakuModel });
                    }
                }
                else
                {
                    BaseProcDanmaku.Invoke(DmjWnd, new object[] { danmakuModel });
                }
            }
            else
            {
                BaseProcDanmaku.Invoke(DmjWnd, new object[] { danmakuModel });
            }
        }

        public override void Inited()
        {
            //Assembly dmAssembly = AppDomain.CurrentDomain.GetAssemblies().FirstOrDefault(p => p.FullName.StartsWith("Bililive_dm,"));
            //Type appType = dmAssembly.ExportedTypes.FirstOrDefault(p => p.FullName == "Bililive_dm.App");
            //Plugins = (ObservableCollection<DMPlugin>)appType.GetField("Plugins", BindingFlags.GetField | BindingFlags.Static | BindingFlags.Public).GetValue(null);
            //Type utilsType = dmAssembly.ExportedTypes.FirstOrDefault(p => p.FullName == "Bililive_dm.Utils");
        }

        public override void DeInit()
        {
            SettingsWnd.Closing -= SettingsWnd.Window_Closing;
            SettingsWnd.Close();
            WriteConfigTimer.Elapsed -= WriteConfigTimer_Elapsed;
            WriteConfigTimer.Dispose();
        }

        public override void Admin()
        {
            SettingsWnd.Show();
            SettingsWnd.Topmost = true;
            SettingsWnd.Topmost = false;
        }

        public override void Start()
        {
            base.Start();
            SaveConfig();
        }

        public override void Stop()
        {
            base.Stop();
            SaveConfig();
        }

        private void WriteConfigTimer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            try
            {
                string extLogPath = Path.Combine(PluginPath, "ExtendLogging");
                JObject j = new JObject(new JProperty("Enabled", this.Status), new JProperty("LogLevel", LogLevel), new JProperty("LogMedal", LogMedal),
                    new JProperty("LogTitle", LogTitle), new JProperty("LogExternInfo", LogExternInfo));
                File.WriteAllText(Path.Combine(extLogPath, "Config.cfg"), j.ToString());
            }
            catch (Exception Ex)
            {
                this.Log($"警告:保存配置文件出错:{Ex.ToString()}");
            }
        }

        private void SaveConfig()
        {
            WriteConfigTimer.Enabled = false;
            WriteConfigTimer.Enabled = true;
        }

        private void LoadConfig()
        {
            string extLogPath = Path.Combine(PluginPath, "ExtendLogging");
            if (!Directory.Exists(extLogPath))
            {
                Directory.CreateDirectory(extLogPath);
            }
            if (File.Exists(Path.Combine(extLogPath, "Config.cfg")))
            {
                try
                {
                    JObject j = JObject.Parse(File.ReadAllText(Path.Combine(extLogPath, "Config.cfg")));
                    LogLevel = j["LogLevel"].ToObject<bool>();
                    LogMedal = j["LogMedal"].ToObject<bool>();
                    LogTitle = j["LogTitle"].ToObject<bool>();
                    LogExternInfo = j["LogExternInfo"].ToObject<bool>();
                    if (j["Enabled"].ToObject<bool>())
                    {
                        this.Start();
                    }
                }
                catch (Exception Ex)
                {
                    this.Log($"警告:读取配置文件出错:{Ex.ToString()}");
                }
            }
        }

        public static void UpdateTitles()
        {
            string json = HttpGet("https://api.live.bilibili.com/rc/v1/Title/webTitles", 5);
            JObject j = JObject.Parse(json);
            int code = j["code"].ToObject<int>();
            if (code == 0)
            {
                foreach (JToken jt in j["data"].Where(p => !Titles.ContainsKey(p["identification"].ToString())))
                {
                    Titles.Add(jt["identification"].ToString(), jt["name"].ToString());
                }
            }
        }

        public static string GetUserNameByUserId(int userId)
        {
            IDictionary<string, string> headers = new Dictionary<string, string>
            {
                { "Origin", "https://space.bilibili.com" },
                { "Referer", $"https://space.bilibili.com/{userId}/" },
                { "X-Requested-With", "XMLHttpRequest" }
            };
            string json = HttpPost("https://space.bilibili.com/ajax/member/GetInfo", $"mid={userId}&csrf=", headers: headers);
            JObject j = JObject.Parse(json);
            if (j["status"].ToObject<bool>())
            {
                return j["data"]["name"].ToString();
            }
            else
            {
                throw new NotImplementedException($"未知的服务器返回:{j.ToString(0)}");
            }
        }

        public const string UserAgent = "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/49.0.2623.22 Safari/537.36 SE 2.X MetaSr 1.0";

        public static string HttpGet(string url, int timeout = 0, string userAgent = UserAgent, string cookie = null, IDictionary<string, string> headers = null)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Accept = "*/*";
            request.Method = "GET";
            request.AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate;
            if (timeout != 0) { request.Timeout = timeout * 1000; request.ReadWriteTimeout = timeout * 1000; }
            else request.ReadWriteTimeout = 10000;
            request.UserAgent = userAgent;
            if (!string.IsNullOrEmpty(cookie))
            {
                request.Headers.Add("Cookie", cookie);
            }
            if (headers != null)
            {
                foreach (string key in headers.Keys)
                {
                    if (key.ToLower() == "accept")
                        request.Accept = headers[key];
                    else if (key.ToLower() == "host")
                        request.Host = headers[key];
                    else if (key.ToLower() == "referer")
                        request.Referer = headers[key];
                    else
                        request.Headers.Add(key, headers[key]);
                }
            }
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                return reader.ReadToEnd();
        }

        public static string HttpPost(string url, string formdata, int timeout = 0, string userAgent = UserAgent, string cookies = null, IDictionary<string, string> headers = null)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Method = "POST";
            request.ContentType = "application/x-www-form-urlencoded";
            request.UserAgent = userAgent;
            if (timeout != 0) { request.Timeout = timeout * 1000; request.ReadWriteTimeout = timeout * 1000; }
            else request.ReadWriteTimeout = 10000;
            if (!string.IsNullOrEmpty(cookies))
            {
                request.Headers.Add("Cookie", cookies);
            }
            if (headers != null)
            {
                foreach (string key in headers.Keys)
                {
                    if (key.ToLower() == "accept")
                        request.Accept = headers[key];
                    else if (key.ToLower() == "host")
                        request.Host = headers[key];
                    else if (key.ToLower() == "referer")
                        request.Referer = headers[key];
                    else if (key.ToLower() == "content-type")
                        request.ContentType = headers[key];
                    else
                        request.Headers.Add(key, headers[key]);
                }
            }
            if (!string.IsNullOrEmpty(formdata))
            {
                byte[] data = Encoding.UTF8.GetBytes(formdata);
                using (Stream stream = request.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }
            }
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            using (Stream stream = response.GetResponseStream())
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                return reader.ReadToEnd();
        }
    }
}
