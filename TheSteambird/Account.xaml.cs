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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;


using TheSteambird.api;
using TheSteambird.webUI;
using System.Collections.ObjectModel;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TheSteambird
{
    
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Account : Page
    {
        ObservableCollection<GenshinAccount> genshinAccounts = new ObservableCollection<GenshinAccount>();
        public Account()
        {
            this.InitializeComponent();
            this.NavigationCacheMode = NavigationCacheMode.Required;
            List<GenshinAccountData> datas = TheSteambirdApi.GetGenshinDataFromSql();
            foreach(GenshinAccountData data in datas)
            {
                genshinAccounts.Add(new GenshinAccount(data.Name, data.Uid.ToString()));
            }
        }

        private void AddGenshinAccountFromMys_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(AddAccountWebview), "https://user.mihoyo.com/#/login/captcha&type=genshin&platform=0");
            //genshinAccounts.Add(new GenshinAccount("¿ÓÀƒ", "115321123"));
        }

        private void AddGenshinAccountFronHoyo_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(AddAccountWebview), "https://account.hoyoverse.com/&type=genshin&platform=1");
        }

        private void DeleteGenshinAccount_Click(object sender, RoutedEventArgs e)
        {
            if (GenshinAccountList.SelectedIndex != -1)
            {
                genshinAccounts.RemoveAt(GenshinAccountList.SelectedIndex);
            }
        }
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter is string && !string.IsNullOrWhiteSpace((string)e.Parameter))
            {
                string input = (string)e.Parameter;
                string[] parts = input.Split('&');
                string cookies = parts[0];
                string type = parts[1].Split('=')[1];
                string platform = parts[2].Split('=')[1];

                if (type == "genshin")
                { 
                    GenshinAccountData data = TheSteambirdApi.GetGenshinAccountFromCookie(cookies, int.Parse(platform));
                    if (data.Id == -1)
                    {
                        //±®¥Ì
                        infoBar.Message = "ÃÌº”’À∫≈¥ÌŒÛ£¨«Î÷ÿ ‘";
                        infoBar.IsOpen = true;
                    }
                    else { 
                        TheSteambirdApi.InsertGenshinAccountData(data);
                        //genshinAccounts.Add(new GenshinAccount(data.Name, data.Uid.ToString()));
                        genshinAccounts = new ObservableCollection<GenshinAccount>();
                        List<GenshinAccountData> datas = TheSteambirdApi.GetGenshinDataFromSql();
                        foreach (GenshinAccountData accountData in datas)
                        {
                            genshinAccounts.Add(new GenshinAccount(accountData.Name, accountData.Uid.ToString()));
                        }
                    }
                    
                }
                

            }
            base.OnNavigatedTo(e);
        }
    }
}
