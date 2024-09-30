//using System;
using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
// 添加了原植物粉碎机的引用
using STRINGS;
using TUNING;
using UnityEngine;
//using HarmonyLib;
//using BUILDINGS = STRINGS.BUILDINGS;

/*******
 * 你好！这个mod是我第一次学习C#以及制作.dll的作品，请多多指教
 * 本mod计划修改本体的植物粉碎机为不需要复制人操作，改为耗电，<-但这是之前的想法，现在想法变了，我要写一个新的建筑
 * 本mod计划增加一个新建筑“豆浆机”以避免与其他mod冲突，贴图与植物粉碎机相同但它耗电且耗电量为240W，且无需复制人捶打
*******/
namespace ModifyPlantPulverizer
{
    public class SoybeanMilkMachineConfig : IBuildingConfig  // 似乎是一个Interface，接口
    {
        public override string[] GetDlcIds()
        {
            return DlcManager.AVAILABLE_ALL_VERSIONS;
        }

        // 创建建筑定义的方法，这整个类都是首先复制的植物粉碎机Config，之后参考“添加新建筑”的
        public override BuildingDef CreateBuildingDef()
        {
            string id = "SoybeanMilkMachine";
            int width = 2;
            int height = 3;
            string anim = "milkpress_kanim";    // 贴图采用原建筑的
            int hitpoints = TUNING.BUILDINGS.HITPOINTS.TIER2; // 查了下竟然是血量
            float construction_time = 30f;  // 建造时间
            float[] tier = TUNING.BUILDINGS.CONSTRUCTION_MASS_KG.TIER4; // 不知道是什么
            string[] all_MINERALS = MATERIALS.ALL_MINERALS; // 同上
            float melting_point = 1600f; // 显然是熔点
            BuildLocationRule build_location_rule = BuildLocationRule.OnFloor; // 还是不知道
            EffectorValues tier2 = NOISE_POLLUTION.NOISY.TIER4; // 好像是装饰度，好像又不是

            // 创建建筑定义
            BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(id, width, height, anim, hitpoints, construction_time, tier, all_MINERALS, melting_point, build_location_rule, TUNING.BUILDINGS.DECOR.PENALTY.TIER1, tier2, 0.2f);
            buildingDef.RequiresPowerInput = true; // 电力输入！改了
            buildingDef.PowerInputOffset = new CellOffset(1, 0); // 本条是新增的
            buildingDef.EnergyConsumptionWhenActive = 240f; // 应该就是耗电瓦数，改了
            buildingDef.SelfHeatKilowattsWhenActive = 2f; // 这是自热吧，我就不改了
            buildingDef.OutputConduitType = ConduitType.Liquid; // 输出是液体
            buildingDef.UtilityOutputOffset = new CellOffset(1, 0); //这是什么？
            buildingDef.LogicInputPorts = LogicOperationalController.CreateSingleInputPortList(new CellOffset(1, 0)); // 似乎是逻辑输入口
            buildingDef.ViewMode = OverlayModes.LiquidConduits.ID;
            buildingDef.AudioCategory = "HollowMetal";
            buildingDef.AudioSize = "Metal";
            return buildingDef;
        }

