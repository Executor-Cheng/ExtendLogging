using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace ExtendLogging
{
    public static class BiliUtils
    {
        public static string GetUserNameByUserId(int userId)
        {
            IDictionary<string, string> headers = new Dictionary<string, string>
            {
                { "Origin", "https://space.bilibili.com" },
                { "Referer", $"https://space.bilibili.com/{userId}/" },
                { "X-Requested-With", "XMLHttpRequest" }
            };
            string json = HttpHelper.HttpPost("https://space.bilibili.com/ajax/member/GetInfo", $"mid={userId}&csrf=", headers: headers);
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
    }
}
