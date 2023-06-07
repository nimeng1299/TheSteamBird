using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;

namespace TheSteambird.api
{
    public class CookieApi
    {
        static readonly string GenshinMysUrl = "https://api-takumi.mihoyo.com";
        static readonly string GenshinHoyoUrl = "https://api-os-takumi.hoyoverse.com";
        public static string GetGenshinMysCookies(string cookies, int platform = 0) {
            string  id = "", login_ticket = "", stoken = "", cookie_token = "";
            string url = "";
            url = GenshinMysUrl;
            string[] cookiesArray = cookies.Split(';');
            foreach (var cookie in cookiesArray) {
                if (cookie.Contains("account_id") || cookie.Contains("login_uid") || cookie.Contains("stuid"))
                {
                    int index = cookie.IndexOf("=");
                    id = cookie.Substring(index + 1, cookie.Length - index - 1);
                }
                else if (cookie.Contains("login_ticket"))
                {
                    int index = cookie.IndexOf("=");
                    login_ticket = cookie.Substring(index + 1, cookie.Length - index - 1);
                    if (login_ticket[login_ticket.Length - 1] == '\"')
                    {
                        login_ticket = login_ticket.Substring(0, login_ticket.Length - 1);
                    }
                }
                else if (cookie.Contains("stoken"))
                {
                    int index = cookie.IndexOf("=");
                    stoken = cookie.Substring(index + 1, cookie.Length - index - 1);
                }
                else if (cookie.Contains("cookie_token"))
                {
                    int index = cookie.IndexOf("=");
                    cookie_token = cookie.Substring(index + 1, cookie.Length - index - 1);
                }
            }
            if (id == "") { return "-1"; }
            if (cookie_token == "")
            {
                if (stoken == "")
                {
                    string stokenJson = "";
                    if (login_ticket == "")
                    {
                        return "-1";
                    }
                    else
                    {
                        var stokenQuery = new Dictionary<string, string>
                        {
                            { "login_ticket", login_ticket },
                            { "token_types", "3" },
                            { "uid", id }
                        };
                        var stokenHeaders = new Dictionary<string, string>
                        {
                            { "x-rpc-app_version", "2.11.2" },
                            { "User-Agent", "Mozilla/5.0 (iPhone; CPU iPhone OS 13_2_3 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) miHoYoBBS/2.11.1" },
                            { "x-rpc-client_type", "5" },
                            { "Referer", "https://webstatic.mihoyo.com/" },
                            { "Origin", "https://webstatic.mihoyo.com" },
                            { "Cookie", cookies },
                        };
                        stokenJson = Api.HttpGet($"{url}/auth/api/getMultiTokenByLoginTicket", stokenQuery, stokenHeaders);
                    }
                    if (!Api.IsJson(stokenJson))
                    {
                        return "-1";
                    }
                    JavaScriptSerializer stokenJss = new JavaScriptSerializer();
                    dynamic stokenJsonObj = stokenJss.Deserialize<dynamic>(stokenJson);
                    if (stokenJsonObj["retcode"] != 0)
                    {
                        return "-1";
                    }
                    stoken = stokenJsonObj["data"]["list"][0]["token"];
                }

                var tokenQuery = new Dictionary<string, string>
                {
                    { "stoken", stoken },
                    { "uid", id }
                };
                var tokenHeaders = new Dictionary<string, string>
                {
                    { "x-rpc-app_version", "2.11.2" },
                    { "User-Agent", "Mozilla/5.0 (iPhone; CPU iPhone OS 13_2_3 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) miHoYoBBS/2.11.1" },
                    { "x-rpc-client_type", "5" },
                    { "Referer", "https://webstatic.mihoyo.com/" },
                    { "Origin", "https://webstatic.mihoyo.com" },
                    { "Cookie", $"{cookies}; cookie_token={stoken}" },
                };

                string tokenJson = Api.HttpGet($"{url}/auth/api/getCookieAccountInfoBySToken", tokenQuery, tokenHeaders);
                if (!Api.IsJson(tokenJson))
                {
                    return "-1";
                }

                JavaScriptSerializer tokenJss = new JavaScriptSerializer();
                dynamic tokenJsonObj = tokenJss.Deserialize<dynamic>(tokenJson);
                if (tokenJsonObj["retcode"] != 0)
                {
                    return "-1";
                }
                cookie_token = tokenJsonObj["data"]["cookie_token"];
            }
            if (cookie_token == "-1")
            {
                return "-1";
            }
            else {
                return $"account_id={id};cookie_token={cookie_token}";
            }
        }
        public static string GetGenshinHoyoCookies(string cookies, string id)
        {
            string url = GenshinHoyoUrl;
            string login_ticket = "";
            string[] cookiesArray = cookies.Split(';');
            foreach (string cookie in cookiesArray)
            {
                string[] keyValue = cookie.Split('=');
                if (keyValue[0].Trim() == "login_ticket")
                {
                    login_ticket = keyValue[1];
                    break;
                }
            }
            if (login_ticket[login_ticket.Length - 1] == '\"')
            {
                login_ticket = login_ticket.Substring(0, login_ticket.Length - 1);
            }
            if (login_ticket == "")
            {
                return "-1";
            }
            //stoken
            string stoken = "";
            var stokenQuery = new Dictionary<string, string>
            {
                { "login_ticket", login_ticket },
                { "token_types", "3" },
                { "uid", id }
            };
            var stokenHeaders = new Dictionary<string, string>
            {
                { "Cookie", cookies}
            };
            string stokenJson = Api.HttpGet($"{url}/auth/api/getMultiTokenByLoginTicket", stokenQuery, stokenHeaders);
            if (!Api.IsJson(stokenJson))
            {
                return "-1";
            }
            JavaScriptSerializer stokenJss = new JavaScriptSerializer();
            dynamic stokenJsonObj = stokenJss.Deserialize<dynamic>(stokenJson);
            if (stokenJsonObj["retcode"] != 0)
            {
                return "-1";
            }
            stoken = stokenJsonObj["data"]["list"][0]["token"];
            //cookie_token
            string cookie_token = "";
            var tokenQuery = new Dictionary<string, string>
            {
                { "uid", id },
                { "stoken", stoken}
            };
            var tokenHerders = new Dictionary<string, string>
            {
                { "Cookie", $"{cookies};stoken={stoken}" }
            };
            string tokenJson = Api.HttpPost($"{url}/auth/api/getCookieAccountInfoBySToken", tokenQuery, tokenHerders);
            if (!Api.IsJson(tokenJson))
            {
                return "-1";
            }

            JavaScriptSerializer tokenJss = new JavaScriptSerializer();
            dynamic tokenJsonObj = tokenJss.Deserialize<dynamic>(tokenJson);
            if (tokenJsonObj["retcode"] != 0)
            {
                return "-1";
            }
            cookie_token = tokenJsonObj["data"]["cookie_token"];
            if (cookie_token == "-1")
            {
                return "-1";
            }
            else
            {
                return $"account_id={id};cookie_token={cookie_token}";
            }
        }

    }
}
