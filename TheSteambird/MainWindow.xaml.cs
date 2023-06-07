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
using Scighost.WinUILib.Helpers;
using System.Security.AccessControl;
using WinRT;
using TheSteambird.Genshin;
using TheSteambird.api;
using WinRT.Interop;
// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TheSteambird
{
    /// <summary>
    /// An empty window that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainWindow : Window
    {
        private SystemBackdrop backdropHelper;
        public MainWindow()
        {
            this.InitializeComponent();
            GlobalVar.Hwnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            
            Directory.CreateDirectory(System.AppDomain.CurrentDomain.BaseDirectory + "\\data");
            //Api.CreateFile(GlobalVar.AccountSqlPath);
            if (!Api.IsExistsSqlTable(GlobalVar.AccountSqlPath, GlobalVar.GenshinAccountSqlTable))
            {
                Api.CreateSqlTable(GlobalVar.AccountSqlPath, GlobalVar.GenshinAccountSqlTable, GlobalVar.GenshinAccountSqlAddRow);
            }
            Title = "TheSteamBird";
            backdropHelper = new SystemBackdrop(this);
            backdropHelper.TrySetMica();
            NavigationViewControl.SelectedItem = NavigationViewControl.MenuItems.OfType<NavigationViewItem>().First();
            ContentFrame.Navigate(typeof(Overview));
        }


        public string GetAppTitleFromSystem()
        {
            //return Windows.ApplicationModel.Package.Current.DisplayName;
            return "TheSteambird";
        }


        private void NavigationViewControl_BackRequested(NavigationView sender, NavigationViewBackRequestedEventArgs args)
        {
            if (ContentFrame.CanGoBack) ContentFrame.GoBack();
        }

        private void ContentFrame_Navigated(object sender, NavigationEventArgs e)
        {
            NavigationViewControl.IsBackEnabled = ContentFrame.CanGoBack;

            NavigationViewControl.Header = ((NavigationViewItem)NavigationViewControl.SelectedItem)?.Content?.ToString();
        }

        private void NavigationViewControl_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
        {
            var selectedItem = (NavigationViewItem)args.SelectedItem;
            string pageTag = (string)selectedItem.Tag;
            Type pageType = null;
            if (pageTag == "Overview")
            {
                pageType = typeof(Overview);
            }
            else if (pageTag == "Genshin.GenshinOverview")
            {
                pageType = typeof(GenshinOverview);
            }
            else if (pageTag == "Genshin.GenshinAbyss")
            {
                pageType = typeof(GenshinAbyss);
            }
            else if (pageTag == "Genshin.GenshinGacha")
            {
                pageType = typeof(GenshinGacha);
            }
            else if (pageTag == "Genshin.GenshinEnkaNetwork")
            {
                pageType = typeof(GenshinEnkaNetwork);
            }
            else if (pageTag == "Account")
            {
                pageType = typeof(Account);
            }
            else if (pageTag == "Settings")
            {
                pageType = typeof(Settings);
            }
            ContentFrame.Navigate(pageType);
        }
    }
}
