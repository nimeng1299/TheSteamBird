// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using Microsoft.UI.Xaml.Navigation;
using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
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
    public sealed partial class GenshinAbyss : Page
    {
        List<GenshinAccountData> datas;
        ObservableCollection<GenshinAccount> genshinAccounts = new ObservableCollection<GenshinAccount>();
        ObservableCollection<GenshinAbyssDate> dates = new ObservableCollection<GenshinAbyssDate>();
        ObservableCollection<GenshinAbyssRevealRanks> RevealRanks = new ObservableCollection<GenshinAbyssRevealRanks>();
        ObservableCollection<GenshinAbyssFloors> Floors = new ObservableCollection<GenshinAbyssFloors>();
        public AbyssBase abyssBase { get; set; }
        public GenshinAbyss()
        {
            this.InitializeComponent();
            abyssBase = new AbyssBase();
            Api.CreateFile(GlobalVar.GenshinAbyssSqlPath);
            datas = TheSteambirdApi.GetGenshinDataFromSql();
            foreach (GenshinAccountData data in datas)
            {
                genshinAccounts.Add(new GenshinAccount(data.Name, data.Uid.ToString()));
            }
            UpdateDatas();
        }

        private void accountCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var account = (GenshinAccount)accountCombo.SelectedItem;
            accountName.Text = account.Name;
            if (!Api.IsExistsSqlTable(GlobalVar.GenshinAbyssSqlPath, account.Uid))
            {
                infoBar.Message = $"未找到 {account.Name} 的相关数据，请获取数据";
                infoBar.IsOpen = true;
            }
            int dateListCount = dateList.Items.Count;
            if (dateListCount > 0)
            {
                dateList.SelectedIndex = 0;
            }
            UpdateDatas();
        }

        private async void updateData_Click(object sender, RoutedEventArgs e)
        {
            var account = (GenshinAccount)accountCombo.SelectedItem;
            GenshinAccountData data = new();
            foreach(var accountData in datas)
            {
                if (accountData.Uid == int.Parse(account.Uid))
                { 
                    data = accountData;
                    break;
                }
            }
            List<string> abyssData = await GenshinAbyssApi.GetAbyss(data);
            GenshinAbyssApi.UpdateAbyssSql(abyssData, data.Uid);
            UpdateDatas();
        }
        private void UpdateDatas() {
            var account = (GenshinAccount)accountCombo.SelectedItem;
            if (account == null)
                return;
            string uid = account.Uid;
            //读取所有列
            dates.Clear();
            List<int> dateData = new();
            string connectionString = $"Data Source={GlobalVar.GenshinAbyssSqlPath};Version=3;";
            using (SQLiteConnection connection = new SQLiteConnection(connectionString))
            {
                connection.Open();
                string query = $"SELECT Id FROM '{uid}'";
                using (SQLiteCommand command = new SQLiteCommand(query, connection))
                {
                    using (SQLiteDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            int id = reader.GetInt32(0);
                            dateData.Add(id);
                        }
                    }
                }
            }
            dateData.Sort((a, b) => b.CompareTo(a));
            for (int i = 0; i < dateData.Count; i++)
            {
                dates.Add(new GenshinAbyssDate(dateData[i].ToString()));
            }
        }

        private void dateList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string count = "0";
            if (e.AddedItems != null && e.AddedItems.Count >= 1)
            {
                var selectedItem = e.AddedItems[0] as GenshinAbyssDate;
                count = selectedItem.Count;
                // do something with count
            }
            ReadToAbyssBase(count);
            UpdateUi();
        }
        private void ReadToAbyssBase(string count)
        {
            var account = (GenshinAccount)accountCombo.SelectedItem;
            if (account == null)
                return;
            string uid = account.Uid;
            string data = GenshinAbyssApi.ReadAbyssSqlData(uid, int.Parse(count));
            JObject obj = JObject.Parse(data);

            abyssBase.Star = ((int)obj["total_star"]).ToString();
            string max_floor = (string)obj["max_floor"];
            if (max_floor == "0-0")
                return;
            abyssBase.Max_floor = max_floor;
            abyssBase.Total_battle_times = ((int)obj["total_battle_times"]).ToString();

            abyssBase.Defeat_rank = ((int)obj["defeat_rank"][0]["value"]).ToString();
            abyssBase.Defeat_rank_pic = TheSteambirdApi.DownloadImage((string)obj["defeat_rank"][0]["avatar_icon"], "res/genshin/abyss/");
            abyssBase.Damage_rank = ((int)obj["damage_rank"][0]["value"]).ToString();
            abyssBase.Damage_rank_pic = TheSteambirdApi.DownloadImage((string)obj["damage_rank"][0]["avatar_icon"], "res/genshin/abyss/");
            abyssBase.Take_damage_rank = ((int)obj["take_damage_rank"][0]["value"]).ToString();
            abyssBase.Take_damage_rank_pic = TheSteambirdApi.DownloadImage((string)obj["take_damage_rank"][0]["avatar_icon"], "res/genshin/abyss/");
            abyssBase.Normal_skill_rank = ((int)obj["normal_skill_rank"][0]["value"]).ToString();
            abyssBase.Normal_skill_rank_pic = TheSteambirdApi.DownloadImage((string)obj["normal_skill_rank"][0]["avatar_icon"], "res/genshin/abyss/");
            abyssBase.Energy_skill_rank = ((int)obj["energy_skill_rank"][0]["value"]).ToString();
            abyssBase.Energy_skill_rank_pic = TheSteambirdApi.DownloadImage((string)obj["energy_skill_rank"][0]["avatar_icon"], "res/genshin/abyss/");

            RevealRanks.Clear();
            JArray revealRankArray = (JArray)obj["reveal_rank"];
            foreach (JObject revealRankObj in revealRankArray)
            {
                RevealRanks.Add(new GenshinAbyssRevealRanks(TheSteambirdApi.DownloadImage((string)revealRankObj["avatar_icon"], "res/genshin/abyss/"), ((int)revealRankObj["value"]).ToString()));
            }

            Floors.Clear();
            JArray floorsArray = (JArray)obj["floors"];
            foreach (JObject floorObj in floorsArray)
            {
                string index = ((int)floorObj["index"]).ToString();
                string star = ((int)floorObj["star"]).ToString();
                JArray levelsArray = (JArray)floorObj["levels"];
                ObservableCollection<GenshinAbyssLevels> Level = new();
                foreach (JObject levelsObj in levelsArray)
                { 
                    string levelIndex = ((int)levelsObj["index"]).ToString();
                    string levelStar = ((int)levelsObj["star"]).ToString();
                    JArray battlesArray = (JArray)levelsObj["battles"];
                    string timeDate = "";
                    ObservableCollection<GenshinAbyssBattles> Battle1 = new();
                    ObservableCollection<GenshinAbyssBattles> Battle2 = new();
                    foreach (JObject battlesObj in battlesArray)
                    {
                        ObservableCollection<GenshinAbyssBattles> Battle = new();
                        timeDate = Api.TimestampToDateTime((string)battlesObj["timestamp"]);
                        JArray avatarsArray = (JArray)battlesObj["avatars"];
                        
                        foreach (JObject avatarObj in avatarsArray)
                        {
                            string pic = TheSteambirdApi.DownloadImage((string)avatarObj["icon"], "res/genshin/abyss/");
                            string avatarLevel = ((int)avatarObj["level"]).ToString();
                            Battle.Add(new GenshinAbyssBattles(pic, avatarLevel));
                        }
                        int battlesIndex = (int)battlesObj["index"];
                        if (battlesIndex == 1)
                        {
                            Battle1 = Battle;
                        }
                        else
                        {
                            Battle2 = Battle;
                        }
                    }
                    Level.Add(new GenshinAbyssLevels(levelIndex, timeDate, levelStar, Battle1, Battle2));
                }
                Floors.Add(new GenshinAbyssFloors(index, star, Level));
            }
        }
        private void UpdateUi()
        {
            star.Text = abyssBase.Star;
            max_floor.Text = abyssBase.Max_floor;
            total_battle_times.Text = abyssBase.Total_battle_times;

            defeat_rank_pic.Source = new BitmapImage(new Uri($"ms-appx://{abyssBase.Defeat_rank_pic}"));
            defeat_rank.Text = abyssBase.Defeat_rank;
            damage_rank_pic.Source = new BitmapImage(new Uri($"ms-appx://{abyssBase.Damage_rank_pic}"));
            damage_rank.Text = abyssBase.Damage_rank;
            take_damage_rank_pic.Source = new BitmapImage(new Uri($"ms-appx://{abyssBase.Take_damage_rank_pic}"));
            take_damage_rank.Text = abyssBase.Take_damage_rank;
            normal_skill_rank_pic.Source = new BitmapImage(new Uri($"ms-appx://{abyssBase.Normal_skill_rank_pic}"));
            normal_skill_rank.Text = abyssBase.Normal_skill_rank;
            energy_skill_rank_pic.Source = new BitmapImage(new Uri($"ms-appx://{abyssBase.Energy_skill_rank_pic}"));
            energy_skill_rank.Text = abyssBase.Energy_skill_rank;
        }

    }
   
    public class AbyssBase
    {
        public string Star { get; set; }
        public string Max_floor { get; set;}
        public string Total_battle_times { get; set;}
        public string Defeat_rank { get; set; }
        public string Defeat_rank_pic { get; set; }
        public string Damage_rank { get; set; }
        public string Damage_rank_pic { get; set; }
        public string Take_damage_rank { get; set; }
        public string Take_damage_rank_pic { get; set; }
        public string Energy_skill_rank { get; set; }
        public string Energy_skill_rank_pic { get; set; }
        public string Normal_skill_rank { get; set; }
        public string Normal_skill_rank_pic { get; set; }
        public AbyssBase()
        {
            Star = "0";
            Max_floor = "0-0";
            Total_battle_times = "2000年1月1日";
            Defeat_rank = "Lumine";
            Defeat_rank_pic = "/res/ico256.png";
            Damage_rank = "Lumine";
            Damage_rank_pic = "/res/ico256.png";
            Take_damage_rank = "Lumine";
            Take_damage_rank_pic = "/res/ico256.png";
            Energy_skill_rank = "Lumine";
            Energy_skill_rank_pic = "/res/ico256.png";
            Normal_skill_rank = "Lumine";
            Normal_skill_rank_pic = "/res/ico256.png";
            Damage_rank = "Lumine";
        }

    }
}
