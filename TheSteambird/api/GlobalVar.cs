using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheSteambird.api
{
    public static class GlobalVar
    {   
        public static IntPtr Hwnd { get; set; }
        public static readonly string SettingsFile = System.AppDomain.CurrentDomain.BaseDirectory + "data\\settings.json";

        public static readonly string AccountSqlPath = System.AppDomain.CurrentDomain.BaseDirectory + "data\\account.db";
        public static readonly string GenshinAbyssSqlPath = System.AppDomain.CurrentDomain.BaseDirectory + "data\\genshinAbyss.db";
        public static readonly string GenshinGachaSqlPath = System.AppDomain.CurrentDomain.BaseDirectory + "data\\genshinGacha.db";

        public static readonly string GenshinAccountSqlTable = "genshin";
        public static readonly string GenshinAccountSqlRow = "Uid, Name, Server, Platform, Id, Cookies";
        public static readonly string GenshinAccountSqlAddRow = "Uid INT, Name TEXT, Server TEXT, Platform INT, Id INT, Cookies TEXT";

        public static GenshinOverViewGetApi GenshinOverviewGet = new GenshinOverViewGetApi();

        public static readonly string GenshinAbyssSqlRow = "Id, Data";
        public static readonly string GenshinAbyssSqlAddRow = "Id INT, Data TEXT";

        public static readonly string GenshinGachaSqlRow = "Id, Gacha_type, Time, Name, Item_type, Rank_type";
        public static readonly string GenshinGachaSqlAddRow = "Id TEXT, Gacha_type INT, Time TEXT, Name TEXT, Item_type TEXT, Rank_type INT";

        public static readonly string GenshinGachaHtml = System.AppDomain.CurrentDomain.BaseDirectory + "echart\\genshinGacha.html";

        public static readonly string GenshinEnkaNetworkSqlPath = System.AppDomain.CurrentDomain.BaseDirectory + "data\\genshinEnkaNetwork.db";
        public static readonly string GenshinEnkaNetworkSqlInfoRow = "Uid, Nickname, Level, Signature, WorldLevel, NameCardId, ProfilePicture";
        public static readonly string GenshinEnkaNetworkSqlInfoAddRow = "Uid INT, Nickname TEXT, Level INT, Signature TEXT, WorldLevel INT, NameCardId INT, ProfilePicture INT";
        public static readonly string GenshinEnkaNetworkSqlDataRow = "AvatarID, TalentIdList, PropMap, FightPropMap, SkillDepotId, InherentProudSkillList, SkillLevelMap, EquipList, ExpLevel";
        public static readonly string GenshinEnkaNetworkSqlDataAddRow = "AvatarID INT, TalentIdList TEXT,PropMap TEXT, FightPropMap TEXT, SkillDepotId INT, InherentProudSkillList TEXT, SkillLevelMap TEXT, EquipList TEXT, ExpLevel INT";
    
        public static readonly string GenshinEnkaNetworkDataPath = System.AppDomain.CurrentDomain.BaseDirectory + "Genshin\\EnkaNetwork";
    }
}
