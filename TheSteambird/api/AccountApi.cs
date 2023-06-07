using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace TheSteambird.api
{
    //用于给xaml提供数据
    public class GenshinAccount
    {
        public string Name { get; set; }
        public string Uid { get; set; }

        public GenshinAccount(string name, string uid)
        {
            this.Name = name;
            this.Uid = uid;
        }
    }
    public class GenshinOverviewData
    {
        public string Name { get; set; }
        public string Uid { get; set; }
        //日常
        //体力
        public string Current_resin { get; set; }                   //体力
        public string Max_resin { get; set; }                       //最大体力
        public string Resin_recovery_time { get; set; }             //回复时间
        //洞天宝钱
        public string Current_home_coin { get; set; }               //洞天宝钱
        public string Max_home_coin { get; set; }                   //最大洞天宝钱
        public string Home_coin_recovery_time { get; set; }         //回复时间
        //参量质变仪
        public string Extra_task_reward_received { get; set; }      //参量完成个数
        public string All_task_reward { get; set; }                 //参量总个数，即1
        public string Is_extra_task_reward_received { get; set; }   //参量是否完成
        //每日委托
        public string Finished_task_num { get; set; }               //委托完成数量
        public string Total_task_num { get; set; }                  //总委托
        public string Is_task { get; set; }                         //委托是否完成
        //探索派遣
        public string Current_expedition_num { get; set; }          //探索派遣完成数量
        public string Max_expedition_num { get; set; }              //总派遣
        public string Is_expedition { get; set; }                   //探索派遣是否完成

        //生涯数据部分
        public string Active_day_number { get; set; }               //活跃天数
        public string Achievement_number { get; set; }              //成就达成数
        public string Avatar_number { get; set; }                   //获得角色数
        public string Way_point_number { get; set; }                //解锁传送点
        public string Anemoculus_number { get; set; }               //风神瞳
        public string Geoculus_number { get; set; }                 //岩神瞳
        public string Electroculus_number { get; set; }             //雷神瞳
        public string Dendroculus_number { get; set; }              //草神瞳
        public string Domain_number { get; set; }                   //解锁秘境
        public string Spiral_abyss { get; set; }                    //深境螺旋
        public string Luxurious_chest_number { get; set; }          //华丽宝箱数
        public string Precious_chest_number { get; set; }           //珍贵宝箱数
        public string Exquisite_chest_number { get; set; }          //精致宝箱数
        public string Common_chest_number { get; set; }             //普通宝箱数
        public string Magic_chest_number { get; set; }              //奇馈宝箱数

        //探索度
        //蒙德
        public string Mengde_exploration { get; set; }              //探索度
        public string Mengde_level { get; set; }                    //声望
        //龙脊雪山
        public string Dragonspine_exploration { get; set; }         //探索度
        public string Dragonspine_Eldertree { get; set; }           //忍冬之树
        //璃月
        public string Liyue_exploration { get; set; }               //探索度
        public string Liyue_level { get; set; }                     //声望
        //层岩巨渊
        public string Cengyan_exploration { get; set; }             //探索度
        public string Cengyan_Lumenstone { get; set; }              //流明石触媒
        //层岩巨渊·地下矿区
        public string Cengyan_under_exploration { get; set; }       //探索度
        public string Cengyan_under_Lumenstone { get; set; }        //流明石触媒
        //稻妻
        public string Daoqi_exploration { get; set; }               //探索度
        public string Daoqi_level { get; set; }                     //声望
        public string Daoqi_Oraionokami { get; set; }               //神樱眷顾
        //渊下宫
        public string Enkanomiya_exploration { get; set; }          //探索度
        //须弥
        public string Xumi_exploration { get; set; }                //探索度
        public string Xumi_level { get; set; }                      //声望
        public string Xumi_DreamTree { get; set; }                  //梦之树
        public GenshinOverviewData(string name, string uid, List<List<string>> daily, List<string> detail, List<List<string>> exploration)
        {
            this.Name = name;
            this.Uid = uid;
            //日常
            List<string> FragileResin = daily[0];
            this.Current_resin = FragileResin[0];
            this.Max_resin = FragileResin[1];
            this.Resin_recovery_time = FragileResin[2];
            List<string> SereniteaPot = daily[1];
            this.Current_home_coin = SereniteaPot[0];
            this.Max_home_coin = SereniteaPot[1];
            this.Home_coin_recovery_time = SereniteaPot[2];
            List<string> ParametricTransformer = daily[2];
            this.Extra_task_reward_received = ParametricTransformer[0];
            this.All_task_reward = ParametricTransformer[1];
            this.Is_extra_task_reward_received = ParametricTransformer[2];
            List<string> Commission = daily[3];
            this.Finished_task_num = Commission[0];
            this.Total_task_num = Commission[1];
            this.Is_task = Commission[2];
            List<string> Domains = daily[4];
            this.Current_expedition_num = Domains[0];
            this.Max_expedition_num = Domains[1];
            this.Is_expedition = Domains[2];
            //数据部分
            this.Active_day_number = detail[0];
            this.Achievement_number = detail[1];
            this.Avatar_number = detail[2];
            this.Way_point_number = detail[3];
            this.Anemoculus_number = detail[4];
            this.Geoculus_number = detail[5];
            this.Electroculus_number = detail[6];
            this.Dendroculus_number = detail[7];
            this.Domain_number = detail[8];
            this.Spiral_abyss = detail[9];
            this.Luxurious_chest_number = detail[10];
            this.Precious_chest_number = detail[11];
            this.Exquisite_chest_number = detail[12];
            this.Common_chest_number = detail[13];
            this.Magic_chest_number = detail[14];
            //探索度
            List<string> mengde = exploration[0];
            this.Mengde_exploration = mengde[0];
            this.Mengde_level = mengde[1];
            List<string> dragonspine = exploration[1];
            this.Dragonspine_exploration = dragonspine[0];
            this.Dragonspine_Eldertree = dragonspine[1];
            List<string> Liyue = exploration[2];
            this.Liyue_exploration = Liyue[0];
            this.Liyue_level = Liyue[1];
            List<string> cengyan = exploration[3];
            this.Cengyan_exploration = cengyan[0];
            this.Cengyan_Lumenstone = cengyan[1];
            List<string> cengyan_under = exploration[4];
            this.Cengyan_under_exploration = cengyan_under[0];
            this.Cengyan_under_Lumenstone = cengyan_under[1];
            List<string> daoyi = exploration[5];
            this.Daoqi_exploration = daoyi[0];
            this.Daoqi_level = daoyi[1];
            this.Daoqi_Oraionokami = daoyi[2];
            List<string> enkanomiya = exploration[6];
            this.Enkanomiya_exploration = enkanomiya[0];
            List<string> xumi = exploration[7];
            this.Xumi_exploration = xumi[0];
            this.Xumi_level = xumi[1];
            this.Xumi_DreamTree = xumi[2];
        }
    }
    public class GenshinAbyssDate
    {
        public GenshinAbyssDate(string count) {
            this.Count = count;
        }
        public string Count { get; set; }
    }
    public class GenshinAbyssRevealRanks{
        public GenshinAbyssRevealRanks(string picRes, string count)
        {
            this.PicRes = picRes;
            this.Count = count;
        }
        public string PicRes { get; set; }
        public string Count { get; set;}
    }
    public class GenshinAbyssFloors {
        public GenshinAbyssFloors(string index, string star, ObservableCollection<GenshinAbyssLevels> level)
        {
            Index = index;
            Star = star;
            Level = level;
        }
        public string Index { get; set; }   //第几层
        public string Star { get; set; }
        public ObservableCollection<GenshinAbyssLevels> Level { get; set; }

    }
    public class GenshinAbyssLevels {
        public GenshinAbyssLevels(string index, string date, string star, ObservableCollection<GenshinAbyssBattles> battle1, ObservableCollection<GenshinAbyssBattles> battle2)
        {
            Index = index;
            Date = date;
            Star = star;
            Battle1 = battle1;
            Battle2 = battle2;
        }
        public string Index { get; set; }   //第几间
        public string Date { get; set; }
        public string Star { get; set; }
        public ObservableCollection<GenshinAbyssBattles> Battle1 { get; set; }  //上半
        public ObservableCollection<GenshinAbyssBattles> Battle2 { get; set; }  //下半
    }
    public class GenshinAbyssBattles { 
        public GenshinAbyssBattles(string pic, string level)
        {
            Pic = pic;
            Level = level;
        }
        public string Pic { get; set; }
        public string Level { get; set; }
    }
    public class GenshinEnkaNetworkIcon { 
        public string Icon { get; set; }
        public int Id { get; set; }
        public GenshinEnkaNetworkIcon(string icon, int id)
        {
            Icon = icon;
            Id = id;
        }
    }
    
}
