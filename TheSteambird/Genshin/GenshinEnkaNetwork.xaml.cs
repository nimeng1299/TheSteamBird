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
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using Microsoft.UI.Xaml.Media.Imaging;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using Windows.Storage.Streams;
using System.Data.SqlTypes;
using System.Xml.Serialization;
using Windows.Networking.Connectivity;
using static TheSteambird.api.GenshinEnkaNetworkAPi;
using Microsoft.UI;
using Windows.UI;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace TheSteambird.Genshin
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    

    public sealed partial class GenshinEnkaNetwork : Page
    {
        ObservableCollection<GenshinAccount> genshinAccounts = new ObservableCollection<GenshinAccount>();
        ObservableCollection<GenshinEnkaNetworkIcon> icons = new ObservableCollection<GenshinEnkaNetworkIcon>();
        List<GenshinEnkaNetworkAvatarData> datas = new List<GenshinEnkaNetworkAvatarData>();
        
        public GenshinEnkaNetwork()
        {
            this.InitializeComponent();
            GlobalProp.prop.OnNowPropChanged += Prop_OnNowPropChanged;
            Api.CreateFile(GlobalVar.GenshinEnkaNetworkSqlPath);
            Api.EnsureDirectoryExists(System.AppDomain.CurrentDomain.BaseDirectory + "res\\genshin\\EnkaNetwork");
            if (!Api.IsExistsSqlTable(GlobalVar.GenshinEnkaNetworkSqlPath, "info"))
            {
                Api.CreateSqlTable(GlobalVar.GenshinEnkaNetworkSqlPath, "info", GlobalVar.GenshinEnkaNetworkSqlInfoAddRow);
            }
            UpdateCombo();
            
        }

        private void Prop_OnNowPropChanged(object sender, EventArgs e)
        {
            string prop = GlobalProp.prop.NowProp;
            List<BorderProp> borderProps = new List<BorderProp>() {
                WeaponMain,
                WeaponAppend,
                EQUIP_BRACER_Prop,
                EQUIP_BRACER_SubProp1,
                EQUIP_BRACER_SubProp2,
                EQUIP_BRACER_SubProp3,
                EQUIP_BRACER_SubProp4,
                EQUIP_NECKLACE_Prop,
                EQUIP_NECKLACE_SubProp1,
                EQUIP_NECKLACE_SubProp2,
                EQUIP_NECKLACE_SubProp3,
                EQUIP_NECKLACE_SubProp4,
                EQUIP_SHOES_Prop,
                EQUIP_SHOES_SubProp1,
                EQUIP_SHOES_SubProp2,
                EQUIP_SHOES_SubProp3,
                EQUIP_SHOES_SubProp4,
                EQUIP_RING_Prop,
                EQUIP_RING_SubProp1,
                EQUIP_RING_SubProp2,
                EQUIP_RING_SubProp3,
                EQUIP_RING_SubProp4,
                EQUIP_DRESS_Prop,
                EQUIP_DRESS_SubProp1,
                EQUIP_DRESS_SubProp2,
                EQUIP_DRESS_SubProp3,
                EQUIP_DRESS_SubProp4,
                FightHPProp,
                FightATKProp,
                FightDEFProp,
                FightElementalMasteryProp,
                FightCRITRateProp,
                FightCRITDMGProp,
                FightEnergyRechargeProp,
                FightDMGBonusProp
            };
            Dictionary<string, string> appendProp = new Dictionary<string, string>
            {
                { "FIGHT_PROP_BASE_ATTACK", "2" },
                { "FIGHT_PROP_HP", "1" },
                { "FIGHT_PROP_ATTACK", "2" },
                { "FIGHT_PROP_DEFENSE", "3" },
                { "FIGHT_PROP_HP_PERCENT", "1" },
                { "FIGHT_PROP_ATTACK_PERCENT", "2" },
                { "FIGHT_PROP_DEFENSE_PERCENT", "3" },
                { "FIGHT_PROP_CRITICAL", "5" },
                { "FIGHT_PROP_CRITICAL_HURT", "6" },
                { "FIGHT_PROP_CHARGE_EFFICIENCY", "7" },
                { "FIGHT_PROP_HEAL_ADD", "8" },
                { "FIGHT_PROP_ELEMENT_MASTERY", "4" },
                { "FIGHT_PROP_PHYSICAL_ADD_HURT", "10" },
                { "FIGHT_PROP_FIRE_ADD_HURT", "11" },
                { "FIGHT_PROP_ELEC_ADD_HURT", "12" },
                { "FIGHT_PROP_WATER_ADD_HURT", "13" },
                { "FIGHT_PROP_WIND_ADD_HURT", "14" },
                { "FIGHT_PROP_ICE_ADD_HURT", "15" },
                { "FIGHT_PROP_ROCK_ADD_HURT", "16" },
                { "FIGHT_PROP_GRASS_ADD_HURT",  "17"}
            };
            foreach (BorderProp borderProp in borderProps)
            {

                Border border = null;
                int childCount = VisualTreeHelper.GetChildrenCount(borderProp);
                if (childCount == 0)
                    continue;
                for (int i = 0; i < childCount; i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(borderProp, i);
                    if (child is Border)
                    {
                        border = (Border)child;
                        break;
                    }
                }
                if (border == null)
                    continue;

                if (prop == null || prop == "null" || prop == string.Empty || !appendProp.ContainsKey(prop) || !appendProp.ContainsKey(borderProp.Prop))
                {
                    border.Background = new SolidColorBrush(Colors.Transparent);
                    continue;
                }

                if (appendProp[borderProp.Prop] == appendProp[prop])
                {
                    Color color = Colors.Black;
                    color.A = 128; // 设置透明度为0.5
                    border.Background = new SolidColorBrush(color);
                }
                else {
                    border.Background = new SolidColorBrush(Colors.Transparent);
                }
            }
        }

        private void UpdateCombo()
        {
            genshinAccounts.Clear();
            List<string> uids = Api.GetAllSqlTableNames(GlobalVar.GenshinEnkaNetworkSqlPath);
            foreach (var uid in uids)
            {
                if (uid == "info")
                {
                    continue;
                }
                genshinAccounts.Add(new GenshinAccount(uid, uid));
            }
            if (genshinAccounts.Count > 0)
            {
                accountCombo.SelectedIndex = 0;
            }
        }

        private void accountCombo_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var account = (GenshinAccount)accountCombo.SelectedItem;
            if (account == null)
                return;
            string uid = account.Uid;
            datas = GenshinEnkaNetworkAPi.LoadSqlDataToList(uid);
            icons.Clear();
            foreach (var data in datas) {
                int id = data.avatarID;
                JObject obj = Api.FileToJson(GlobalVar.GenshinEnkaNetworkDataPath + "\\characters.json");
                JObject character = (JObject)obj[id.ToString()];
                string icon = (string)character["SideIconName"];
                icons.Add(new GenshinEnkaNetworkIcon(GenshinEnkaNetworkAPi.GetIcon(icon), id));
            }
            if (icons.Count > 0)
            {
                AvatarList.SelectedIndex = 0;
                GenshinEnkaNetworkIcon icon = icons[0] as GenshinEnkaNetworkIcon;
                UpdateEnkaData(icon.Id);
            }

        }

        private async void getData_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new ContentDialog();
            var textBox = new TextBox();
            dialog.Content = textBox;
            dialog.XamlRoot = this.XamlRoot;
            dialog.Title = "请输入UID";
            dialog.PrimaryButtonText = "确定";
            dialog.SecondaryButtonText = "取消";
            var result = await dialog.ShowAsync();
            string data;
            if (result == ContentDialogResult.Primary)
            {
                string uid = textBox.Text;
                try {
                    data = GenshinEnkaNetworkAPi.GetUIDData(uid);
                    GenshinEnkaNetworkAPi.ProcessData(data);
                }
                catch (Exception ex)
                {
                    infoBar.Message = ex.Message;
                    infoBar.IsOpen = true;
                    return;
                }
            }
            else
            {
                return;
            }

        }

        private void ListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            var clicked = e.ClickedItem as GenshinEnkaNetworkIcon;
            int id = clicked.Id;
            UpdateEnkaData(id);


        }

        private void UpdateEnkaData(int id) {
            JObject charactersObj = Api.FileToJson(GlobalVar.GenshinEnkaNetworkDataPath + "\\characters.json");
            GenshinEnkaNetworkAvatarData Avatar = new GenshinEnkaNetworkAvatarData();
            foreach (GenshinEnkaNetworkAvatarData data in datas)
            {
                if (data.avatarID == id)
                {
                    Avatar = data;
                    break;
                }
            }
            string idStr = id.ToString();
            if (id == 10000005 || id == 10000007)
            {
                idStr = $"{id}-{Avatar.skillDepotId}";
            }
            JObject character = (JObject)charactersObj[idStr];
            string icon = (string)character["SideIconName"];
            string[] parts = icon.Split('_');
            icon = "UI_Gacha_AvatarImg_" + parts[parts.Length - 1];

            InitUIData();

            UI_Gacha_AvatarImg.Source = new BitmapImage(new Uri($"ms-appx://{GenshinEnkaNetworkAPi.GetIcon(icon)}"));
            switch ((string)character["Element"])
            {
                case "Wind":
                    background.Source = new BitmapImage(new Uri($"ms-appx:///res/genshin/EnkaNetwork/ANEMO.png"));
                    break;
                case "Ice":
                    background.Source = new BitmapImage(new Uri($"ms-appx:///res/genshin/EnkaNetwork/CRYO.png"));
                    break;
                case "Grass":
                    background.Source = new BitmapImage(new Uri($"ms-appx:///res/genshin/EnkaNetwork/DENDRO.png"));
                    break;
                case "Electric":
                    background.Source = new BitmapImage(new Uri($"ms-appx:///res/genshin/EnkaNetwork/ELECTRO.png"));
                    break;
                case "Rock":
                    background.Source = new BitmapImage(new Uri($"ms-appx:///res/genshin/EnkaNetwork/GEO.png"));
                    break;
                case "Water":
                    background.Source = new BitmapImage(new Uri($"ms-appx:///res/genshin/EnkaNetwork/GYDRO.png"));
                    break;
                case "Fire":
                    background.Source = new BitmapImage(new Uri($"ms-appx:///res/genshin/EnkaNetwork/PYRO.png"));
                    break;

            }
            JObject locObj = Api.FileToJson(GlobalVar.GenshinEnkaNetworkDataPath + "\\loc.json");
            JObject loc = (JObject)locObj["zh-CN"];
            long characterNameTextMapHash = (long)character["NameTextMapHash"];
            avatarID.Text = (string)loc[characterNameTextMapHash.ToString()];
            level.Text = Avatar.propMap["4001"];
            expLevel.Text = Avatar.expLevel.ToString();

            JArray constsArr = (JArray)character["Consts"];
            List<string> consts = constsArr.ToObject<List<string>>();
            const1.Source = new BitmapImage(new Uri($"ms-appx://{GenshinEnkaNetworkAPi.GetIcon(consts[0])}"));
            const2.Source = new BitmapImage(new Uri($"ms-appx://{GenshinEnkaNetworkAPi.GetIcon(consts[1])}"));
            const3.Source = new BitmapImage(new Uri($"ms-appx://{GenshinEnkaNetworkAPi.GetIcon(consts[2])}"));
            const4.Source = new BitmapImage(new Uri($"ms-appx://{GenshinEnkaNetworkAPi.GetIcon(consts[3])}"));
            const5.Source = new BitmapImage(new Uri($"ms-appx://{GenshinEnkaNetworkAPi.GetIcon(consts[4])}"));
            const6.Source = new BitmapImage(new Uri($"ms-appx://{GenshinEnkaNetworkAPi.GetIcon(consts[5])}"));
            switch (Avatar.talentIdList.Count)
            {
                case 0:
                    constBorder1.BorderThickness = new Thickness(0);
                    const1.Opacity = 0.5;
                    constBorder2.BorderThickness = new Thickness(0);
                    const2.Opacity = 0.5;
                    constBorder3.BorderThickness = new Thickness(0);
                    const3.Opacity = 0.5;
                    constBorder4.BorderThickness = new Thickness(0);
                    const4.Opacity = 0.5;
                    constBorder5.BorderThickness = new Thickness(0);
                    const5.Opacity = 0.5;
                    constBorder6.BorderThickness = new Thickness(0);
                    const6.Opacity = 0.5;
                    break;
                case 1:
                    constBorder1.BorderThickness = new Thickness(2);
                    const1.Opacity = 1;
                    constBorder2.BorderThickness = new Thickness(0);
                    const2.Opacity = 0.5;
                    constBorder3.BorderThickness = new Thickness(0);
                    const3.Opacity = 0.5;
                    constBorder4.BorderThickness = new Thickness(0);
                    const4.Opacity = 0.5;
                    constBorder5.BorderThickness = new Thickness(0);
                    const5.Opacity = 0.5;
                    constBorder6.BorderThickness = new Thickness(0);
                    const6.Opacity = 0.5;
                    break;
                case 2:
                    constBorder1.BorderThickness = new Thickness(2);
                    const1.Opacity = 1;
                    constBorder2.BorderThickness = new Thickness(2);
                    const2.Opacity = 1;
                    constBorder3.BorderThickness = new Thickness(0);
                    const3.Opacity = 0.5;
                    constBorder4.BorderThickness = new Thickness(0);
                    const4.Opacity = 0.5;
                    constBorder5.BorderThickness = new Thickness(0);
                    const5.Opacity = 0.5;
                    constBorder6.BorderThickness = new Thickness(0);
                    const6.Opacity = 0.5;
                    break;
                case 3:
                    constBorder1.BorderThickness = new Thickness(2);
                    const1.Opacity = 1;
                    constBorder2.BorderThickness = new Thickness(2);
                    const2.Opacity = 1;
                    constBorder3.BorderThickness = new Thickness(2);
                    const3.Opacity = 1;
                    constBorder4.BorderThickness = new Thickness(0);
                    const4.Opacity = 0.5;
                    constBorder5.BorderThickness = new Thickness(0);
                    const5.Opacity = 0.5;
                    constBorder6.BorderThickness = new Thickness(0);
                    const6.Opacity = 0.5;
                    break;
                case 4:
                    constBorder1.BorderThickness = new Thickness(2);
                    const1.Opacity = 1;
                    constBorder2.BorderThickness = new Thickness(2);
                    const2.Opacity = 1;
                    constBorder3.BorderThickness = new Thickness(2);
                    const3.Opacity = 1;
                    constBorder4.BorderThickness = new Thickness(2);
                    const4.Opacity = 1;
                    constBorder5.BorderThickness = new Thickness(0);
                    const5.Opacity = 0.5;
                    constBorder6.BorderThickness = new Thickness(0);
                    const6.Opacity = 0.5;
                    break;
                case 5:
                    constBorder1.BorderThickness = new Thickness(2);
                    const1.Opacity = 1;
                    constBorder2.BorderThickness = new Thickness(2);
                    const2.Opacity = 1;
                    constBorder3.BorderThickness = new Thickness(2);
                    const3.Opacity = 1;
                    constBorder4.BorderThickness = new Thickness(2);
                    const4.Opacity = 1;
                    constBorder5.BorderThickness = new Thickness(2);
                    const5.Opacity = 1;
                    constBorder6.BorderThickness = new Thickness(0);
                    const6.Opacity = 0.5;
                    break;
                case 6:
                    constBorder1.BorderThickness = new Thickness(2);
                    const1.Opacity = 1;
                    constBorder2.BorderThickness = new Thickness(2);
                    const2.Opacity = 1;
                    constBorder3.BorderThickness = new Thickness(2);
                    const3.Opacity = 1;
                    constBorder4.BorderThickness = new Thickness(2);
                    const4.Opacity = 1;
                    constBorder5.BorderThickness = new Thickness(2);
                    const5.Opacity = 1;
                    constBorder6.BorderThickness = new Thickness(2);
                    const6.Opacity = 1;
                    break;
                default:
                    break;
            }

            Dictionary<string, int> skillLevelMap = Avatar.skillLevelMap;
            JObject SkillsObj = (JObject)character["Skills"];
            JArray SkillOrderArr = (JArray)character["SkillOrder"];
            List<int> SkillOrder = SkillOrderArr.ToObject<List<int>>();
            int skillId1 = SkillOrder[0];
            skillImage1.Source = new BitmapImage(new Uri($"ms-appx://{GenshinEnkaNetworkAPi.GetIcon((string)SkillsObj[skillId1.ToString()])}"));
            skillLevel1.Text = skillLevelMap[skillId1.ToString()].ToString();
            int skillId2 = SkillOrder[1];
            skillImage2.Source = new BitmapImage(new Uri($"ms-appx://{GenshinEnkaNetworkAPi.GetIcon((string)SkillsObj[skillId2.ToString()])}"));
            skillLevel2.Text = skillLevelMap[skillId2.ToString()].ToString();
            int skillId3 = SkillOrder[2];
            skillImage3.Source = new BitmapImage(new Uri($"ms-appx://{GenshinEnkaNetworkAPi.GetIcon((string)SkillsObj[skillId3.ToString()])}"));
            skillLevel3.Text = skillLevelMap[skillId3.ToString()].ToString();

            GenshinEnkaNetworkWeapon weapon = Avatar.weapon;
            int weaponId = weapon.itemId;
            JArray weaponArr = Api.FileToJarray(GlobalVar.GenshinEnkaNetworkDataPath + "\\WeaponExcelConfigData.json");
            JObject weaponObj = new JObject();
            foreach (JObject weaponobject in weaponArr)
            { 
                if(!weaponobject.ContainsKey("id"))
                {
                    continue;
                }
                if ((int)weaponobject["id"] == weaponId)
                { 
                    weaponObj = weaponobject;
                    break;
                }
            }
            long WeaponNameTextMapHash = (long)weaponObj["nameTextMapHash"];
            WeaponImage.Source = new BitmapImage(new Uri($"ms-appx://{GenshinEnkaNetworkAPi.GetIcon((string)weaponObj["awakenIcon"])}"));
            WeaponName.Text = (string)loc[WeaponNameTextMapHash.ToString()];
            GenshinEnkaNetworkWeaponFlat weaponFlat = weapon.flat;
            JArray weaponProp = (JArray)weaponObj["weaponProp"];
            string weaponMainType = (string)((JObject)weaponProp[0])["propType"];
            WeaponMainName.Text = GenshinEnkaNetworkAPi.appendProp[weaponMainType];
            WeaponMainValue.Text = weaponFlat.weaponStats[weaponMainType].ToString();
            WeaponMain.Prop = weaponMainType;
            string weaponAppendType = "null";
            if ((!(weaponProp.Count < 2)) && ((JObject)weaponProp[1]).ContainsKey("propType"))
            {
                weaponAppendType = (string)((JObject)weaponProp[1])["propType"];
                WeaponAppendName.Text = GenshinEnkaNetworkAPi.appendProp[weaponAppendType];
                WeaponAppendValue.Text = weaponFlat.weaponStats[weaponAppendType].ToString();
                WeaponAppend.Prop = weaponAppendType;
            }
            else {
                WeaponAppendName.Text = "";
                WeaponAppendValue.Text = "";
                WeaponAppend.Prop = "null";
            }
            WeaponAffix.Text = $"R{weapon.affixMap}";
            WeaponLevel.Text = weapon.level.ToString();

            foreach (GenshinEnkaNetworkReliquary reliquary in Avatar.reliquaryList)
            {
                GenshinEnkaNetworkReliquaryFlat reliquaryFlat = reliquary.flat;
                var mainName = reliquaryFlat.reliquaryMainstat.Keys;
                string MainName = "";
                double mainValue = 0;
                foreach (string key in mainName)
                {
                    MainName = key;
                    mainValue = reliquaryFlat.reliquaryMainstat[key];
                }
                var SubName = reliquaryFlat.reliquarySubstats.Keys;
                List<string> SubNames = new List<string>();
                List<double> SubValue = new();
                foreach(string key in SubName)
                {
                    SubNames.Add(key);
                    SubValue.Add(reliquaryFlat.reliquarySubstats[key]);
                }
                switch (reliquaryFlat.equipType)
                {
                    case "EQUIP_BRACER":
                        EQUIP_BRACER_Prop.Prop = MainName;
                        EQUIP_BRACER_Image.Source = new BitmapImage(new Uri($"ms-appx://{GenshinEnkaNetworkAPi.GetIcon(reliquaryFlat.icon)}"));
                        EQUIP_BRACER_MainName.Text = GenshinEnkaNetworkAPi.appendProp[MainName];
                        EQUIP_BRACER_MainValue.Text = mainValue.ToString();
                        EQUIP_BRACER_Rating.MaxRating = reliquaryFlat.rankLevel;
                        EQUIP_BRACER_Rating.Value = reliquaryFlat.rankLevel;
                        EQUIP_BRACER_Rating.Visibility = Visibility.Visible;
                        EQUIP_BRACER_Level.Text = "+" +  (reliquary.level - 1).ToString();
                        for (int i = 0; i < SubNames.Count; i++)
                        {
                            switch (i) {
                                case 0:
                                    EQUIP_BRACER_SubProp1.Prop = SubNames[i];
                                    EQUIP_BRACER_SubName1.Text = Api.PadString(GenshinEnkaNetworkAPi.appendProp[SubNames[i]], 16);
                                    EQUIP_BRACER_SubValue1.Text = Api.PadString(SubValue[i].ToString(), 10);
                                    break;
                                case 1:
                                    EQUIP_BRACER_SubProp2.Prop = SubNames[i];
                                    EQUIP_BRACER_SubName2.Text = Api.PadString(GenshinEnkaNetworkAPi.appendProp[SubNames[i]], 16);
                                    EQUIP_BRACER_SubValue2.Text = Api.PadString(SubValue[i].ToString(), 10);
                                    break;
                                case 2:
                                    EQUIP_BRACER_SubProp3.Prop = SubNames[i];
                                    EQUIP_BRACER_SubName3.Text = Api.PadString(GenshinEnkaNetworkAPi.appendProp[SubNames[i]], 16);
                                    EQUIP_BRACER_SubValue3.Text = Api.PadString(SubValue[i].ToString(), 10);
                                    break;
                                case 3:
                                    EQUIP_BRACER_SubProp4.Prop = SubNames[i];
                                    EQUIP_BRACER_SubName4.Text = Api.PadString(GenshinEnkaNetworkAPi.appendProp[SubNames[i]], 16);
                                    EQUIP_BRACER_SubValue4.Text = Api.PadString(SubValue[i].ToString(), 10);
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    case "EQUIP_NECKLACE":
                        EQUIP_NECKLACE_Prop.Prop = MainName;
                        EQUIP_NECKLACE_Image.Source = new BitmapImage(new Uri($"ms-appx://{GenshinEnkaNetworkAPi.GetIcon(reliquaryFlat.icon)}"));
                        EQUIP_NECKLACE_MainName.Text = GenshinEnkaNetworkAPi.appendProp[MainName];
                        EQUIP_NECKLACE_MainValue.Text = mainValue.ToString();
                        EQUIP_NECKLACE_Rating.MaxRating = reliquaryFlat.rankLevel;
                        EQUIP_NECKLACE_Rating.Value = reliquaryFlat.rankLevel;
                        EQUIP_NECKLACE_Rating.Visibility = Visibility.Visible;
                        EQUIP_NECKLACE_Level.Text = "+" + (reliquary.level - 1).ToString();
                        for (int i = 0; i < SubNames.Count; i++)
                        {
                            switch (i)
                            {
                                case 0:
                                    EQUIP_NECKLACE_SubProp1.Prop = SubNames[i];
                                    EQUIP_NECKLACE_SubName1.Text = Api.PadString(GenshinEnkaNetworkAPi.appendProp[SubNames[i]], 16);
                                    EQUIP_NECKLACE_SubValue1.Text = Api.PadString(SubValue[i].ToString(), 10);
                                    break;
                                case 1:
                                    EQUIP_NECKLACE_SubProp2.Prop = SubNames[i];
                                    EQUIP_NECKLACE_SubName2.Text = Api.PadString(GenshinEnkaNetworkAPi.appendProp[SubNames[i]], 16);
                                    EQUIP_NECKLACE_SubValue2.Text = Api.PadString(SubValue[i].ToString(), 10);
                                    break;
                                case 2:
                                    EQUIP_NECKLACE_SubProp3.Prop = SubNames[i];
                                    EQUIP_NECKLACE_SubName3.Text = Api.PadString(GenshinEnkaNetworkAPi.appendProp[SubNames[i]], 16);
                                    EQUIP_NECKLACE_SubValue3.Text = Api.PadString(SubValue[i].ToString(), 10);
                                    break;
                                case 3:
                                    EQUIP_NECKLACE_SubProp4.Prop = SubNames[i];
                                    EQUIP_NECKLACE_SubName4.Text = Api.PadString(GenshinEnkaNetworkAPi.appendProp[SubNames[i]], 16);
                                    EQUIP_NECKLACE_SubValue4.Text = Api.PadString(SubValue[i].ToString(), 10);
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    case "EQUIP_SHOES":
                        EQUIP_SHOES_Prop.Prop = MainName;
                        EQUIP_SHOES_Image.Source = new BitmapImage(new Uri($"ms-appx://{GenshinEnkaNetworkAPi.GetIcon(reliquaryFlat.icon)}"));
                        EQUIP_SHOES_MainName.Text = GenshinEnkaNetworkAPi.appendProp[MainName];
                        EQUIP_SHOES_MainValue.Text = mainValue.ToString();
                        EQUIP_SHOES_Rating.MaxRating = reliquaryFlat.rankLevel;
                        EQUIP_SHOES_Rating.Value = reliquaryFlat.rankLevel;
                        EQUIP_SHOES_Rating.Visibility = Visibility.Visible;
                        EQUIP_SHOES_Level.Text = "+" + (reliquary.level - 1).ToString();
                        for (int i = 0; i < SubNames.Count; i++)
                        {
                            switch (i)
                            {
                                case 0:
                                    EQUIP_SHOES_SubProp1.Prop = SubNames[i];
                                    EQUIP_SHOES_SubName1.Text = Api.PadString(GenshinEnkaNetworkAPi.appendProp[SubNames[i]], 16);
                                    EQUIP_SHOES_SubValue1.Text = Api.PadString(SubValue[i].ToString(), 10);
                                    break;
                                case 1:
                                    EQUIP_SHOES_SubProp2.Prop = SubNames[i];
                                    EQUIP_SHOES_SubName2.Text = Api.PadString(GenshinEnkaNetworkAPi.appendProp[SubNames[i]], 16);
                                    EQUIP_SHOES_SubValue2.Text = Api.PadString(SubValue[i].ToString(), 10);
                                    break;
                                case 2:
                                    EQUIP_SHOES_SubProp3.Prop = SubNames[i];
                                    EQUIP_SHOES_SubName3.Text = Api.PadString(GenshinEnkaNetworkAPi.appendProp[SubNames[i]], 16);
                                    EQUIP_SHOES_SubValue3.Text = Api.PadString(SubValue[i].ToString(), 10);
                                    break;
                                case 3:
                                    EQUIP_SHOES_SubProp4.Prop = SubNames[i];
                                    EQUIP_SHOES_SubName4.Text = Api.PadString(GenshinEnkaNetworkAPi.appendProp[SubNames[i]], 16);
                                    EQUIP_SHOES_SubValue4.Text = Api.PadString(SubValue[i].ToString(), 10);
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    case "EQUIP_RING":
                        EQUIP_RING_Prop.Prop = MainName;
                        EQUIP_RING_Image.Source = new BitmapImage(new Uri($"ms-appx://{GenshinEnkaNetworkAPi.GetIcon(reliquaryFlat.icon)}"));
                        EQUIP_RING_MainName.Text = GenshinEnkaNetworkAPi.appendProp[MainName];
                        EQUIP_RING_MainValue.Text = mainValue.ToString();
                        EQUIP_RING_Rating.MaxRating = reliquaryFlat.rankLevel;
                        EQUIP_RING_Rating.Value = reliquaryFlat.rankLevel;
                        EQUIP_RING_Rating.Visibility = Visibility.Visible;
                        EQUIP_RING_Level.Text = "+" + (reliquary.level - 1).ToString();
                        for (int i = 0; i < SubNames.Count; i++)
                        {
                            switch (i)
                            {
                                case 0:
                                    EQUIP_RING_SubProp1.Prop = SubNames[i];
                                    EQUIP_RING_SubName1.Text = Api.PadString(GenshinEnkaNetworkAPi.appendProp[SubNames[i]], 16);
                                    EQUIP_RING_SubValue1.Text = Api.PadString(SubValue[i].ToString(), 10);
                                    break;
                                case 1:
                                    EQUIP_RING_SubProp2.Prop = SubNames[i];
                                    EQUIP_RING_SubName2.Text = Api.PadString(GenshinEnkaNetworkAPi.appendProp[SubNames[i]], 16);
                                    EQUIP_RING_SubValue2.Text = Api.PadString(SubValue[i].ToString(), 10);
                                    break;
                                case 2:
                                    EQUIP_RING_SubProp3.Prop = SubNames[i];
                                    EQUIP_RING_SubName3.Text = Api.PadString(GenshinEnkaNetworkAPi.appendProp[SubNames[i]], 16);
                                    EQUIP_RING_SubValue3.Text = Api.PadString(SubValue[i].ToString(), 10);
                                    break;
                                case 3:
                                    EQUIP_RING_SubProp4.Prop = SubNames[i];
                                    EQUIP_RING_SubName4.Text = Api.PadString(GenshinEnkaNetworkAPi.appendProp[SubNames[i]], 16);
                                    EQUIP_RING_SubValue4.Text = Api.PadString(SubValue[i].ToString(), 10);
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    case "EQUIP_DRESS":
                        EQUIP_DRESS_Prop.Prop = MainName;
                        EQUIP_DRESS_Image.Source = new BitmapImage(new Uri($"ms-appx://{GenshinEnkaNetworkAPi.GetIcon(reliquaryFlat.icon)}"));
                        EQUIP_DRESS_MainName.Text = GenshinEnkaNetworkAPi.appendProp[MainName];
                        EQUIP_DRESS_MainValue.Text = mainValue.ToString();
                        EQUIP_DRESS_Rating.MaxRating = reliquaryFlat.rankLevel;
                        EQUIP_DRESS_Rating.Value = reliquaryFlat.rankLevel;
                        EQUIP_DRESS_Rating.Visibility = Visibility.Visible;
                        EQUIP_DRESS_Level.Text = "+" + (reliquary.level - 1).ToString();
                        for (int i = 0; i < SubNames.Count; i++)
                        {
                            switch (i)
                            {
                                case 0:
                                    EQUIP_DRESS_SubProp1.Prop = SubNames[i];
                                    EQUIP_DRESS_SubName1.Text = Api.PadString(GenshinEnkaNetworkAPi.appendProp[SubNames[i]], 16);
                                    EQUIP_DRESS_SubValue1.Text = Api.PadString(SubValue[i].ToString(), 10);
                                    break;
                                case 1:
                                    EQUIP_DRESS_SubProp2.Prop = SubNames[i];
                                    EQUIP_DRESS_SubName2.Text = Api.PadString(GenshinEnkaNetworkAPi.appendProp[SubNames[i]], 16);
                                    EQUIP_DRESS_SubValue2.Text = Api.PadString(SubValue[i].ToString(), 10);
                                    break;
                                case 2:
                                    EQUIP_DRESS_SubProp3.Prop = SubNames[i];
                                    EQUIP_DRESS_SubName3.Text = Api.PadString(GenshinEnkaNetworkAPi.appendProp[SubNames[i]], 16);
                                    EQUIP_DRESS_SubValue3.Text = Api.PadString(SubValue[i].ToString(), 10);
                                    break;
                                case 3:
                                    EQUIP_DRESS_SubProp4.Prop = SubNames[i];
                                    EQUIP_DRESS_SubName4.Text = Api.PadString(GenshinEnkaNetworkAPi.appendProp[SubNames[i]], 16);
                                    EQUIP_DRESS_SubValue4.Text = Api.PadString(SubValue[i].ToString(), 10);
                                    break;
                                default:
                                    break;
                            }
                        }
                        break;
                    default:
                        break;
                }
            }

            Func<string, double> GetFight = (key) =>
            {
                var fightProp = Avatar.fightProp;
                if (fightProp.ContainsKey(key))
                {
                    return fightProp[key];
                }
                else
                {
                    return 0;
                }
            };

            FightHPBase.Text = Math.Round(GetFight("1"), 2).ToString();
            FightHPAppend.Text = Math.Round((GetFight("2000") - GetFight("1")), 2).ToString();
            FightATKBase.Text = Math.Round(GetFight("4"), 2).ToString();
            FightATKAppend.Text = Math.Round((GetFight("2001") - GetFight("4")), 2).ToString();
            FightDEFBase.Text = Math.Round(GetFight("7"), 2).ToString();
            FightDEFAppend.Text = Math.Round((GetFight("2002") - GetFight("7")), 2).ToString();
            FightElementalMasteryValue.Text = Math.Round(GetFight("28"), 2).ToString();
            FightCRITRateValue.Text = Math.Round(GetFight("20") * 100, 2).ToString();
            FightCRITDMGValue.Text = Math.Round(GetFight("22") * 100, 2).ToString();
            FightEnergyRechargeValue.Text = Math.Round(GetFight("23") * 100, 2).ToString();
            switch ((string)character["Element"])
            {
                case "Wind":
                    FightDMGBonusProp.Prop = "FIGHT_PROP_WIND_ADD_HURT";
                    FightDMGBonusName.Text = "风元素伤害加成";
                    FightDMGBonusValue.Text = Math.Round(GetFight("44") * 100, 2).ToString();
                    break;
                case "Ice":
                    FightDMGBonusProp.Prop = "FIGHT_PROP_ICE_ADD_HURT";
                    FightDMGBonusName.Text = "冰元素伤害加成";
                    FightDMGBonusValue.Text = Math.Round(GetFight("46") * 100, 2).ToString();
                    break;
                case "Grass":
                    FightDMGBonusProp.Prop = "FIGHT_PROP_GRASS_ADD_HURT";
                    FightDMGBonusName.Text = "草元素伤害加成";
                    FightDMGBonusValue.Text = Math.Round(GetFight("43") * 100, 2).ToString();
                    break;
                case "Electric":
                    FightDMGBonusProp.Prop = "FIGHT_PROP_ELEC_ADD_HURT";
                    FightDMGBonusName.Text = "雷元素伤害加成";
                    FightDMGBonusValue.Text = Math.Round(GetFight("41") * 100, 2).ToString();
                    break;
                case "Rock":
                    FightDMGBonusProp.Prop = "FIGHT_PROP_ROCK_ADD_HURT";
                    FightDMGBonusName.Text = "岩元素伤害加成";
                    FightDMGBonusValue.Text = Math.Round(GetFight("45") * 100, 2).ToString();
                    break;
                case "Water":
                    FightDMGBonusProp.Prop = "FIGHT_PROP_WATER_ADD_HURT";
                    FightDMGBonusName.Text = "水元素伤害加成";
                    FightDMGBonusValue.Text = Math.Round(GetFight("42") * 100, 2).ToString();
                    break;
                case "Fire":
                    FightDMGBonusProp.Prop = "FIGHT_PROP_FIRE_ADD_HURT";
                    FightDMGBonusName.Text = "火元素伤害加成";
                    FightDMGBonusValue.Text = Math.Round(GetFight("40") * 100, 2).ToString();
                    break;
            }
        }

        private void InitUIData() {
            UI_Gacha_AvatarImg.Source = null;
            avatarID.Text = string.Empty;
            level.Text = "0";
            expLevel.Text = "0";

            const1.Source = null;
            const2.Source = null;
            const3.Source = null;
            const4.Source = null;
            const5.Source = null;
            const6.Source = null;
            const1.Opacity = 0.5;
            const2.Opacity = 0.5;
            const3.Opacity = 0.5;
            const4.Opacity = 0.5;
            const5.Opacity = 0.5;
            const6.Opacity = 0.5;
            constBorder1.BorderThickness = new Thickness(0);
            constBorder2.BorderThickness = new Thickness(0);
            constBorder3.BorderThickness = new Thickness(0);
            constBorder4.BorderThickness = new Thickness(0);
            constBorder5.BorderThickness = new Thickness(0);
            constBorder6.BorderThickness = new Thickness(0);
            skillImage1.Source = null;
            skillImage2.Source = null;
            skillImage3.Source = null;
            skillLevel1.Text = "0";
            skillLevel2.Text = "0";
            skillLevel3.Text = "0";

            WeaponImage.Source = null;
            WeaponName.Text = string.Empty;
            WeaponMainName.Text = string.Empty;
            WeaponMainValue.Text = string.Empty;
            WeaponMain.Prop = "null";
            WeaponAppendName.Text = string.Empty;
            WeaponAppendValue.Text = string.Empty;
            WeaponAppend.Prop = "null";
            WeaponAffix.Text = "R0";
            WeaponLevel.Text = "0";

            EQUIP_BRACER_Prop.Prop = "null";
            EQUIP_BRACER_Image.Source = null;
            EQUIP_BRACER_MainName.Text = string.Empty;
            EQUIP_BRACER_MainValue.Text = string.Empty;
            EQUIP_BRACER_Rating.Value = 0;
            EQUIP_BRACER_Rating.MaxRating = 0;
            EQUIP_BRACER_Rating.Visibility = Visibility.Collapsed;
            EQUIP_BRACER_Level.Text = string.Empty;
            EQUIP_BRACER_SubProp1.Prop = "null";
            EQUIP_BRACER_SubName1.Text = string.Empty;
            EQUIP_BRACER_SubValue1.Text = string.Empty;
            EQUIP_BRACER_SubProp1.Prop = "null";
            EQUIP_BRACER_SubName2.Text = string.Empty;
            EQUIP_BRACER_SubValue2.Text = string.Empty;
            EQUIP_BRACER_SubProp3.Prop = "null";
            EQUIP_BRACER_SubName3.Text = string.Empty;
            EQUIP_BRACER_SubValue3.Text = string.Empty;
            EQUIP_BRACER_SubProp4.Prop = "null";
            EQUIP_BRACER_SubName4.Text = string.Empty;
            EQUIP_BRACER_SubValue4.Text = string.Empty;

            EQUIP_NECKLACE_Prop.Prop = "null";
            EQUIP_NECKLACE_Image.Source = null;
            EQUIP_NECKLACE_MainName.Text = string.Empty;
            EQUIP_NECKLACE_MainValue.Text = string.Empty;
            EQUIP_NECKLACE_Rating.Value = 0;
            EQUIP_NECKLACE_Rating.MaxRating = 0;
            EQUIP_NECKLACE_Rating.Visibility = Visibility.Collapsed;
            EQUIP_NECKLACE_Level.Text = string.Empty;
            EQUIP_NECKLACE_SubProp1.Prop = "null";
            EQUIP_NECKLACE_SubName1.Text = string.Empty;
            EQUIP_NECKLACE_SubValue1.Text = string.Empty;
            EQUIP_NECKLACE_SubProp1.Prop = "null";
            EQUIP_NECKLACE_SubName2.Text = string.Empty;
            EQUIP_NECKLACE_SubValue2.Text = string.Empty;
            EQUIP_NECKLACE_SubProp3.Prop = "null";
            EQUIP_NECKLACE_SubName3.Text = string.Empty;
            EQUIP_NECKLACE_SubValue3.Text = string.Empty;
            EQUIP_NECKLACE_SubProp4.Prop = "null";
            EQUIP_NECKLACE_SubName4.Text = string.Empty;
            EQUIP_NECKLACE_SubValue4.Text = string.Empty;

            EQUIP_SHOES_Prop.Prop = "null";
            EQUIP_SHOES_Image.Source = null;
            EQUIP_SHOES_MainName.Text = string.Empty;
            EQUIP_SHOES_MainValue.Text = string.Empty;
            EQUIP_SHOES_Rating.Value = 0;
            EQUIP_SHOES_Rating.MaxRating = 0;
            EQUIP_SHOES_Rating.Visibility = Visibility.Collapsed;
            EQUIP_SHOES_Level.Text = string.Empty;
            EQUIP_SHOES_SubProp1.Prop = "null";
            EQUIP_SHOES_SubName1.Text = string.Empty;
            EQUIP_SHOES_SubValue1.Text = string.Empty;
            EQUIP_SHOES_SubProp1.Prop = "null";
            EQUIP_SHOES_SubName2.Text = string.Empty;
            EQUIP_SHOES_SubValue2.Text = string.Empty;
            EQUIP_SHOES_SubProp3.Prop = "null";
            EQUIP_SHOES_SubName3.Text = string.Empty;
            EQUIP_SHOES_SubValue3.Text = string.Empty;
            EQUIP_SHOES_SubProp4.Prop = "null";
            EQUIP_SHOES_SubName4.Text = string.Empty;
            EQUIP_SHOES_SubValue4.Text = string.Empty;

            EQUIP_RING_Prop.Prop = "null";
            EQUIP_RING_Image.Source = null;
            EQUIP_RING_MainName.Text = string.Empty;
            EQUIP_RING_MainValue.Text = string.Empty;
            EQUIP_RING_Rating.Value = 0;
            EQUIP_RING_Rating.MaxRating = 0;
            EQUIP_RING_Rating.Visibility = Visibility.Collapsed;
            EQUIP_RING_Level.Text = string.Empty;
            EQUIP_RING_SubProp1.Prop = "null";
            EQUIP_RING_SubName1.Text = string.Empty;
            EQUIP_RING_SubValue1.Text = string.Empty;
            EQUIP_RING_SubProp1.Prop = "null";
            EQUIP_RING_SubName2.Text = string.Empty;
            EQUIP_RING_SubValue2.Text = string.Empty;
            EQUIP_RING_SubProp3.Prop = "null";
            EQUIP_RING_SubName3.Text = string.Empty;
            EQUIP_RING_SubValue3.Text = string.Empty;
            EQUIP_RING_SubProp4.Prop = "null";
            EQUIP_RING_SubName4.Text = string.Empty;
            EQUIP_RING_SubValue4.Text = string.Empty;

            EQUIP_DRESS_Prop.Prop = "null";
            EQUIP_DRESS_Image.Source = null;
            EQUIP_DRESS_MainName.Text = string.Empty;
            EQUIP_DRESS_MainValue.Text = string.Empty;
            EQUIP_DRESS_Rating.Value = 0;
            EQUIP_DRESS_Rating.MaxRating = 0;
            EQUIP_DRESS_Rating.Visibility = Visibility.Collapsed;
            EQUIP_DRESS_Level.Text = string.Empty;
            EQUIP_DRESS_SubProp1.Prop = "null";
            EQUIP_DRESS_SubName1.Text = string.Empty;
            EQUIP_DRESS_SubValue1.Text = string.Empty;
            EQUIP_DRESS_SubProp1.Prop = "null";
            EQUIP_DRESS_SubName2.Text = string.Empty;
            EQUIP_DRESS_SubValue2.Text = string.Empty;
            EQUIP_DRESS_SubProp3.Prop = "null";
            EQUIP_DRESS_SubName3.Text = string.Empty;
            EQUIP_DRESS_SubValue3.Text = string.Empty;
            EQUIP_DRESS_SubProp4.Prop = "null";
            EQUIP_DRESS_SubName4.Text = string.Empty;
            EQUIP_DRESS_SubValue4.Text = string.Empty;

            FightHPBase.Text = "0";
            FightHPAppend.Text = "0";
            FightATKBase.Text = "0";
            FightATKAppend.Text = "0";
            FightDEFBase.Text = "0";
            FightDEFAppend.Text = "0";
            FightElementalMasteryValue.Text = "0";
            FightCRITRateValue.Text = "0";
            FightCRITDMGValue.Text = "0";
            FightEnergyRechargeValue.Text = "0";


            FightDMGBonusProp.Prop = "null";
            FightDMGBonusName.Text = "元素伤害加成";
            FightDMGBonusValue.Text = "0";
        }

        private void MyGrid_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            enkaView.Width = e.NewSize.Width;
            UI_Gacha_AvatarImg.Height = background.ActualHeight;
            UI_Gacha_AvatarImg.Width = background.ActualWidth;
        }

        private void scrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            MyGrid.Width = e.NewSize.Width;
        }
        
    }

    public sealed partial class BorderProp : UserControl
    {
        public static readonly DependencyProperty PropProperty =
            DependencyProperty.Register("Prop", typeof(string), typeof(BorderProp), new PropertyMetadata(null));

        public string Prop
        {
            get { return (string)GetValue(PropProperty); }
            set { SetValue(PropProperty, value); }
        }

        public BorderProp() { 
            this.PointerEntered += BorderProp_PointerEntered;
            this.PointerExited += BorderProp_PointerExited;
        }
        /*
         !!!
         这里我也不知道赋值为什么要反着来，正常应该是移出赋一个null，但会出bug
         !!!
         */
        private void BorderProp_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            GlobalProp.prop.NowProp = this.Prop;
        }

        private void BorderProp_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            // 在这里处理鼠标进入事件
            GlobalProp.prop.NowProp = "null";
        }
    }

    public class Prop
    {
        private string nowProp;
        public string NowProp
        {
            get { return nowProp; }
            set
            {
                if (value != nowProp)
                {
                    WhenNowPropChange();
                }
                nowProp = value;
            }
        }

        //定义的委托
        public delegate void NowPropChanged(object sender, EventArgs e);

        //与委托相关联的事件
        public event NowPropChanged OnNowPropChanged;

        //事件触发函数
        private void WhenNowPropChange()
        {
            if (OnNowPropChanged != null)
            {
                OnNowPropChanged(this, null);
            }
        }
    }
    public static class GlobalProp { 
        public static Prop prop = new();
    }
}
