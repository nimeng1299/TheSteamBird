using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.AppBroadcasting;
using System.Data.SQLite;
using System.Data.SqlClient;
using Windows.Networking;
using Microsoft.UI.Xaml.Shapes;
using System.Net;
using System.Web.Script.Serialization;
using System.Net.Http;
using System.Collections.Specialized;
using Newtonsoft.Json.Linq;
using System.Globalization;

namespace TheSteambird.api
{
    class Api
    {
        //检查文件是否存在,如果文件不存在，它将被创建
        public static void CreateFile(string path)
        {
            if (!File.Exists(path))
            {
                File.Create(path);
            }
        }
        public static string ReadFile(string path)
        {
            if (!File.Exists(path))
            {
                return "";
            }
            else { 
                return File.ReadAllText(path);
            }

        }
        //检查文件夹是否存在,如果文件夹不存在，它将被创建
        public static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }
        //判断是否为Json字符串
        public static bool IsJson(string input)
        {
            input = input.Trim();
            if ((input.StartsWith("{") && input.EndsWith("}")) || //For object
                (input.StartsWith("[") && input.EndsWith("]"))) //For array
            {
                try
                {
                    var jss = new JavaScriptSerializer();
                    jss.Deserialize<dynamic>(input);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        //MD5加密
        public static string MD5Encrypt(string input)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                byte[] inputBytes = System.Text.Encoding.ASCII.GetBytes(input);
                byte[] hashBytes = md5.ComputeHash(inputBytes);
                string MD5 = null;
                for (int i = 0; i < hashBytes.Length; i++)

                {

                    MD5 += hashBytes[i].ToString("x2");  //hashBytes

                }
                return MD5;
            }
        }
        //时间戳倒计时转换 
        public static string ConvertTimestampToString(int timestamp)
        {
            TimeSpan time = TimeSpan.FromSeconds(timestamp);
            string result = string.Format("{0:D2}天{1:D2}时{2:D2}分",
                time.Days,
                time.Hours,
                time.Minutes);

            if (time.Days == 0)
            {
                result = result.Substring(result.IndexOf("天") + 1);
            }
            if (time.Days == 0 && time.Hours == 0)
            {
                result = result.Substring(result.IndexOf("时") + 1);
            }
            return result;
        }
        //时间戳转时间
        public static string TimestampToDateTime(long timestamp, string DateType = "yyyy.MM.dd HH:mm:ss")
        {
            DateTimeOffset dateTimeOffset = DateTimeOffset.FromUnixTimeSeconds(timestamp);
            DateTime dateTime = dateTimeOffset.DateTime;
            // Format the date as desired
            string formattedDate = dateTime.ToString("yyyy-MM-dd HH:mm:ss");
            return formattedDate;
        }
        public static string TimestampToDateTime(string timestamp, string DateType = "yyyy.MM.dd HH:mm:ss")
        {
            long time = long.Parse(timestamp);
            return TimestampToDateTime(time, DateType);

        }
        //日期转毫秒时间戳
        public static string DateTimeToMsTimestamp(string date, string format = "yyyy-MM-dd HH:mm:ss") {
            DateTime dateTime = DateTime.ParseExact(date, format, CultureInfo.InvariantCulture);
            DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            long timeStamp = (long)(dateTime - startTime).TotalMilliseconds;
            string timeStampString = timeStamp.ToString();
            return timeStampString;
        }
        //把字符串补足长度
        public static string PadString(string input, int minLength, char placeholder = ' ')
        {
            if (input.Length < minLength)
            {
                input = input.PadRight(minLength, placeholder);
            }
            return input;
        }
        //读取json文件
        public static JObject FileToJson(string filePath)
        {
            if (Directory.Exists(filePath))
            {
                return new JObject();
            }
            string fileContent = File.ReadAllText(filePath);
            if (!IsJson(fileContent))
            { 
                return new JObject();
            }
            return JObject.Parse(fileContent);
        }
        public static JArray FileToJarray(string filePath)
        {
            if (Directory.Exists(filePath))
            {
                return new JArray();
            }
            string fileContent = File.ReadAllText(filePath);
            if (!IsJson(fileContent))
            {
                return new JArray();
            }
            return JArray.Parse(fileContent);
        }
        //SQLite数据库操作类
        //检测文件是否为Sql数据库文件
        public static bool IsSqlFile(string filePath)
        {
            if (!File.Exists(filePath))
            {
                return false;
            }
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            {
                byte[] buffer = new byte[16];
                stream.Read(buffer, 0, buffer.Length);
                string header = Encoding.ASCII.GetString(buffer);
                return header.Contains("SQLite format 3");
            }
        }
        //创建一个数据库文件
        public static void CreateSql(string filePath)
        {
            SQLiteConnection.CreateFile(filePath);
        }
        //检测数据库是否存在表
        public static bool IsExistsSqlTable(string filePath, string tableName)
        {
            if (!IsSqlFile(filePath))
            {
                return false;
            }
            string connectionString = $"Data Source={filePath};Version=3;";
            SQLiteConnection source = new SQLiteConnection(connectionString);
            source.Open();
            using (SQLiteConnection connection = new SQLiteConnection("Data Source=:memory:"))
            {
                connection.Open();
                source.BackupDatabase(connection, "main", "main", -1, null, 0);
                source.Close();
                using (SQLiteCommand command = new SQLiteCommand($"SELECT name FROM sqlite_master WHERE type='table' AND name='{tableName}'", connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        return reader.HasRows;
                    }
                }
            }
        }
        //数据库调用语句函数
        public static void ExecuteSql(string filePath, string sql)
        {
            string connectionString = $"Data Source={filePath};Version=3;";
            SQLiteConnection sqliteConn = new SQLiteConnection(connectionString);
            if (sqliteConn.State != System.Data.ConnectionState.Open)
            {
                sqliteConn.Open();
                SQLiteCommand cmd = new SQLiteCommand();
                cmd.Connection = sqliteConn;
                cmd.CommandText = sql;
                cmd.ExecuteNonQuery();
            }
            sqliteConn.Close();
        }
        //数据库中创建表,第三个参数为表列如 CustomerId INT PRIMARY KEY,FirstName NVARCHAR(50),
        public static void CreateSqlTable(string filePath, string tableName, string tablieType)
        {
            if (!IsSqlFile(filePath))
            {
                CreateSql(filePath);
            }
            string createTableSql = $"CREATE TABLE '{tableName}' ({tablieType})";
            ExecuteSql(filePath, createTableSql);
        }
        //读取数据库的所有表
        public static List<string> GetAllSqlTableNames(string databasePath)
        {
            string connectionString = @"Data Source=" + databasePath + ";Version=3;";
            List<string> tableNames = new List<string>();
            if (!IsSqlFile(databasePath))
            { 
                return tableNames;
            }

            using (SQLiteConnection conn = new SQLiteConnection("Data Source=:memory:"))
            {
                conn.Open();
                SQLiteConnection source = new SQLiteConnection(connectionString);
                source.Open();
                source.BackupDatabase(conn, "main", "main", -1, null, 0);
                source.Close();
                string sqlTableNames = "SELECT name FROM sqlite_master WHERE type='table' ORDER BY name;";
                SQLiteCommand cmd = new SQLiteCommand(sqlTableNames, conn);
                using (SQLiteDataReader dr = cmd.ExecuteReader())
                {
                    while (dr.Read())
                    {
                        tableNames.Add(dr["Name"].ToString());
                    }
                }
            }
            return tableNames;
        }

        //https网络操作类
        //https Get
        public static string HttpGet(string url, Dictionary<string, string> query, Dictionary<string, string> headers)
        {
            using (var client = new HttpClient())
            {
                // Add headers to the client
                foreach (var header in headers)
                {
                    client.DefaultRequestHeaders.Add(header.Key, header.Value);
                }

                // Build the query string from the query dictionary
                var queryString = string.Join("&", query.Select(kvp => $"{kvp.Key}={kvp.Value}"));

                // Build the final URL with the query parameters
                var finalUrl = $"{url}?{queryString}";

                // Send the GET request and get the response
                var response = client.GetAsync(finalUrl).Result;

                // Return the content of the response
                return response.Content.ReadAsStringAsync().Result;
            }
        }
        //https Post
        public static string HttpPost(string url, Dictionary<string, string> query, Dictionary<string, string> headers)
        {
            using (var client = new WebClient())
            {
                // 添加header
                foreach (var header in headers)
                {
                    client.Headers.Add(header.Key, header.Value);
                }

                // 将查询字符串转换为NameValueCollection
                var data = new NameValueCollection();
                foreach (var kvp in query)
                {
                    data.Add(kvp.Key, kvp.Value);
                }

                // 发送POST请求
                var responseBytes = client.UploadValues(url, "POST", data);

                // 返回响应字符串
                return Encoding.UTF8.GetString(responseBytes);
            }
        }
    }
}
