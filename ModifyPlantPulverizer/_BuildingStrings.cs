using HarmonyLib;
//using Klei.AI;
//using STRINGS;
//using static ResearchTypes;

namespace ModifyPlantPulverizer
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
        public static class PlantPulverizer_1LoadGeneratedBuildings_Patch
        {
            public static void Prefix()
            {
                // 下面两行主要是由  BuildMenu : KScreen 管理的
                // 将建筑添加到菜单
                ModUtil.AddBuildingToPlanScreen("Refining", SoybeanMilkMachineConfig.ID);
                // TODO: 使其可研究，这个后面看看要不要改回去
                Db.Get().Techs.Get("FoodRepurposing").unlockedItemIDs.Add(SoybeanMilkMachineConfig.ID);
                // 调用上面的轮子，将字符串添加到游戏中
                StringUtils.Add_New_BuildStrings(SoybeanMilkMachineConfig.ID, BUILDING_MOD.NAME, BUILDING_MOD.DESC, BUILDING_MOD.EFFECT);
            }
        }
    }
    public class BUILDING_MOD
    {
        //文本
        public static LocString NAME = "酒羊豆浆机";
        public static LocString DESC = "不过是一台每家每户都会用到的豆浆机，比人工稍快那么一点点而已";
        public static LocString EFFECT = STRINGS.BUILDINGS.PREFABS.MILKPRESS.EFFECT;
    }
}