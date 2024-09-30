using HarmonyLib;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

namespace ReDesalinator
{
    // 以下参考（照抄）了路径“QQ\向游戏中添加新建筑\new_building”中的轮子
    public static class StringUtils
    {
        public static void Add_New_BuildStrings(string plantId, string name, string description, string effect)
        {
            Strings.Add(new string[]
            {
                // name
                "STRINGS.BUILDINGS.PREFABS." + plantId.ToUpperInvariant() + ".NAME",
                name
            });
            Strings.Add(new string[]
            {
                // 制作描述
                "STRINGS.BUILDINGS.PREFABS." + plantId.ToUpperInvariant() + ".DESC",
                description
            });
            Strings.Add(new string[]
            {
                //扩展描述
                "STRINGS.BUILDINGS.PREFABS." + plantId.ToUpperInvariant() + ".EFFECT",
                effect
            });
        }
    }

    public static class BuildPatch
    {
        [HarmonyPatch(typeof(GeneratedBuildings), "LoadGeneratedBuildings")]
        public static class ReDesalinator_1LoadGeneratedBuildings_Patch
        {
            [HarmonyPrefix]
            public static void Prefix()
            {
                // 下面两行主要是由  BuildMenu : KScreen 管理的
                // 将建筑添加到菜单
                ModUtil.AddBuildingToPlanScreen("Refining", ReDesalinatorConfig.ID);
                Db.Get().Techs.Get("LiquidFiltering").unlockedItemIDs.Add(ReDesalinatorConfig.ID);
                // 调用上面的轮子，将字符串添加到游戏中
                StringUtils.Add_New_BuildStrings(ReDesalinatorConfig.ID, BUILDING_MOD.NAME, BUILDING_MOD.DESC, BUILDING_MOD.EFFECT);
            }
        }
    }
    public class BUILDING_MOD
    {
        //文本
        public static LocString NAME = "盐水合成器";
        public static LocString DESC = "玩到会写mod之前从未用过脱盐器，萌新之怒，后面忘了";
        public static LocString EFFECT = "将盐加入水中进行溶解，产生盐水";
    }
}
