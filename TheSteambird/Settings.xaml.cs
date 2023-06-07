// Copyright (c) Microsoft Corporation and Contributors.
// Licensed under the MIT License.

using System;
using System.IO;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using TheSteambird.api;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Windows.Storage.Pickers;
using Windows.Storage;
using WinRT.Interop;
using WinRT;
using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml.Navigation;
using System.Net;
using System.Threading.Tasks;
using Ionic.Zip;
using Windows.UI.Core;
using Microsoft.UI.Dispatching;
using System.Collections.Generic;


// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TheSteambird
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Settings : Page
    {
        JObject obj = new();
        DispatcherQueue dispatcherQueue;
        public Settings()
        {
            this.InitializeComponent();
            dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            Init();
        }
        private void Init()
        {
            if (!File.Exists(GlobalVar.SettingsFile))
            {
                File.Create(GlobalVar.SettingsFile);
                return;
            }
            string json = File.ReadAllText(GlobalVar.SettingsFile);
            if (!Api.IsJson(json))
            {
                return;
            }
            obj = JObject.Parse(json);
            if (obj.ContainsKey("genshin") && obj["genshin"] is JObject genshin)
            {
                if (genshin.ContainsKey("CN_path"))
                {
                    genshinCNPathTextBox.Text = (string)genshin["CN_path"];
                }
                if (genshin.ContainsKey("OS_path"))
                {
                    genshinOSPathTextBox.Text = (string)genshin["OS_path"];
                }
            }

            string enkaPath = System.AppDomain.CurrentDomain.BaseDirectory + "Genshin\\EnkaNetwork";
            DirectoryInfo enkaDir = new DirectoryInfo(enkaPath);
            if (enkaDir.GetFiles().Length > 1)
            {
                EnkaNetworkText.Text = "Enka.Network数据库\t已加载";
            }
            else {
                EnkaNetworkText.Text = "未找到Enka.Network数据库\t点击右方按钮下载";
            }
        }
        private void UpdateGenshinPath(int platform, string path)
        {
            string key;
            if (platform == 0)
            {
                key = "CN_path";
            }
            else
            {
                key = "OS_path";
            }
            if (obj.ContainsKey("genshin") && obj["genshin"] is JObject genshin)
            {
                obj["genshin"][key] = path;
            }
            else
            {
                obj["genshin"] = new JObject();
                obj["genshin"][key] = path;
            }
            UpdateJson();
        }
        private void UpdateJson()
        {
            string json = JsonConvert.SerializeObject(obj, Formatting.Indented);
            File.WriteAllText(GlobalVar.SettingsFile, json);
        }
        private void genshinCNPathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = genshinCNPathTextBox.Text;
            if (text != "")
            {
                UpdateGenshinPath(0, text);
            }
        }

        private async void genshinCNPath_Click(object sender, RoutedEventArgs e)
        {

            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".exe");
            WinRT.Interop.InitializeWithWindow.Initialize(picker, GlobalVar.Hwnd);

            StorageFile file = await picker.PickSingleFileAsync();


            if (file != null)
            {
                genshinCNPathTextBox.Text = file.Path;
            }

        }

        private void genshinOSPathTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            string text = genshinOSPathTextBox.Text;
            if (text != "")
            {
                UpdateGenshinPath(1, text);
            }
        }

        private async void genshinOSPath_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FileOpenPicker();
            picker.FileTypeFilter.Add(".exe");
            WinRT.Interop.InitializeWithWindow.Initialize(picker, GlobalVar.Hwnd);

            StorageFile file = await picker.PickSingleFileAsync();


            if (file != null)
            {
                genshinOSPathTextBox.Text = file.Path;
            }
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is string && !string.IsNullOrWhiteSpace((string)e.Parameter))
            {
                string text = (string)e.Parameter;
                infoBar.Message = text;
                infoBar.IsOpen = true;
            }
            base.OnNavigatedTo(e);
        }

        private async void EnkaNetworkUpdate_Click(object sender, RoutedEventArgs e)
        {
            EnkaNetworkProgressRing.Value = 2;
            //下载文件
            string url = "https://gitlab.com/Dimbreath/AnimeGameData/-/archive/master/AnimeGameData-master.zip";
            string extractPath = GlobalVar.GenshinEnkaNetworkDataPath;
            string filePath = System.AppDomain.CurrentDomain.BaseDirectory + "Genshin\\EnkaNetwork\\EnkaNetworkData.zip";
            Api.EnsureDirectoryExists(extractPath);
            WebClient client = new WebClient();
            DateTime startTime = DateTime.Now;
            Uri uri = new Uri(url);
            client.DownloadProgressChanged += (sender, e) =>
            {
                EnkaNetworkProgressRing.Value = e.ProgressPercentage / 2 + 1;
                double secondsPassed = (DateTime.Now - startTime).TotalSeconds;
                double speed = e.BytesReceived / secondsPassed;
                EnkaNetworkText.Text = $"Enka.Network数据库\t正在下载\t速度：{speed} byts/second\t已下载 {e.BytesReceived} bytes";

            };
            await client.DownloadFileTaskAsync(uri, filePath);
            List<string> githubFiles = new List<string>() {
                "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/affixes.json",
                "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/characters.json",
                "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/costumes.json",
                "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/loc.json",
                "https://raw.githubusercontent.com/EnkaNetwork/API-docs/master/store/namecards.json"
            };
            foreach (string githubFile in githubFiles)
            {
                await client.DownloadFileTaskAsync(githubFile, extractPath + "\\" + Path.GetFileName(githubFile));
            }
            //解压文件
            string folderName = @"AnimeGameData-master/ExcelBinOutput";
            await Task.Run(() =>
            {
                using (ZipFile zip = ZipFile.Read(filePath))
                {
                    zip.FlattenFoldersOnExtract = true;
                    foreach (ZipEntry entry in zip)
                    {
                        if (entry.FileName.StartsWith(folderName + "/"))
                        {
                            entry.Extract(extractPath, ExtractExistingFileAction.OverwriteSilently);
                            dispatcherQueue.TryEnqueue(() => EnkaNetworkProgressRing.Value += 0.05);
                        }
                    }
                }
            });
            //删除下载的压缩包
            File.Delete(filePath);
            EnkaNetworkProgressRing.Value = 100;
            for (int i = 0; i < 100; i++)
            {
                EnkaNetworkProgressRing.Value--;
                await Task.Delay(10);
            }
            EnkaNetworkText.Text = "Enka.Network数据库\t已加载";
        }
    }
}