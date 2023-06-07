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
using Microsoft.Web.WebView2.Core;
using System.Net;
using TheSteambird.api;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TheSteambird.webUI
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AddAccountWebview : Page
    {
        string url;
        string type;
        string platform;
        public AddAccountWebview()
        {
            this.InitializeComponent();
        }

        private async void getCookie_Click(object sender, RoutedEventArgs e)
        {
            string cookie = await AddAccountCookieWebview.ExecuteScriptAsync("document.cookie");
            if (type == "genshin" && platform == "0")
            {
                this.Frame.Navigate(typeof(Account), CookieApi.GetGenshinMysCookies(cookie) + $"&type={type}&platform={platform}");
            }
            else if (type == "genshin" && platform == "1") 
            {
                string id = await AddAccountCookieWebview.ExecuteScriptAsync("document.querySelector(\".main-container .content-container .mhy-account-info .account-info-container .account-info-item:nth-child(2) .account-info-value\").innerHTML;");
                string output = string.Empty;
                for (int i = 0; i < id.Length; i++)
                {
                    if (Char.IsDigit(id[i]))
                    {
                        output += id[i];
                    }
                }
                id = output;
                this.Frame.Navigate(typeof(Account), CookieApi.GetGenshinHoyoCookies(cookie, id) + $"&type={type}&platform={platform}");
            }
            else {
                this.Frame.Navigate(typeof(Account));
            }

        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is string && !string.IsNullOrWhiteSpace((string)e.Parameter))
            {
                string input = (string)e.Parameter;
                string[] parts = input.Split('&');
                url = parts[0];
                type = parts[1].Split('=')[1];
                platform = parts[2].Split('=')[1];
                urlText.Text = url;
                AddAccountCookieWebview.Source = new Uri(url);
            }
            base.OnNavigatedTo(e);
        }
    }
}
