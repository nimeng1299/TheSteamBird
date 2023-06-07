using System;
using System.IO;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Linq;
using System.Net;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Xml.Linq;
using Windows.Foundation.Metadata;
using Newtonsoft.Json.Linq;

namespace TheSteambird.api
{
    //给项目专属的api
    public class TheSteambirdApi
    {
        static readonly string genshinMysApiUrl = "https://api-takumi.mihoyo.com";
        static readonly string genshinHoyoApiUrl = "https://api-os-takumi.hoyoverse.com";
        //读取所有账号
        public static List<GenshinAccountData> GetGenshinDataFromSql(string filePath) {
            var datas = new List<GenshinAccountData>();
            SQLiteConnection m_dbConnection = new SQLiteConnection($"Data Source={filePath};Version=3;");
            try
            {
                m_dbConnection.Open();
                string sql = $"SELECT * FROM {GlobalVar.GenshinAccountSqlTable}";
                SQLiteCommand cmd = new SQLiteCommand(sql, m_dbConnection);
                SQLiteDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    string name = reader["Name"].ToString();
                    string server = reader["Server"].ToString();
                    int platform = Convert.ToInt32(reader["Platform"]);
                    int id = Convert.ToInt32(reader["Id"]);
                    int uid = Convert.ToInt32(reader["Uid"]);
                    string cookies = reader["Cookies"].ToString();
                    GenshinAccountData data = new GenshinAccountData(name, server, platform, id, uid, cookies);
                    datas.Add(data);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                m_dbConnection.Close();
            }
            return datas;
        }
        public static List<GenshinAccountData> GetGenshinDataFromSql()
        {
            return GetGenshinDataFromSql(GlobalVar.AccountSqlPath);
        }
        //从cookie获取账号
        public static GenshinAccountData GetGenshinAccountFromCookie(string cookies, int platform = 0) {
            GenshinAccountData errorData = new GenshinAccountData("", "", 0, -1, 0, "");
            var query = new Dictionary<string, string>
            {
                { "game_biz", platform == 0 ? "hk4e_cn" : "hk4e_global" }
            };
            var mysHeaders = new Dictionary<string, string>
            {
                { "x-rpc-app_version", "2.11.2" },
                { "User-Agent", "Mozilla/5.0 (iPhone; CPU iPhone OS 13_2_3 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) miHoYoBBS/2.11.1" },
                { "x-rpc-client_type", "5" },
                { "Referer", "https://webstatic.mihoyo.com/" },
                { "Origin", "https://webstatic.mihoyo.com" },
                { "Cookie", cookies }
            };
            var HoyoHeaders = new Dictionary<string, string>
            {
                { "Cookie", cookies }
            };
            string hostUrl = platform == 0 ? genshinMysApiUrl : genshinHoyoApiUrl;
            string url = $"{hostUrl}/binding/api/getUserGameRolesByCookie";
            string data = Api.HttpGet(url, query, platform == 0 ? mysHeaders : HoyoHeaders);
            if (!Api.IsJson(data))
            { 
                return errorData;
            }
            JavaScriptSerializer stokenJss = new JavaScriptSerializer();
            dynamic JsonObj = stokenJss.Deserialize<dynamic>(data);
            if (JsonObj["retcode"] != 0)
            {
                return errorData;
            }
            //获取id
            int id = 0;
            // Split the string by ';'
            var parts = cookies.Split(';');
            // Loop through the parts
            foreach (var part in parts)
            {
                // Split the part by '='
                var keyValue = part.Split('=');

                // Check if the key is "account_id"
                if (keyValue[0] == "account_id")
                {
                    // Parse the value as an integer
                    id = int.Parse(keyValue[1]);
                    break;
                }
            }
            string name = JsonObj["data"]["list"][0]["nickname"];
            string server = JsonObj["data"]["list"][0]["region"];
            int uid = int.Parse(JsonObj["data"]["list"][0]["game_uid"]);
            GenshinAccountData accountData = new GenshinAccountData(name, server, platform, id, uid, cookies);
            return accountData;

        }
        //插入账号到数据库中
        public static void InsertGenshinAccountData(GenshinAccountData data)
        {   
            //先查找有没有旧数据，有就删除
            using (SQLiteConnection conn = new SQLiteConnection($"Data Source={GlobalVar.AccountSqlPath};Version=3;"))
            {
                conn.Open();
                string sql = $"DELETE FROM {GlobalVar.GenshinAccountSqlTable} WHERE uid=@uid";
                using (SQLiteCommand cmd = new SQLiteCommand(sql, conn))
                {
                    cmd.Parameters.AddWithValue("@uid", data.Uid);
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
            SQLiteConnection m_dbConnection = new SQLiteConnection($"Data Source={GlobalVar.AccountSqlPath};Version=3;"); ;
            try
            {
                m_dbConnection.Open();
                string sql = $"INSERT INTO {GlobalVar.GenshinAccountSqlTable} (Uid, Name, Server, Platform, Id, Cookies) VALUES (@Uid, @Name, @Server, @Platform, @Id, @Cookies)";
                SQLiteCommand cmd = new SQLiteCommand(sql, m_dbConnection);
                cmd.Parameters.Add(new SQLiteParameter("@Uid", data.Uid));
                cmd.Parameters.Add(new SQLiteParameter("@Name", data.Name));
                cmd.Parameters.Add(new SQLiteParameter("@Server", data.Server));
                cmd.Parameters.Add(new SQLiteParameter("@Platform", data.Platform));
                cmd.Parameters.Add(new SQLiteParameter("@Id", data.Id));
                cmd.Parameters.Add(new SQLiteParameter("@Cookies", data.Cookies));
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
        //下载图片到相对路径(开头不带/,结尾带/ 例：res/genshin/)
        public static string DownloadImage(string imageUrl, string folderPath) {
            string fileName = Path.GetFileName(imageUrl);
            string path = System.AppDomain.CurrentDomain.BaseDirectory + folderPath;
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            string imagePath = System.AppDomain.CurrentDomain.BaseDirectory + folderPath + fileName;
            if (File.Exists(imagePath))
            {
                return $"/{folderPath + fileName}";
            }
            using (WebClient client = new WebClient())
            {
                client.DownloadFile(imageUrl, imagePath);
            }
            return $"/{folderPath + fileName}";
        }
    }
    public class Settings {
        //用于读取设置的值
        public static string ReadGenshinCNPath() {
            JObject obj = Api.FileToJson(GlobalVar.SettingsFile);
            if (obj.ContainsKey("genshin") && obj["genshin"] is JObject genshin)
            {
                if (genshin.ContainsKey("CN_path"))
                {
                    return (string)genshin["CN_path"];
                }
            }
            return "";
        }
        public static string ReadGenshinOSPath()
        {
            JObject obj = Api.FileToJson(GlobalVar.SettingsFile);
            if (obj.ContainsKey("genshin") && obj["genshin"] is JObject genshin)
            {
                if (genshin.ContainsKey("OS_path"))
                {
                    return (string)genshin["OS_path"];
                }
            }
            return "";
        }
    }
}
