using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.WebUI;

namespace TheSteambird.api
{
    public struct GenshinAccountData
    {
        public GenshinAccountData(string name, string server, int platform, int id, int uid, string cookies)
        {
            Name = name;
            Server = server;
            Platform = platform;
            Id = id;
            Uid = uid;
            Cookies = cookies;
        }
        public string Name { get; }         //游戏名
        public string Server { get; }     //服务器
        public int Platform { get; }    //平台 0、米游社 1、HoyoLab
        public int Id { get; }                //平台id
        public int Uid { get; }              //游戏uid
        public string Cookies { get; }   //账户cookie
    }
    public struct GenshinGachaData
    {
        public GenshinGachaData(string id, int gacha_type, int rank_type, string name, string date, string item_type)
        {
            Id = id;
            Gacha_type = gacha_type;
            Rank_type = rank_type;
            Name = name;
            Date = date;
            Item_type = item_type;
        }
        public GenshinGachaData(string id, int gacha_type, int rank_type, string name, string date, string item_type, string uid)
        {
            Id = id;
            Gacha_type = gacha_type;
            Rank_type = rank_type;
            Name = name;
            Date = date;
            Item_type = item_type;
            Uid = uid;
        }
        public string Id { get; set; }
        public int Gacha_type { get; set; }
        public int Rank_type { get; set; }
        public string Name { get; set; }
        public string Date { get; set; }
        public string Item_type { get; set; }
        public string Uid { get; set; }
    }
    public struct GenshinEnkaNetworkAvatarData {
        public GenshinEnkaNetworkAvatarData(int avatarID, List<int> talentIdList, Dictionary<string, string> propMap, Dictionary<string, double> fightProp, int skillDepotId, List<int> inherentProudSkillList, Dictionary<string, int> skillLevelMap, GenshinEnkaNetworkWeapon weapon, List<GenshinEnkaNetworkReliquary> reliquaryList, int expLevel)
        {
            this.avatarID = avatarID;
            this.talentIdList = talentIdList;
            this.propMap = propMap;
            this.fightProp = fightProp;
            this.skillDepotId = skillDepotId;
            this.inherentProudSkillList = inherentProudSkillList;
            this.skillLevelMap = skillLevelMap;
            this.weapon = weapon;
            this.reliquaryList = reliquaryList;
            this.expLevel = expLevel;
        }

        public int avatarID { get; set; }
        public List<int> talentIdList { get; set; }
        public Dictionary<string, string> propMap { get; set; }
        public Dictionary<string, double> fightProp { get; set; }
        public int skillDepotId { get; set; }
        public List<int> inherentProudSkillList { get; set; }
        public Dictionary <string, int> skillLevelMap { get; set; }
        public GenshinEnkaNetworkWeapon weapon { get; set; }
        public List<GenshinEnkaNetworkReliquary> reliquaryList { get; set; }
        public int expLevel { get; set; }


    }
    public struct GenshinEnkaNetworkWeapon{
        public GenshinEnkaNetworkWeapon(int itemId, int level, int promoteLevel, int affixMap, GenshinEnkaNetworkWeaponFlat flat)
        {
            this.itemId = itemId;
            this.level = level;
            this.promoteLevel = promoteLevel;
            this.affixMap = affixMap;
            this.flat = flat;
        }

        public int itemId { get; set; }
        public int level { get; set; }
        public int promoteLevel { get; set; }   //武器突破等级
        public int affixMap { get; set; }       //武器精炼等级 [0-4]
        public GenshinEnkaNetworkWeaponFlat flat { get; set; }
    }
    public struct GenshinEnkaNetworkReliquary
    {
        public GenshinEnkaNetworkReliquary(int itemId, int level, int mainPropId, List<int> appendPropIdList, GenshinEnkaNetworkReliquaryFlat flat)
        {
            this.itemId = itemId;
            this.level = level;
            this.mainPropId = mainPropId;
            this.appendPropIdList = appendPropIdList;
            this.flat = flat;
        }

        public int itemId { get; set; }
        public int level { get; set; }                      //圣遗物等级 [1-21]
        public int mainPropId { get; set; }                 //圣遗物主属性
        public List<int> appendPropIdList { get; set; }     //圣遗物副属性 ID 列表
        public GenshinEnkaNetworkReliquaryFlat flat { get; set; }
    }
    public struct GenshinEnkaNetworkWeaponFlat
    {
        public GenshinEnkaNetworkWeaponFlat(string nameTextHashMap, int rankLevel, Dictionary<string, double> weaponStats, string itemType, string icon)
        {
            this.nameTextHashMap = nameTextHashMap;
            this.rankLevel = rankLevel;
            this.weaponStats = weaponStats;
            this.itemType = itemType;
            this.icon = icon;
        }

        public string nameTextHashMap { get; set; }
        public int rankLevel { get; set; }
        public Dictionary<string, double> weaponStats { get; set; }
        public string itemType { get; set; }
        public string icon { get; set; }
    }
    public struct GenshinEnkaNetworkReliquaryFlat
    {
        public GenshinEnkaNetworkReliquaryFlat(string nameTextHashMap, string setNameTextHashMap, int rankLevel, Dictionary<string, double> reliquaryMainstat, Dictionary<string, double> reliquarySubstats, string itemType, string icon, string equipType)
        {
            this.nameTextHashMap = nameTextHashMap;
            this.setNameTextHashMap = setNameTextHashMap;
            this.rankLevel = rankLevel;
            this.reliquaryMainstat = reliquaryMainstat;
            this.reliquarySubstats = reliquarySubstats;
            this.itemType = itemType;
            this.icon = icon;
            this.equipType = equipType;
        }

        public string nameTextHashMap { get; set; }
        public string setNameTextHashMap { get; set; }                  //圣遗物套装的名称的哈希值
        public int rankLevel { get; set; }
        public Dictionary<string, double> reliquaryMainstat { get; set; }  //圣遗物主属性
        public Dictionary<string, double> reliquarySubstats { get; set; }  //圣遗物副属性列表
        public string itemType { get; set; }
        public string icon { get; set; }
        public string equipType { get; set; }                           //圣遗物部位
    }
}