        // 配置建筑模板的方法
        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.AddOrGet<LogicOperationalController>(); // 添加逻辑操作控制器
            go.AddOrGet<DropAllWorkable>(); // 不懂
            go.AddOrGet<BuildingComplete>().isManuallyOperated = false; // 是否让小人过来操作，我猜，又猜是玩家选择配方的操作，改了
            ComplexFabricator complexFabricator = go.AddOrGet<ComplexFabricator>(); // 不懂
            complexFabricator.sideScreenStyle = ComplexFabricatorSideScreen.StyleSetting.ListQueueHybrid; // 什么什么列表队列混合物，是在说配方吗
            complexFabricator.duplicantOperated = false; // TODO: 怎么又有一个复制人操作的选项？猜是一个用来操作机器一个用来运送？待验证。mod加载成功之后觉得这两个大概一个是机器使用层面的需操作，一个是建筑详情文本层面的需操作
            go.AddOrGet<FabricatorIngredientStatusManager>(); // 翻译过来是制造商身份状态管理器。。也许是需要技能？
            go.AddOrGet<CopyBuildingSettings>();
            ComplexFabricatorWorkable complexFabricatorWorkable = go.AddOrGet<ComplexFabricatorWorkable>(); // 机翻：复杂的可加工结构
            BuildingTemplates.CreateComplexFabricatorStorage(go, complexFabricator);
            complexFabricatorWorkable.overrideAnims = new KAnimFile[]
            {
            Assets.GetAnim("anim_interacts_milkpress_kanim") // 看起来是小人击打动画了，也许这说明前面这些都是小人过来操作的代码，注释掉算了，先不注释看看能否运行，不能的话参考别的无需操作的代码改改
            };
            complexFabricatorWorkable.workingPstComplete = new HashedString[]
            {
            "working_pst_complete"
            };
            complexFabricator.storeProduced = true;
            complexFabricator.inStorage.SetDefaultStoredItemModifiers(SoybeanMilkMachineConfig.RefineryStoredItemModifiers);
            complexFabricator.outStorage.SetDefaultStoredItemModifiers(SoybeanMilkMachineConfig.RefineryStoredItemModifiers);
            ConduitDispenser conduitDispenser = go.AddOrGet<ConduitDispenser>(); // 下面这些都是说液体管道的，大概
            conduitDispenser.conduitType = ConduitType.Liquid;
            conduitDispenser.alwaysDispense = true;
            conduitDispenser.elementFilter = null;
            conduitDispenser.storage = go.GetComponent<ComplexFabricator>().outStorage;
            this.AddRecipes(go);
            Prioritizable.AddRef(go);
        }

