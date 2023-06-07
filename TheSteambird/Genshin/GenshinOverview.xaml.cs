// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Web.Script.Serialization;
using TheSteambird.api;
using Windows.Foundation;
using Windows.Foundation.Collections;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TheSteambird.Genshin
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GenshinOverview : Page
    {
        ObservableCollection<GenshinOverviewData> genhsinDatas = new ObservableCollection<GenshinOverviewData>();
        public GenshinOverview()
        {
            this.InitializeComponent();
            Init();
        }
        //初始化
        private async void Init()
        {
            List<GenshinAccountData> datas = TheSteambirdApi.GetGenshinDataFromSql();
            Dictionary<int, List<string>> overviewDatas = new();
            try
            {
                overviewDatas = await GlobalVar.GenshinOverviewGet.GetAllOverview(datas);
            }
            catch
            {
                infoBar.Message = "网络错误";
                infoBar.IsOpen = true;
            }
            foreach (GenshinAccountData data in datas)
            {
                List<string> value = overviewDatas[data.Uid];
                try
                {
                    InitData(data, value);
                }
                catch (Exception ex)
                {
                    infoBar.Message = "uid " + data.Uid.ToString() + " 报错：" + ex.Message;
                    infoBar.IsOpen = true;
                }
            }
        }
        //处理初始化的数据
        private void InitData(GenshinAccountData account, List<string> datas) {
            string data = datas[0];     //成就方面的json
            string daily = datas[1];    //日常方面的json
            //传回数据检验
            if (!(Api.IsJson(data) && Api.IsJson(daily)))
            {
                infoBar.Message = $"uid:{account.Uid.ToString()} 出现网络错误";
                infoBar.IsOpen = true;
                return;
            }
            var dataSerializer = new JavaScriptSerializer();
            var dataobj = dataSerializer.Deserialize<dynamic>(data);
            //错误处理
            int retcode = dataobj["retcode"];
            if(retcode != 0)
            {
                infoBar.Message = $"uid:{account.Uid.ToString()} 出现错误{dataobj["message"]}";
                infoBar.IsOpen = true;
                return;
            }
            var dailySerializer = new JavaScriptSerializer();
            var dailyobj = dailySerializer.Deserialize<dynamic>(daily);
            retcode = dailyobj["retcode"];
            if (retcode != 0)
            {
                infoBar.Message = $"uid:{account.Uid.ToString()} 出现错误{dailyobj["message"]}";
                infoBar.IsOpen = true;
                return;
            }
            //数据处理
            //日常
            int current_resin = dailyobj["data"]["current_resin"];
            int max_resin = dailyobj["data"]["max_resin"];
            string resin_recovery_time = dailyobj["data"]["resin_recovery_time"];
            List<string> FragileResin = new() { current_resin.ToString(), max_resin.ToString(), Api.ConvertTimestampToString(int.Parse(resin_recovery_time)) };
            int current_home_coin = dailyobj["data"]["current_home_coin"];
            int max_home_coin = dailyobj["data"]["max_home_coin"];
            string home_coin_recovery_time = dailyobj["data"]["home_coin_recovery_time"];
            List<string> SereniteaPot = new() { current_home_coin.ToString(), max_home_coin.ToString(), Api.ConvertTimestampToString(int.Parse(home_coin_recovery_time)) };
            bool is_extra_task_reward_received = dailyobj["data"]["is_extra_task_reward_received"];
            List<string> ParametricTransformer = new() { is_extra_task_reward_received ? "1" : "0", "1", is_extra_task_reward_received ? "参量质变仪已可使用" : "参量质变仪尚未完成" };
            int finished_task_num = dailyobj["data"]["finished_task_num"];
            int total_task_num = dailyobj["data"]["total_task_num"];
            List<string> Commission = new() { finished_task_num.ToString(), total_task_num.ToString(), finished_task_num == total_task_num ? "每日任务全部完成" : "每日任务尚未完成" };
            int current_expedition_num = dailyobj["data"]["current_expedition_num"];
            int max_expedition_num = dailyobj["data"]["max_expedition_num"];
            List<string> Domains = new() { (max_expedition_num - current_expedition_num).ToString(), max_expedition_num.ToString(), current_expedition_num == 0 ? "探索派遣全部完成" : "探索派遣尚未完成" };
            List<List<string>> dailys = new() { FragileResin, SereniteaPot, ParametricTransformer, Commission, Domains };
            //生涯数据
            int active_day_number = dataobj["data"]["stats"]["active_day_number"];
            int achievement_number = dataobj["data"]["stats"]["achievement_number"];
            int avatar_number = dataobj["data"]["stats"]["avatar_number"];
            int way_point_number = dataobj["data"]["stats"]["way_point_number"];
            int anemoculus_number = dataobj["data"]["stats"]["anemoculus_number"];
            int geoculus_number = dataobj["data"]["stats"]["geoculus_number"];
            int electroculus_number = dataobj["data"]["stats"]["electroculus_number"];
            int dendroculus_number = dataobj["data"]["stats"]["dendroculus_number"];
            int domain_number = dataobj["data"]["stats"]["domain_number"];
            string spiral_abyss = dataobj["data"]["stats"]["spiral_abyss"];
            int luxurious_chest_number = dataobj["data"]["stats"]["luxurious_chest_number"];
            int precious_chest_number = dataobj["data"]["stats"]["precious_chest_number"];
            int exquisite_chest_number = dataobj["data"]["stats"]["exquisite_chest_number"];
            int common_chest_number = dataobj["data"]["stats"]["common_chest_number"];
            int magic_chest_number = dataobj["data"]["stats"]["magic_chest_number"];
            List<string> detail = new() {
                active_day_number.ToString(), achievement_number.ToString(), avatar_number.ToString(), way_point_number.ToString(), anemoculus_number.ToString(),
                geoculus_number.ToString(), electroculus_number.ToString(), dendroculus_number.ToString(), domain_number.ToString(), spiral_abyss,
                luxurious_chest_number.ToString(), precious_chest_number.ToString(), exquisite_chest_number.ToString(), common_chest_number.ToString(), magic_chest_number.ToString()
            };
            //探索度
            List<string> mengde = new();
            List<string> dragonspine = new();
            List<string> liyue = new();
            List<string> cengyan = new();
            List<string> cengyan_under = new();
            List<string> daoqi = new();
            List<string> enkanomiya = new();
            List<string> xumi = new();
            //先遍历
            int count = dataobj["data"]["world_explorations"].Length;
            for (int i = 0; i < count; i++)
            {
                int id = dataobj["data"]["world_explorations"][i]["id"];
                int exploration_percentage, level;
                switch (id)
                {
                    
                    case 1:
                        //蒙德
                        exploration_percentage = dataobj["data"]["world_explorations"][i]["exploration_percentage"];
                        level = dataobj["data"]["world_explorations"][i]["level"];
                        mengde = new() { (exploration_percentage / 10.0).ToString() , level.ToString()};
                        break; 
                    case 2:
                        // 璃月
                        exploration_percentage = dataobj["data"]["world_explorations"][i]["exploration_percentage"];
                        level = dataobj["data"]["world_explorations"][i]["level"];
                        liyue = new() { (exploration_percentage / 10.0).ToString(), level.ToString() };
                        break;
                    case 3:
                        //雪山
                        exploration_percentage = dataobj["data"]["world_explorations"][i]["exploration_percentage"];
                        int dragonspine_Eldertree = dataobj["data"]["world_explorations"][i]["offerings"][0]["level"];
                        dragonspine = new() { (exploration_percentage / 10.0).ToString(), dragonspine_Eldertree.ToString() };
                        break;
                    case 4:
                        //稻妻
                        exploration_percentage = dataobj["data"]["world_explorations"][i]["exploration_percentage"];
                        level = dataobj["data"]["world_explorations"][i]["level"];
                        int daoqi_Oraionokami = dataobj["data"]["world_explorations"][i]["offerings"][0]["level"];
                        daoqi = new() { (exploration_percentage / 10.0).ToString(), level.ToString(), daoqi_Oraionokami.ToString() };
                        break;
                    case 5:
                        //渊下宫
                        exploration_percentage = dataobj["data"]["world_explorations"][i]["exploration_percentage"];
                        enkanomiya = new() { (exploration_percentage / 10.0).ToString()};
                        break;
                    case 6:
                        //层岩巨渊
                        exploration_percentage = dataobj["data"]["world_explorations"][i]["exploration_percentage"];
                        int cengyan_Lumenstone = dataobj["data"]["world_explorations"][i]["offerings"][0]["level"];
                        cengyan = new() { (exploration_percentage / 10.0).ToString(), cengyan_Lumenstone.ToString() };
                        break;
                    case 7:
                        //层岩巨渊·地下矿区
                        exploration_percentage = dataobj["data"]["world_explorations"][i]["exploration_percentage"];
                        int cengyan_under_Lumenstone = dataobj["data"]["world_explorations"][i]["offerings"][0]["level"];
                        cengyan_under = new() { (exploration_percentage / 10.0).ToString(), cengyan_under_Lumenstone.ToString() };
                        break;
                    case 8:
                        //须弥
                        exploration_percentage = dataobj["data"]["world_explorations"][i]["exploration_percentage"];
                        level = dataobj["data"]["world_explorations"][i]["level"];
                        int xumi_DreamTree = dataobj["data"]["world_explorations"][i]["offerings"][0]["level"];
                        xumi = new() { (exploration_percentage / 10.0).ToString(), level.ToString(), xumi_DreamTree.ToString() };
                        break;
                    default: 
                        break;
                }
            }
            //注意添加的地区顺序
            List<List<string>> exploration = new() { 
                mengde, dragonspine, liyue, cengyan, cengyan_under, daoqi, enkanomiya, xumi
            };
            genhsinDatas.Add(new GenshinOverviewData(account.Name, account.Uid.ToString(), dailys, detail, exploration));
        }
    }
    public class PadStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string str && parameter is string padLengthStr && int.TryParse(padLengthStr, out int padLength))
            {
                return str.PadRight(padLength);
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
