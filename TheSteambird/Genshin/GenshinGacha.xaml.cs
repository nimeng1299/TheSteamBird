// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Navigation;
using System.Collections.ObjectModel;
using TheSteambird.api;
using System.Data;
using TheSteambird.webUI;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Windows.UI.WebUI;
using System.Security.Cryptography;
using System.Data.SQLite;
using System.Data.Common;
using Microsoft.UI.Xaml.Shapes;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

/*
    新手祈愿 - 100
    常驻祈愿 - 200
    角色活动祈愿 - 301
    武器活动祈愿 - 302
    角色活动祈愿2 - 400
 */

namespace TheSteambird.Genshin
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class GenshinGacha : Page
    {
        ObservableCollection<GenshinAccount> genshinAccounts = new ObservableCollection<GenshinAccount>();
        bool isWebViewLoaded = false;
        public GenshinGacha()
        {
            this.InitializeComponent();
            Api.CreateFile(GlobalVar.GenshinGachaSqlPath);
            UpdateCombo();
            gachaView.Source = new Uri($"file:///{GlobalVar.GenshinGachaHtml}");
            gachaView.NavigationCompleted += (sender, args) =>
            {
                isWebViewLoaded = true;
            };
            if (genshinAccounts.Count > 0)
            {
                accountCombo.SelectedIndex = 0;
            }

        }

        private void accountCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateChart();
        }

        private async void MenuFlyoutItem_Click_FromCN(object sender, RoutedEventArgs e)
        {
            string path = TheSteambird.api.Settings.ReadGenshinCNPath();
            if (path == "")
            {
                Frame.Navigate(typeof(Settings), "请设置国服文件路径");
                return;
            }
            progressRing.IsActive = true;
            var task = Task.Run(() => GenshinGachaApi.FromCNGetGacha(path));
            List<List<GenshinGachaData>> datas = await task;
            GenshinGachaApi.AddGachaToSql(datas, datas);
            UpdateCombo();
            progressRing.IsActive = false;
        }

        private async void MenuFlyoutItem_Click_FromOS(object sender, RoutedEventArgs e)
        {
            string path = TheSteambird.api.Settings.ReadGenshinOSPath();
            if (path == "")
            {
                Frame.Navigate(typeof(Settings), "请设置国际服文件路径");
                return;
            }
            progressRing.IsActive = true;
            var task = Task.Run(() => GenshinGachaApi.FromOSGetGacha(path));
            List<List<GenshinGachaData>> datas = await task;
            GenshinGachaApi.AddGachaToSql(datas, datas);
            UpdateCombo();
            progressRing.IsActive = false;
        }

        private async void MenuFlyoutItem_Click_FromURL(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog();
            var textBox = new TextBox();
            dialog.Content = textBox;
            dialog.XamlRoot = this.XamlRoot;
            dialog.Title = "请输入网址，以https://开头，game_biz=...结束";
            dialog.PrimaryButtonText = "确定";
            dialog.SecondaryButtonText = "取消";
            var result = await dialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                string url = textBox.Text;
                progressRing.IsActive = true;
                var task = Task.Run(() => GenshinGachaApi.FromUrlGetGacha(url));
                List<List<GenshinGachaData>> datas = await task;
                GenshinGachaApi.AddGachaToSql(datas, datas);
                UpdateCombo();
                progressRing.IsActive = false;

            }
            else
            {
                return;
            }
        }
        private void UpdateCombo()
        {
            genshinAccounts.Clear();
            List<string> uids = Api.GetAllSqlTableNames(GlobalVar.GenshinGachaSqlPath);
            foreach (var uid in uids)
            {
                genshinAccounts.Add(new GenshinAccount(uid, uid));
            }
        }
        private async void UpdateChart()
        {
            var account = (GenshinAccount)accountCombo.SelectedItem;
            if (account == null)
                return;
            string uid = account.Uid;
            if (!isWebViewLoaded)
            {
                // 等待页面加载完成
                var tcs = new TaskCompletionSource<bool>();
                gachaView.NavigationCompleted += (sender, args) =>
                 {
                     tcs.SetResult(true);
                 };
                await tcs.Task;
            }

            await gachaView.ExecuteScriptAsync($"reload();");
            await gachaView.ExecuteScriptAsync($"setTitle({uid});");
            List<GenshinGachaData> datas = GetGachaAllData(uid);
            var sortedDataList = datas.OrderBy(data => long.Parse(data.Id));
            Dictionary<int, int> count = new Dictionary<int, int>();
            count.Add(200, 0);
            count.Add(301, 0);
            count.Add(302, 0);
            foreach (var data in sortedDataList)
            {
                if (data.Rank_type == 5)
                {
                    int i = 0;
                    if (data.Gacha_type == 200)
                    {
                        i = 2;
                    }
                    else if (data.Gacha_type == 302)
                    {
                        i = 1;
                    }
                    else if (data.Gacha_type == 301)
                    {
                        i = 0;
                    }
                    await gachaView.ExecuteScriptAsync($"addData({i},{data.Date},{count[data.Gacha_type]},'{data.Name}')");
                    count[data.Gacha_type] = 0;
                }
                else {
                    count[data.Gacha_type]++;
                }
            }

        }
        private List<GenshinGachaData> GetGachaAllData(string uid)
        {
            List<GenshinGachaData> datas = new();
            string connectionString = "data source = " + GlobalVar.GenshinGachaSqlPath;
            using (SQLiteConnection conn = new("Data Source=:memory:")) {
                conn.Open();
                // 从磁盘上的数据库文件中读取数据
                SQLiteConnection diskConnection = new(connectionString);
                diskConnection.Open();
                diskConnection.BackupDatabase(conn, "main", "main", -1, null, 0);
                diskConnection.Close();
                string sql = $"SELECT * FROM '{uid}'";
                SQLiteCommand command = new SQLiteCommand(sql, conn);
                SQLiteDataReader reader = command.ExecuteReader();
                Dictionary<int, string> lastTime = new();
                lastTime.Add(100, "0");
                lastTime.Add(200, "0");
                lastTime.Add(301, "0");
                lastTime.Add(302, "0");
                while (reader.Read())
                {
                    string id =  reader.GetString(0);
                    int gacha_type = reader.GetInt32(1);
                    if (gacha_type == 400)
                    {
                        gacha_type = 301;
                    }
                    string date = reader.GetString(2);
                    string name = reader.GetString(3);
                    string item_type = reader.GetString(4);
                    int rank_type = reader.GetInt32(5);
                    string time = Api.DateTimeToMsTimestamp(date);

                    if (rank_type == 5 && time == lastTime[gacha_type])
                    {
                        long timeStamp = long.Parse(time);
                        timeStamp += 60000; // 加上一分钟的毫秒数
                        time = timeStamp.ToString();
                    }
                    if(rank_type == 5)
                        lastTime[gacha_type] = time;
                    GenshinGachaData data= new GenshinGachaData(id, gacha_type, rank_type, name, time, item_type);
                    datas.Add(data);
                }
            }
            return datas;
        }
    }
}