        private void AddRecipes(GameObject go)
        {
            ComplexRecipe.RecipeElement[] array = new ComplexRecipe.RecipeElement[] // 这是配方，大概不用改
            {
            new ComplexRecipe.RecipeElement("ColdWheatSeed", 10f), // 嗯，冰霜小麦+水，熟悉的味道
            new ComplexRecipe.RecipeElement(SimHashes.Water.CreateTag(), 15f)
            };
            ComplexRecipe.RecipeElement[] array2 = new ComplexRecipe.RecipeElement[]
            {
            new ComplexRecipe.RecipeElement(SimHashes.Milk.CreateTag(), 20f, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature, false)
            };
            ComplexRecipe complexRecipe = new ComplexRecipe(ComplexRecipeManager.MakeRecipeID("SoybeanMilkMachine", array, array2), array, array2, 0, 0);
            complexRecipe.time = 20f; // TODO: 加工时间，我觉得这个可以动手脚，改不改呢？留给玩家好了，先改成了20s
            complexRecipe.description = string.Format(STRINGS.BUILDINGS.PREFABS.MILKPRESS.WHEAT_MILK_RECIPE_DESCRIPTION, ITEMS.FOOD.COLDWHEATSEED.NAME, SimHashes.Milk.CreateTag().ProperName());
            complexRecipe.nameDisplay = ComplexRecipe.RecipeNameDisplay.IngredientToResult;
            complexRecipe.fabricators = new List<Tag>
        {
            TagManager.Create("SoybeanMilkMachine")
        };
            ComplexRecipe.RecipeElement[] array3 = new ComplexRecipe.RecipeElement[]
            {
            new ComplexRecipe.RecipeElement(SpiceNutConfig.ID, 3f),
            new ComplexRecipe.RecipeElement(SimHashes.Water.CreateTag(), 17f)
            };
            ComplexRecipe.RecipeElement[] array4 = new ComplexRecipe.RecipeElement[]
            {
            new ComplexRecipe.RecipeElement(SimHashes.Milk.CreateTag(), 20f, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature, false)
            };
            ComplexRecipe complexRecipe2 = new ComplexRecipe(ComplexRecipeManager.MakeRecipeID("SoybeanMilkMachine", array3, array4), array3, array4, 0, 0);
            complexRecipe2.time = 20f; // TODO: 加工时间，我觉得这个可以动手脚，改不改呢？留给玩家好了，先改成了20s
            complexRecipe2.description = string.Format(STRINGS.BUILDINGS.PREFABS.MILKPRESS.NUT_MILK_RECIPE_DESCRIPTION, ITEMS.FOOD.SPICENUT.NAME, SimHashes.Milk.CreateTag().ProperName());
            complexRecipe2.nameDisplay = ComplexRecipe.RecipeNameDisplay.IngredientToResult;
            complexRecipe2.fabricators = new List<Tag>
        {
            TagManager.Create("SoybeanMilkMachine")
        };
            ComplexRecipe.RecipeElement[] array5 = new ComplexRecipe.RecipeElement[]
            {
            new ComplexRecipe.RecipeElement("BeanPlantSeed", 2f),
            new ComplexRecipe.RecipeElement(SimHashes.Water.CreateTag(), 18f)
            };
            ComplexRecipe.RecipeElement[] array6 = new ComplexRecipe.RecipeElement[]
            {
            new ComplexRecipe.RecipeElement(SimHashes.Milk.CreateTag(), 20f, ComplexRecipe.RecipeElement.TemperatureOperation.AverageTemperature, false)
            };
            ComplexRecipe complexRecipe3 = new ComplexRecipe(ComplexRecipeManager.MakeRecipeID("SoybeanMilkMachine", array5, array6), array5, array6, 0, 0);
            complexRecipe3.time = 20f; // TODO: 加工时间，我觉得这个可以动手脚，改不改呢？留给玩家好了，先改成了20s
            complexRecipe3.description = string.Format(STRINGS.BUILDINGS.PREFABS.MILKPRESS.NUT_MILK_RECIPE_DESCRIPTION, ITEMS.FOOD.BEANPLANTSEED.NAME, SimHashes.Milk.CreateTag().ProperName());
            complexRecipe3.nameDisplay = ComplexRecipe.RecipeNameDisplay.IngredientToResult;
            complexRecipe3.fabricators = new List<Tag>
        {
            TagManager.Create("SoybeanMilkMachine")
        };
        }

        // 解释成完成后的配置方法，或操作之后的提升好一些
        public override void DoPostConfigureComplete(GameObject go)
        {
            SymbolOverrideControllerUtil.AddToPrefab(go);
            go.GetComponent<KPrefabID>().prefabSpawnFn += delegate (GameObject game_object)
            {
                ComplexFabricatorWorkable component = game_object.GetComponent<ComplexFabricatorWorkable>();
                component.WorkerStatusItem = Db.Get().DuplicantStatusItems.Processing;
                component.AttributeConverter = Db.Get().AttributeConverters.MachinerySpeed;
                component.AttributeExperienceMultiplier = DUPLICANTSTATS.ATTRIBUTE_LEVELING.PART_DAY_EXPERIENCE;
                component.SkillExperienceSkillGroup = Db.Get().SkillGroups.Technicals.Id;
                component.SkillExperienceMultiplier = SKILLS.PART_DAY_EXPERIENCE;
            };
        }

        // Token: 0x0400087A RID: 2170
        public const string ID = "SoybeanMilkMachine";

        // Token: 0x0400087B RID: 2171
        private static readonly List<Storage.StoredItemModifier> RefineryStoredItemModifiers = new List<Storage.StoredItemModifier>
    {
        Storage.StoredItemModifier.Hide,
        Storage.StoredItemModifier.Preserve,
        Storage.StoredItemModifier.Insulate,
        Storage.StoredItemModifier.Seal
    };
    }

}