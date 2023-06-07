using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.SQLite;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using Newtonsoft.Json.Linq;
using Windows.Graphics.Printing;
using static System.Formats.Asn1.AsnWriter;
using System.Data;
using System.Threading;
using System.Xml.Serialization;
using Microsoft.UI.Xaml.Controls.Primitives;
using System.Data.SqlClient;
using Newtonsoft.Json;
using System.Xml.Linq;

namespace TheSteambird.api
{
    public class GenshinApi
    {

    }
    public class GenshinOverViewGetApi
    {
        private static readonly HttpClient client = new HttpClient();
        private readonly Dictionary<string, (DateTime, string)> cache = new Dictionary<string, (DateTime, string)>();
        private readonly TimeSpan cacheDuration = TimeSpan.FromMinutes(10);

        public async Task<string> GetAsync(string url, Dictionary<string, string> query = null, Dictionary<string, string> headers = null)
        {
            if (query != null)
            {
                url += "?" + string.Join("&", query.Select(kvp => $"{kvp.Key}={kvp.Value}"));
            }

            if (cache.TryGetValue(url, out var cachedData))
            {
                var (lastRequestTime, lastResponse) = cachedData;
                if (DateTime.Now - lastRequestTime < cacheDuration)
                {
                    return lastResponse;
                }
            }

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            if (headers != null)
            {
                foreach (var kvp in headers)
                {
                    request.Headers.Add(kvp.Key, kvp.Value);
                }
            }

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            cache[url] = (DateTime.Now, responseContent);

            return responseContent;
        }
        //返回的list，第一个是全局，第二个是每日
        public async Task<Dictionary<int, List<string>>> GetAllOverview(List<GenshinAccountData> AccountData)
        {
            Dictionary<int, List<string>> data = new();
            foreach (var account in AccountData)
            {
                List<string> list = await GetOverview(account);
                data.Add(account.Uid, list);
            }
            return data;
        }
        public async Task<List<string>> GetOverview(GenshinAccountData data)
        {
            string url;
            Dictionary<string, string> query, headers;
            List<string> res = new();
            //总览
            if (data.Platform == 0)
            {
                //国服
                url = "https://api-takumi-record.mihoyo.com/game_record/app/genshin/api/index";
                query = new() {
                    { "server", data.Server },
                    { "role_id", data.Uid.ToString()}
                };
                headers = new() {
                    {"Access-Control-Request-Headers","ds,x-rpc-app_version,x-rpc-client_type,x-rpc-page"},
                    {"Access-Control-Request-Method","GET"},
                    {"Accept","application/json, text/plain, */*"},
                    {"Accept-Encoding","deflate"},
                    {"Accept-Language","zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7"},
                    {"Connection","keep-alive"},
                    {"DS",GetMysDS(data.Server, data.Uid.ToString())},
                    {"Host","api-takumi-record.mihoyo.com"},
                    {"Origin","https://webstatic.mihoyo.com"},
                    {"Referer","https://webstatic.mihoyo.com/"},
                    {"Sec-Fecth-Dest","empty"},
                    {"Sec-Fecth-Mode","cors"},
                    {"Sec-Fetch-Site","cookiessame-site"},
                    {"X-Requested-With","com.mihoyo.hyperion"},
                    {"x-rpc-app_version","2.35.2"},
                    {"x-rpc-client_type","5"},
                    {"x-rpc-page","/ys"},
                    {"Cookie",data.Cookies}
                };
            }
            else
            {
                //国际服
                url = "bbs-api-os.hoyolab.com";
                query = new() {
                    { "server", data.Server },
                    { "role_id", data.Uid.ToString()}
                };
                headers = new() {
                    {"ds",GetHoyoDS()},
                    {"x-rpc-app_version","1.5.0"},
                    {"x-rpc-client_type","5"},
                    {"Cookie",data.Cookies}
                };
            }
            var response = await GetAsync(url, query, headers);
            res.Add(response);
            //体力
            if (data.Platform == 0)
            {
                url = "https://api-takumi-record.mihoyo.com/game_record/app/genshin/api/dailyNote";
                query = new() {
                    { "role_id", data.Uid.ToString()},
                    { "server", data.Server}
                };
                headers = new() {
                    {"Access-Control-Request-Headers","ds,x-rpc-app_version,x-rpc-client_type,x-rpc-page"},
                    {"Access-Control-Request-Method","GET"},
                    {"Accept","application/json, text/plain, */*"},
                    {"Accept-Encoding","deflate"},
                    {"Accept-Language","zh-CN,zh;q=0.9,en-US;q=0.8,en;q=0.7"},
                    {"Connection","keep-alive"},
                    {"DS",GetMysDS(data.Server, data.Uid.ToString())},
                    {"Host","api-takumi-record.mihoyo.com"},
                    {"Origin","https://webstatic.mihoyo.com"},
                    {"Referer","https://webstatic.mihoyo.com/"},
                    {"Sec-Fecth-Dest","empty"},
                    {"Sec-Fecth-Mode","cors"},
                    {"Sec-Fetch-Site","cookiessame-site"},
                    {"X-Requested-With","com.mihoyo.hyperion"},
                    {"x-rpc-app_version","2.35.2"},
                    {"x-rpc-client_type","5"},
                    {"x-rpc-page","/ys/daily/"},
                    {"Cookie",data.Cookies}
                };
            }
            else
            {
                url = "https://bbs-api-os.hoyolab.com/game_record/genshin/api/dailyNote";
                query = new() {
                    { "role_id", data.Uid.ToString()},
                    { "server", data.Server}
                };
                headers = new() {
                    {"User-Agent","Mozilla/5.0 (Linux; Android 9; OPPO R11 Plus Build/NMF26X; wv) AppleWebKit/537.36 (KHTML, like Gecko) Version/4.0 Chrome/92.0.4515.131 Mobile Safari/537.36"},
                    {"ds",GetHoyoDS()},
                    {"x-rpc-app_version","1.5.0"},
                    {"x-rpc-client_type","5"},
                    {"Cookie",data.Cookies}
                };
            }
            var response_daily = await GetAsync(url, query, headers);
            res.Add(response_daily);
            return res;
        }
        private string GetMysDS(string region, string uid)
        {
            string salt = "xV8v4Qu54lUKrEYFZkJhB8cuOh9Asafs";
            string t = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
            int random = new Random().Next(100000);
            string r = (random + 100000).ToString();
            string q = "role_id=" + uid + "&server=" + region;
            string DS = "salt=" + salt + "&t=" + t + "&r=" + r + "&b=&q=" + q;
            string DSMD5 = Api.MD5Encrypt(DS);
            string DSresult = t + "," + r + "," + DSMD5;
            return DSresult;
        }
        private string GetHoyoDS()
        {
            string salt = "6s25p5ox5y14umn1p61aqyyvbvvl3lrt";
            int t = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            string r = "";
            Random generator = new Random(t);
            for (int i = 0; i < 6; i++)
            {
                int v8 = generator.Next(0, 32768) % 26;
                int v9 = 87;
                if (v8 < 10)
                {
                    v9 = 48;
                }
                r += (char)(v8 + v9);
            }
            string DS = "salt=" + salt + "&t=" + t.ToString() + "&r=" + r;
            string DSMD5 = Api.MD5Encrypt(DS);
            return t.ToString() + "," + r + "," + DSMD5;
        }
    }
    public class GenshinAbyssApi
    {
        public static async Task<List<string>> GetAbyss(GenshinAccountData data)
        {
            Func<GenshinAccountData, int, Task<string>> GetMysAbyss = async (account, schedule_type) =>
            {
                Dictionary<string, string> query = new() {
                    { "schedule_type", schedule_type.ToString()},
                    { "role_id", account.Uid.ToString()},
                    { "server", account.Server}
                };
                Dictionary<string, string> headers = new() {
                    {"Cookie", account.Cookies},
                    {"x-rpc-app_version", "2.45.1"},
                    {"x-rpc-client_type", "5"},
                    {"DS", GetMysDS(schedule_type, account.Uid, account.Server)}
                };
                return Api.HttpGet("https://api-takumi-record.mihoyo.com/game_record/app/genshin/api/spiralAbyss", query, headers);
            };
            Func<GenshinAccountData, int, Task<string>> GetHoyoAbyss = async (account, schedule_type) =>
            {
                Dictionary<string, string> query = new() {
                    { "schedule_type", schedule_type.ToString()},
                    { "role_id", account.Uid.ToString()},
                    { "server", account.Server}
                };
                Dictionary<string, string> headers = new() {
                    {"Cookie", account.Cookies},
                    {"x-rpc-app_version", "2.45.1"},
                    {"x-rpc-client_type", "5"},
                    {"DS", GetHoyoDS()}
                };
                return Api.HttpGet("https://bbs-api-os.hoyolab.com/game_record/genshin/api/spiralAbyss", query, headers);
            };
            Task<string> thisAbyss, lastAbyss;
            if (data.Platform == 0)
            {
                thisAbyss = GetMysAbyss(data, 1);
                lastAbyss = GetMysAbyss(data, 2);
            }
            else
            {
                thisAbyss = GetHoyoAbyss(data, 1);
                lastAbyss = GetHoyoAbyss(data, 2);
            }
            await Task.WhenAll(thisAbyss, lastAbyss);
            List<string> list = new() { thisAbyss.Result, lastAbyss.Result };
            return list;
        }
        public static void UpdateAbyssSql(List<string> datas, int uid)
        {
            if (!Api.IsExistsSqlTable(GlobalVar.GenshinAbyssSqlPath, uid.ToString()))
            {
                Api.CreateSqlTable(GlobalVar.GenshinAbyssSqlPath, uid.ToString(), GlobalVar.GenshinAbyssSqlAddRow);
            }
            foreach (var data in datas)
            {
                var Serializer = new JavaScriptSerializer();
                var obj = Serializer.Deserialize<dynamic>(data);
                int retcode = obj["retcode"];
                if (retcode != 0)
                {
                    return;
                }
                var dataobj = obj["data"];
                int schedule_id = dataobj["schedule_id"];
                //先查找有没有旧数据，有就删除
                using (SQLiteConnection conn = new SQLiteConnection($"Data Source={GlobalVar.GenshinAbyssSqlPath};Version=3;"))
                {
                    conn.Open();
                    string sql = $"DELETE FROM '{uid.ToString()}' WHERE Id=@id";
                    using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                    {
                        cmd.Parameters.AddWithValue("@Id", schedule_id);
                        int rowsAffected = cmd.ExecuteNonQuery();
                        if (rowsAffected > 0)
                        {
                            Console.WriteLine("数据删除成功");
                        }
                        else
                        {
                            Console.WriteLine("没有找到匹配的数据");
                        }
                    }
                }
                using (SQLiteConnection m_dbConnection = new SQLiteConnection($"Data Source={GlobalVar.GenshinAbyssSqlPath};Version=3;"))
                {

                    try
                    {
                        //json转string
                        JObject jsonObj = JObject.Parse(data);
                        string dataobjToString = jsonObj["data"].ToString();
                        m_dbConnection.Open();
                        string sql = $"INSERT INTO '{uid.ToString()}' (Id, Data) VALUES (@Id, @Data)";
                        SQLiteCommand cmd = new SQLiteCommand(sql, m_dbConnection);
                        cmd.Parameters.Add(new SQLiteParameter("@Id", schedule_id));
                        cmd.Parameters.Add(new SQLiteParameter("@Data", dataobjToString));
                        cmd.ExecuteNonQuery();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                    finally
                    {
                        m_dbConnection.Close();
                    }
                }
            }
        }
        public static string ReadAbyssSqlData(string uid, int id)
        {
            using (var connection = new SQLiteConnection($"Data Source={GlobalVar.GenshinAbyssSqlPath}"))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = $"SELECT Data FROM '{uid}' WHERE ID = $id";
                command.Parameters.AddWithValue("$id", id);
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var data = reader.GetString(0);
                        Console.WriteLine(data);
                        return data;
                    }
                }
            }
            return "{}";
        }
        private static string GetMysDS(int schedule_type, int uid, string server)
        {
            string salt = "xV8v4Qu54lUKrEYFZkJhB8cuOh9Asafs";
            string t = DateTimeOffset.Now.ToUnixTimeSeconds().ToString();
            int random = new Random().Next(100000);
            string r = (random + 100000).ToString();
            string q = $"role_id={uid.ToString()}&schedule_type={schedule_type.ToString()}&server={server}";
            string DS = "salt=" + salt + "&t=" + t + "&r=" + r + "&b=&q=" + q;
            string DSMD5 = Api.MD5Encrypt(DS);
            string DSresult = t + "," + r + "," + DSMD5;
            return DSresult;
        }
        private static string GetHoyoDS()
        {
            string salt = "6s25p5ox5y14umn1p61aqyyvbvvl3lrt";
            int t = (int)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1))).TotalSeconds;
            string r = "";
            Random generator = new Random(t);
            for (int i = 0; i < 6; i++)
            {
                int v8 = generator.Next(0, 32768) % 26;
                int v9 = 87;
                if (v8 < 10)
                {
                    v9 = 48;
                }
                r += (char)(v8 + v9);
            }
            string DS = "salt=" + salt + "&t=" + t.ToString() + "&r=" + r;
            string DSMD5 = Api.MD5Encrypt(DS);
            return t.ToString() + "," + r + "," + DSMD5;
        }
    }
    public class GenshinGachaApi
    {
        public static async Task<List<List<GenshinGachaData>>> FromCNGetGacha(string CNpath)
        {
            string path = Path.GetDirectoryName(CNpath) + "\\YuanShen_Data\\webCaches\\Cache\\Cache_Data\\data_2";
            string file = Api.ReadFile(path);
            if (file == "")
            {
                return new List<List<GenshinGachaData>>();
            }
            string url;
            string pattern = "e20190909gacha-v2";
            Match match = Regex.Match(file, pattern, RegexOptions.RightToLeft);
            url = file.Substring(match.Index);
            url = url.Substring(url.IndexOf("hk4e-api.mihoyo.com"));
            url = url.Substring(0, url.IndexOf("game_biz") + 50);
            url = url.Substring(0, url.LastIndexOf("&gacha_type"));
            url = "https://" + url;
            List<List<GenshinGachaData>> datas = new();
            //用异步同时进行3次get
            var task200 = Task.Run(() => GetGacha(url, 200));
            var task301 = Task.Run(() => GetGacha(url, 301));
            var task302 = Task.Run(() => GetGacha(url, 302));
            var task400 = Task.Run(() => GetGacha(url, 400));
            await Task.WhenAll(task200, task301, task302, task400);

            datas.Add(task200.Result);
            datas.Add(task301.Result);
            datas.Add(task302.Result);
            datas.Add(task400.Result);

            return datas;
        }
        public static async Task<List<List<GenshinGachaData>>> FromOSGetGacha(string OSpath)
        {
            string path = Path.GetDirectoryName(OSpath) + "\\GenshinImpact_Data\\webCaches\\Cache\\Cache_Data\\data_2";
            string file = Api.ReadFile(path);
            if (file == "")
            {
                return new List<List<GenshinGachaData>>();
            }
            string url;
            string pattern = "e20190909gacha-v2";
            Match match = Regex.Match(file, pattern, RegexOptions.RightToLeft);
            url = file.Substring(match.Index);
            url = url.Substring(url.IndexOf("hk4e-api-os.hoyoverse.com"));
            url = url.Substring(0, url.IndexOf("game_biz") + 50);
            url = url.Substring(0, url.LastIndexOf("&gacha_type"));
            url = "https://" + url;
            List<List<GenshinGachaData>> datas = new();
            //用异步同时进行3次get
            var task200 = Task.Run(() => GetGacha(url, 200));
            var task301 = Task.Run(() => GetGacha(url, 301));
            var task302 = Task.Run(() => GetGacha(url, 302));
            var task400 = Task.Run(() => GetGacha(url, 400));
            await Task.WhenAll(task200, task301, task302, task400);

            datas.Add(task200.Result);
            datas.Add(task301.Result);
            datas.Add(task302.Result);
            datas.Add(task400.Result);

            return datas;
        }
        public static async Task<List<List<GenshinGachaData>>> FromUrlGetGacha(string url)
        {
            List<List<GenshinGachaData>> datas = new();
            //用异步同时进行3次get
            var task200 = Task.Run(() => GetGacha(url, 200));
            var task301 = Task.Run(() => GetGacha(url, 301));
            var task302 = Task.Run(() => GetGacha(url, 302));
            var task400 = Task.Run(() => GetGacha(url, 400));
            await Task.WhenAll(task200, task301, task302, task400);

            datas.Add(task200.Result);
            datas.Add(task301.Result);
            datas.Add(task302.Result);
            datas.Add(task400.Result);

            return datas;
        }
        public static void AddGachaToSql(List<List<GenshinGachaData>> datas, List<List<GenshinGachaData>> listdatas)
        {
            string uid = "";
            bool exitLoop = false;
            foreach(var listdata  in listdatas)
            {
                foreach (var item in listdata)
                {
                    if (item.Uid != null)
                    { 
                        uid = item.Uid;
                        exitLoop = true;
                        break;
                    }
                }
                if (exitLoop)
                {
                    break;
                }
            }
            if (!Api.IsExistsSqlTable(GlobalVar.GenshinGachaSqlPath, uid))
            {
                Api.CreateSqlTable(GlobalVar.GenshinGachaSqlPath, uid, GlobalVar.GenshinGachaSqlAddRow);
            }
            using (var connection = new SQLiteConnection($"Data Source={GlobalVar.GenshinGachaSqlPath};Version=3;"))
            {
                connection.Open();
                foreach (var data in datas)
                {
                    foreach (var item in data)
                    {
                        string delete_sql = $"DELETE FROM '{uid}' WHERE Id=@id";
                        using (SQLiteCommand cmd = new SQLiteCommand(delete_sql, connection))
                        {
                            cmd.Parameters.AddWithValue("@id", item.Id);
                            cmd.ExecuteNonQuery();
                        }
                        string sql = $"INSERT INTO '{uid}' ({GlobalVar.GenshinGachaSqlRow}) VALUES ('{item.Id}', {item.Gacha_type}, '{item.Date}', '{item.Name}', '{item.Item_type}', {item.Rank_type})";
                        using (SQLiteCommand cmd = new SQLiteCommand(sql, connection))
                        {
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
            }
        }
        private static async Task<List<GenshinGachaData>> GetGacha(string url, int type)
        {
            List<GenshinGachaData> data = new List<GenshinGachaData>();
            const int size = 10;//一次获取10个
            int page = 1;
            string endId = "0";
            bool start = true;
            while (start)
            {
                start = false;//防止网络无响应死循环
                string nowUrl = $"{url}&gacha_type={type.ToString()}&page={page.ToString()}&size={size.ToString()}&end_id={endId}";
                var client = new HttpClient();
                var response = client.GetAsync(nowUrl).Result;
                string result = response.Content.ReadAsStringAsync().Result;
                var json = JObject.Parse(result);
                int retcode = (int)json["retcode"];
                if (retcode == 0)
                {
                    JArray dataarray = (JArray)json["data"]["list"];
                    int count = dataarray.Count;
                    if (count == size)
                    {
                        start = true;
                    }
                    else if (count == 0)
                    {
                        break;
                    }
                    foreach (JObject item in dataarray)
                    {
                        string uid = (string)item["uid"];
                        string id = (string)item["id"];
                        string gacha_type = (string)item["gacha_type"];
                        string rank_type = (string)item["rank_type"];
                        string name = (string)item["name"];
                        string date = (string)item["time"];
                        string item_type = (string)item["item_type"];
                        GenshinGachaData genshinGachaData = new(id, int.Parse(gacha_type), int.Parse(rank_type), name, date, item_type, uid);
                        data.Add(genshinGachaData);
                        endId = id;
                    }
                    page++;
                    Thread.Sleep(200);
                }
            }
            return data;
        }
    }
    public class GenshinEnkaNetworkAPi {
        enum GetStatus { 
            Success = 200,
            UID格式错误 = 400,
            玩家不存在 = 404,
            游戏维护中 = 424,
            服务器错误 = 500,
            我搞砸了 = 503
        }
        public enum FightProp
        {
            基础生命值 = 1,
            生命值 = 2,
            生命值百分比 = 3,
            基础攻击力 = 4,
            攻击力0 = 5,       //未知用法
            攻击力百分比 = 6,
            基础防御力 = 7,
            防御力0 = 8,       //未知用法
            百分比防御力 = 9,
            基础速度 = 10,
            速度百分比 = 11,
            暴击率 = 20,
            暴击伤害 = 22,
            元素充能效率 = 23,
            治疗加成 = 26,
            受治疗加成 = 27,
            元素精通 = 28,
            物理抗性 = 29,
            物理伤害加成 = 30,
            火元素伤害加成 = 40,
            雷元素伤害加成 = 41,
            水元素伤害加成 = 42,
            草元素伤害加成 = 43,
            风元素伤害加成 = 44,
            岩元素伤害加成 = 45,
            冰元素伤害加成 = 46,
            火元素抗性 = 50,
            雷元素抗性 = 51,
            水元素抗性 = 52,
            草元素抗性 = 53,
            风元素抗性 = 54,
            岩元素抗性 = 55,
            冰元素抗性 = 56,
            火元素能量需求 = 70 ,      //元素爆发
            雷元素能量需求 = 71 ,      //元素爆发
            水元素能量需求 = 72 ,      //元素爆发
            草元素能量需求 = 73 ,      //元素爆发
            风元素能量需求 = 74 ,      //元素爆发
            冰元素能量需求 = 75 ,      //元素爆发
            岩元素能量需求 = 76 ,      //元素爆发
            冷却缩减 = 80,
            护盾强效 = 81,
            当前火元素能量 = 1000,
            当前雷元素能量 = 1001,
            当前水元素能量 = 1002,
            当前草元素能量 = 1003,
            当前风元素能量 = 1004,
            当前冰元素能量 = 1005,
            当前岩元素能量 = 1006,
            当前生命值 = 1010,
            生命值上限 = 2000,
            攻击力 = 2001,
            防御力 = 2002,
            速度 = 2003,
            元素反应暴击率 = 3025,
            元素反应暴击伤害 = 3026,
            元素反应超载暴击率 = 3027,
            元素反应超载暴击伤害 = 3028,
            元素反应扩散暴击率 = 3029,
            元素反应扩散暴击伤害 = 3030,
            元素反应感电暴击率 = 3031,
            元素反应感电暴击伤害 = 3032,
            元素反应超导暴击率 = 3033,
            元素反应超导暴击伤害 = 3034,
            元素反应燃烧暴击率 = 3035,
            元素反应燃烧暴击伤害 = 3036,
            元素反应碎冰暴击率 = 3037,
            元素反应碎冰暴击伤害 = 3038,
            元素反应绽放暴击率 = 3039,
            元素反应绽放暴击伤害 = 3040,
            元素反应烈绽放暴击率 = 3041,
            元素反应烈绽放暴击伤害 = 3042,
            元素反应超绽放暴击率 = 3043,
            元素反应超绽放暴击伤害 = 3044,
            基础元素反应暴击率 = 3045,
            基础元素反应暴击伤害 = 3046
        }
        public static readonly Dictionary<string, string> appendProp = new Dictionary<string, string>
        {
            { "FIGHT_PROP_BASE_ATTACK", "基础攻击力" },
            { "FIGHT_PROP_HP", "生命值" },
            { "FIGHT_PROP_ATTACK", "攻击力" },
            { "FIGHT_PROP_DEFENSE", "防御力" },
            { "FIGHT_PROP_HP_PERCENT", "生命值百分比" },
            { "FIGHT_PROP_ATTACK_PERCENT", "攻击力百分比" },
            { "FIGHT_PROP_DEFENSE_PERCENT", "防御力百分比" },
            { "FIGHT_PROP_CRITICAL", "暴击率" },
            { "FIGHT_PROP_CRITICAL_HURT", "暴击伤害" },
            { "FIGHT_PROP_CHARGE_EFFICIENCY", "元素充能效率" },
            { "FIGHT_PROP_HEAL_ADD", "治疗加成" },
            { "FIGHT_PROP_ELEMENT_MASTERY", "元素精通" },
            { "FIGHT_PROP_PHYSICAL_ADD_HURT", "物理伤害加成" },
            { "FIGHT_PROP_FIRE_ADD_HURT", "火元素伤害加成" },
            { "FIGHT_PROP_ELEC_ADD_HURT", "雷元素伤害加成" },
            { "FIGHT_PROP_WATER_ADD_HURT", "水元素伤害加成" },
            { "FIGHT_PROP_WIND_ADD_HURT", "风元素伤害加成" },
            { "FIGHT_PROP_ICE_ADD_HURT", "冰元素伤害加成" },
            { "FIGHT_PROP_ROCK_ADD_HURT", "岩元素伤害加成" },
            { "FIGHT_PROP_GRASS_ADD_HURT",  "草元素伤害加成"}
        };
        public static string GetUIDData(string uid) {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create($"https://enka.network/api/uid/{uid}/");
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            int statusCode = (int)response.StatusCode;
            if (statusCode == 200)
            {
                Stream receiveStream = response.GetResponseStream();
                StreamReader readStream = new StreamReader(receiveStream, Encoding.UTF8);
                string content = readStream.ReadToEnd();
                return content;
            }
            else {
                GetStatus status = (GetStatus)statusCode;
                throw new Exception(status.ToString());
            }
        }
        //把数据存到sql里面ss
        public static void ProcessData(string data) { 
            JObject obj = JObject.Parse(data);  
            string uid = (string)obj["uid"];
            if (!obj.ContainsKey("playerInfo"))
            { 
                throw new Exception("未找到角色信息");
            }
            if (!obj.ContainsKey("avatarInfoList"))
            {
                throw new Exception("可能未公开角色展柜");
            }
            JObject playerInfo = (JObject)obj["playerInfo"];
            JArray avatarInfoList = (JArray)obj["avatarInfoList"];
            string signature;
            if (playerInfo.ContainsKey("signature"))
            {
                signature = (string)playerInfo["signature"];
            }
            else {
                signature = "";
            }

            //处理sql表
            if (!Api.IsExistsSqlTable(GlobalVar.GenshinEnkaNetworkSqlPath, uid))
            {
                Api.CreateSqlTable(GlobalVar.GenshinEnkaNetworkSqlPath, uid, GlobalVar.GenshinEnkaNetworkSqlDataAddRow);
            }

            string connectionString = $"Data Source={GlobalVar.GenshinEnkaNetworkSqlPath};Version=3;";
            foreach (JObject avatar in avatarInfoList)
            {
                int avatarID = (int)avatar["avatarId"];
                JObject propMap = (JObject)avatar["propMap"];
                JObject fightPropMap = (JObject)avatar["fightPropMap"];
                int skillDepotId = (int)avatar["skillDepotId"];
                JArray inherentProudSkillList = (JArray)avatar["inherentProudSkillList"];
                JObject skillLevelMap = (JObject)avatar["skillLevelMap"];
                JArray equipList = (JArray)avatar["equipList"];
                JObject fetterInfo = (JObject)avatar["fetterInfo"];
                int expLevel = (int)fetterInfo["expLevel"];
                string talentIdList = "";
                if (avatar.ContainsKey("talentIdList"))
                {
                    JArray talent = (JArray)avatar["talentIdList"];
                    talentIdList = talent.ToString();
                }
                using (SQLiteConnection connection = new SQLiteConnection(connectionString))
                {
                    connection.Open();
                    string deleteInfoSql = $"DELETE FROM '{uid}' WHERE AvatarID = @AvatarID";
                    using (SQLiteCommand command = new SQLiteCommand(deleteInfoSql, connection))
                    {
                        command.Parameters.AddWithValue("@AvatarID", avatarID);
                        command.ExecuteNonQuery();
                    }
                    using (SQLiteCommand command = new SQLiteCommand(connection))
                    {
                        command.CommandText =
                            $"INSERT INTO '{uid}' ({GlobalVar.GenshinEnkaNetworkSqlDataRow}) VALUES (@AvatarID, @TalentIdList, @PropMap, @FightPropMap, @SkillDepotId, @InherentProudSkillList, @SkillLevelMap, @EquipList, @ExpLevel)";
                        command.Parameters.AddWithValue("@AvatarID", avatarID);
                        command.Parameters.AddWithValue("@TalentIdList", talentIdList);
                        command.Parameters.AddWithValue("@PropMap", propMap.ToString());
                        command.Parameters.AddWithValue("@FightPropMap", fightPropMap.ToString());
                        command.Parameters.AddWithValue("@SkillDepotId", skillDepotId);
                        command.Parameters.AddWithValue("@InherentProudSkillList", inherentProudSkillList.ToString());
                        command.Parameters.AddWithValue("@SkillLevelMap", skillLevelMap.ToString());
                        command.Parameters.AddWithValue("@EquipList", equipList.ToString());
                        command.Parameters.AddWithValue("@ExpLevel", expLevel);
                        command.ExecuteNonQuery();
                    }
                }
            }
           

            
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string deleteInfoSql = "DELETE FROM info WHERE Uid = @uid";
                using (SQLiteCommand command = new SQLiteCommand(deleteInfoSql, connection))
                {
                    command.Parameters.AddWithValue("@uid", int.Parse(uid));
                    command.ExecuteNonQuery();
                }
                string sql = $"INSERT INTO info ({GlobalVar.GenshinEnkaNetworkSqlInfoRow}) VALUES (@uid, @nickname, @level, @signature, @worldlevel, @namecardid, @profilepicture)";
                using (SQLiteCommand command = new SQLiteCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@uid", int.Parse(uid));
                    command.Parameters.AddWithValue("@nickname", (string)playerInfo["nickname"]);
                    command.Parameters.AddWithValue("@level", (int)playerInfo["level"]);
                    command.Parameters.AddWithValue("@signature", signature);
                    command.Parameters.AddWithValue("@worldlevel", (int)playerInfo["worldLevel"]);
                    command.Parameters.AddWithValue("@namecardid", (int)playerInfo["nameCardId"]);
                    JObject profilePicture = (JObject)playerInfo["profilePicture"];
                    command.Parameters.AddWithValue("@profilepicture", (int)profilePicture["avatarId"]);
                    command.ExecuteNonQuery();
                }
            }
        }
        public static List<GenshinEnkaNetworkAvatarData> LoadSqlDataToList(string uid)
        {
            List<GenshinEnkaNetworkAvatarData> datas = new();
            string connectionString = $"Data Source={GlobalVar.GenshinEnkaNetworkSqlPath};Version=3;";
            string queryString = $"SELECT * FROM '{uid}'";

            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                SQLiteCommand command = new SQLiteCommand(queryString, connection);
                connection.Open();
                SQLiteDataReader reader = command.ExecuteReader();
                while (reader.Read())
                {
                    int avatarID = (int)reader["AvatarID"];
                    string talentIdList = reader["TalentIdList"].ToString();
                    string propMap = reader["PropMap"].ToString();
                    string fightPropMap = reader["FightPropMap"].ToString();
                    int skillDepotId = (int)reader["SkillDepotId"];
                    string inherentProudSkillList = reader["InherentProudSkillList"].ToString();
                    string skillLevelMap = reader["SkillLevelMap"].ToString();
                    string equipList = reader["EquipList"].ToString();
                    int expLevel = (int)reader["ExpLevel"];
                    datas.Add(LoadSingleData(avatarID, talentIdList, propMap, fightPropMap, skillDepotId, inherentProudSkillList, skillLevelMap, equipList, expLevel));
                }
                reader.Close();
            }
            return datas;
        }
        private static GenshinEnkaNetworkAvatarData LoadSingleData(int avatarID, string talentIdList, string propMap, string fightPropMap, int skillDepotId, string inherentProudSkillList, string skillLevelMap, string equipList, int expLevel)
        {
            List<int> talentIDs = new();
            if (talentIdList != "")
            {
                talentIDs = JsonConvert.DeserializeObject<List<int>>(talentIdList);
            }
            JObject prop = JObject.Parse(propMap);
            Dictionary<string, string> propMaps = new();
            foreach (var property in prop.Properties())
            {
                if (property.Value["val"] != null)
                {
                    propMaps.Add(property.Name, property.Value["val"].ToString());
                }
            }

            JObject fight = JObject.Parse(fightPropMap);
            Dictionary<string, double> fightProp = new Dictionary<string, double>();
            foreach (var property in fight.Properties())
            {
                fightProp.Add(property.Name, Convert.ToDouble(property.Value));
            }

            List<int> inherentProudSkillLists = JsonConvert.DeserializeObject<List<int>>(inherentProudSkillList);

            JObject skillLevel = JObject.Parse(skillLevelMap);
            Dictionary<string, int> skillLevelMaps = new();
            foreach (var property in skillLevel.Properties())
            {
                skillLevelMaps.Add(property.Name, Convert.ToInt32(property.Value));
            }

            JArray equip = JArray.Parse(equipList);
            GenshinEnkaNetworkWeapon weapon = new GenshinEnkaNetworkWeapon();
            List<GenshinEnkaNetworkReliquary> reliquaryList = new List<GenshinEnkaNetworkReliquary>();
            foreach (JObject equipobj in equip)
            {
                JObject flat = (JObject)equipobj["flat"];
                string itemType = (string)flat["itemType"];
                if (itemType == "ITEM_WEAPON")
                {
                    //武器
                    int itemId = (int)equipobj["itemId"];
                    JObject weaponobj = (JObject)equipobj["weapon"];
                    int level = (int)weaponobj["level"];
                    int promoteLevel = 0;
                    if (weaponobj.ContainsKey("promoteLevel"))
                        promoteLevel = (int)weaponobj["promoteLevel"];
                    int affixMapValue = 0;
                    if (weaponobj.ContainsKey("affixMap"))
                    { 
                        JObject affixMap = (JObject)weaponobj["affixMap"];
                        JProperty property = affixMap.Properties().First();
                        affixMapValue = property.Value.ToObject<int>();
                    }
                    string nameTextHashMap = (string)flat["nameTextMapHash"];
                    int rankLevel = (int)flat["rankLevel"];
                    Dictionary<string, double> weaponStats = new();
                    JArray weaponStatsArr = (JArray)flat["weaponStats"];
                    foreach (JObject Stats in weaponStatsArr) { 
                        string appendPropId = (string)Stats["appendPropId"];
                        double statValue = (double)Stats["statValue"];
                        weaponStats.Add(appendPropId, statValue);
                    }
                    string icon = (string)flat["icon"];

                    GenshinEnkaNetworkWeaponFlat weaponFlat = new GenshinEnkaNetworkWeaponFlat(nameTextHashMap, rankLevel, weaponStats, itemType, icon);
                    weapon = new GenshinEnkaNetworkWeapon(itemId, level, promoteLevel, affixMapValue, weaponFlat);
                }
                else if (itemType == "ITEM_RELIQUARY")
                {
                    //圣遗物
                    int itemId = (int)equipobj["itemId"];
                    JObject reliquaryobj = (JObject)equipobj["reliquary"];
                    int level = (int)reliquaryobj["level"];
                    int mainPropId = (int)reliquaryobj["mainPropId"];
                    JArray appendPropIdListArr = (JArray)reliquaryobj["appendPropIdList"];
                    List<int> appendPropIdList = appendPropIdListArr.ToObject<List<int>>();

                    string nameTextHashMap = (string)flat["nameTextMapHash"];
                    string setNameTextMapHash = (string)flat["setNameTextMapHash"];
                    int rankLevel = (int)flat["rankLevel"];
                    JObject reliquaryMainstatobj = (JObject)flat["reliquaryMainstat"];
                    Dictionary<string, double> reliquaryMainstat = new Dictionary<string, double>();
                    reliquaryMainstat.Add((string)reliquaryMainstatobj["mainPropId"], (double)reliquaryMainstatobj["statValue"]);
                    JArray reliquarySubstatsArr = (JArray)flat["reliquarySubstats"];
                    Dictionary<string, double> reliquarySubstats = new Dictionary<string, double>();
                    foreach (JObject eliquarySubstatsObj in reliquarySubstatsArr)
                    {
                        reliquarySubstats.Add((string)eliquarySubstatsObj["appendPropId"], (double)eliquarySubstatsObj["statValue"]);
                    }
                    string icon = (string)flat["icon"];
                    string equipType = (string)flat["equipType"];

                    GenshinEnkaNetworkReliquaryFlat reliquaryFlat = new GenshinEnkaNetworkReliquaryFlat(nameTextHashMap, setNameTextMapHash, rankLevel, reliquaryMainstat, reliquarySubstats, itemType, icon, equipType);
                    GenshinEnkaNetworkReliquary reliquary = new GenshinEnkaNetworkReliquary(itemId, level, mainPropId, appendPropIdList, reliquaryFlat);
                    reliquaryList.Add(reliquary);
                }
            }

            return new GenshinEnkaNetworkAvatarData(avatarID, talentIDs, propMaps, fightProp, skillDepotId, inherentProudSkillLists, skillLevelMaps, weapon, reliquaryList, expLevel);
        }
        public static string GetIcon(string iconName) {
            string url = $"https://enka.network/ui/{iconName}.png";
            return TheSteambirdApi.DownloadImage(url, "res/genshin/EnkaNetwork/");
        }
    }
}
