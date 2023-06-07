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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using TheSteambird.api;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage.Pickers;
using Windows.Storage;
using static System.Net.Mime.MediaTypeNames;
using System.Net;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TheSteambird
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Overview : Page
    {
        public Overview()
        {
            this.InitializeComponent();
            this.SizeChanged += Overview_SizeChanged;
            Init();
        }

        private void Overview_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            mainGrid.Width = e.NewSize.Width - 10;
        }

        private void Init() {
            string url = "https://www.dmoe.cc/random.php";
            Dictionary<string, string>query = new Dictionary<string, string>() {
            { "return", "json" }
            };
            string returnImg = Api.HttpGet(url, query, new());
            JObject obj = JObject.Parse(returnImg);
            string imgurl = (string)obj["imgurl"];
            image.Source = new BitmapImage(new Uri(imgurl)); ;
        }

        private async void HyperlinkButton_Click(object sender, RoutedEventArgs e)
        {
            var picker = new FolderPicker();
            picker.FileTypeFilter.Add("*");
            WinRT.Interop.InitializeWithWindow.Initialize(picker, GlobalVar.Hwnd);

            StorageFolder folder = await picker.PickSingleFolderAsync();

            if (folder != null)
            {
                var bitmapImage = image.Source as BitmapImage;
                if (bitmapImage != null)
                {
                    string url = bitmapImage.UriSource.ToString();
                    string fileName = Path.GetFileName(url);
                    string filePath = System.IO.Path.Combine(folder.Path, fileName);
                    using (var client = new WebClient())
                    {
                        client.DownloadFile(url, filePath);
                    }
                }
            }
        }
    }
}
