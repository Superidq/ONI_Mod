//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
using TUNING;
using UnityEngine;
//using BepInEx;

namespace ReDesalinator
{
    /**
     * 本程序旨在制作盐水合成器，与脱盐器相反：    复制人运送：盐 + 液体输入管道：水 = 液体输出管道：盐水   
     * 动画使用脱盐器的，电力480瓦，发热量16千复制热每秒，参考现实中加速溶解需加热的原理
     * 从输入输出来说，跟“金属精炼器/脱盐器”类似，都是液体输入输出，但多了一个复制人运送盐的操作，这点又跟“乙醇蒸馏器”的运送木材类似
     * 因此多多借鉴这两个的代码
     */
    //[BepInPlugin("com.lu06001.plugin.RD", "ReDesalinatorConfig", "0.1.0")]
    public class ReDesalinatorConfig : IBuildingConfig
    {
        // 这个函数上次植物粉碎机用了，但脱盐器的代码没用，没玩过dlc，难道dlc没有脱盐器？猜的
        public override string[] GetDlcIds()
        {
            return DlcManager.AVAILABLE_ALL_VERSIONS;
        }
        public override BuildingDef CreateBuildingDef()
        {
            string id = "ReDesalinator";
            int width = 4;
            int height = 3;
            // 动画使用脱盐器的
            string anim = "desalinator_kanim";
            // 还是30
            int hitpoints = 30;
            //Debug.Log($"hitpoits:{hitpoints}");
            float construction_time = 10f;
            // 看起来是建造材料重量，不重要
            float[] tier = BUILDINGS.CONSTRUCTION_MASS_KG.TIER3;
            string[] raw_METALS = MATERIALS.RAW_METALS;
            float melting_point = 1600f;
            BuildLocationRule build_location_rule = BuildLocationRule.OnFloor;
            //Debug.Log($"建筑规则: {build_location_rule}");
            EffectorValues tier2 = NOISE_POLLUTION.NOISY.TIER1;

            
            BuildingDef buildingDef = BuildingTemplates.CreateBuildingDef(id, width, height, anim, hitpoints, 
                construction_time, tier, raw_METALS, melting_point, build_location_rule,
                BUILDINGS.DECOR.PENALTY.TIER0, tier2, 0.2f);
            //Debug.Log("完成建造");
            // TODO:注意！本条新增不过热！当然温度太高导致输入输出液体相变就不关我事了
            buildingDef.Overheatable = false;
            // 电量相关
            buildingDef.RequiresPowerInput = true;
            buildingDef.EnergyConsumptionWhenActive = 480f;
            buildingDef.SelfHeatKilowattsWhenActive = 16f;
            // TODO:这个不知道是什么，查一下
            buildingDef.ExhaustKilowattsWhenActive = 0f;
            // 下面照抄完事
            buildingDef.InputConduitType = ConduitType.Liquid;
            buildingDef.OutputConduitType = ConduitType.Liquid;
            buildingDef.Floodable = false;
            buildingDef.ViewMode = OverlayModes.LiquidConduits.ID;
            buildingDef.AudioCategory = "Metal";
            buildingDef.UtilityInputOffset = new CellOffset(-1, 0);
            buildingDef.UtilityOutputOffset = new CellOffset(0, 0);
            buildingDef.PermittedRotations = PermittedRotations.FlipH;
            return buildingDef;
        }

        public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag)
        {
            go.AddOrGet<LogicOperationalController>(); // 添加逻辑操作控制器
            // 虽然不知道是什么但大家好像都是false
            go.GetComponent<KPrefabID>().AddTag(RoomConstraints.ConstraintTags.IndustrialMachinery, false);
            Storage storage = go.AddOrGet<Storage>();
            storage.SetDefaultStoredItemModifiers(Storage.StandardSealedStorage);
            storage.showInUI = true;
            storage.capacityKg = 1000f;
            // 人工运送配置
            ManualDeliveryKG manualDeliveryKG = go.AddOrGet<ManualDeliveryKG>();
            manualDeliveryKG.SetStorage(storage);
            manualDeliveryKG.RequestedItemTag = TAG1_mod_lu06001;
            manualDeliveryKG.choreTypeIDHash = Db.Get().ChoreTypes.MachineFetch.IdHash;
            // 一次送满到600 KG盐
            manualDeliveryKG.capacity = 600f;
            // 根据游戏经验，到这个界限的时候小人就会过来补充 ↓
            manualDeliveryKG.refillMass = 150f;
            // 输入输出元素相关
            ElementConverter elementConverter = go.AddComponent<ElementConverter>();
            // 4650g水 + 350g 盐
            elementConverter.consumedElements = new ElementConverter.ConsumedElement[]
            {
                new ElementConverter.ConsumedElement(TAG1_mod_lu06001, 0.35f, true),
                new ElementConverter.ConsumedElement(TAG2_mod_lu06001, 4.65f, true)
            };
            // 5000g 盐水，这里有几个参数关于outputElementOffset的不太懂
            elementConverter.outputElements = new ElementConverter.OutputElement[]
            {
                new ElementConverter.OutputElement(5f, SimHashes.SaltWater, 0f,
                    false, true, 0f, 0.5f,
                    0.75f, byte.MaxValue, 0, true)
            };
            ConduitConsumer conduitConsumer = go.AddOrGet<ConduitConsumer>();
            conduitConsumer.conduitType = ConduitType.Liquid;
            conduitConsumer.consumptionRate = 46.5f;
            conduitConsumer.capacityKG = 20f;
            conduitConsumer.capacityTag = GameTags.AnyWater;
            conduitConsumer.forceAlwaysSatisfied = true;
            // 加点东西，这点东西很重要！是用来消耗材料的，这里是套用藻类蒸馏器模型消耗固体，那个乙醇蒸馏器也是这么写的
            AlgaeDistillery algaeDistillery = go.AddOrGet<AlgaeDistillery>();
            algaeDistillery.emitMass = 3.5f;
            algaeDistillery.emitTag = null;
            // 不对的液体直接排出去，又改成存了
            conduitConsumer.wrongElementResult = ConduitConsumer.WrongElementResult.Store;
            // 有条疑似什么元素转化过滤的代码跳过了，脱盐器、乙醇蒸馏器、金属精炼器三个里面就脱盐器有这个
            ConduitDispenser conduitDispenser = go.AddOrGet<ConduitDispenser>();
            conduitDispenser.conduitType = ConduitType.Liquid;
            conduitDispenser.elementFilter = new SimHashes[]
            { 
                SimHashes.SaltWater
            };
            Prioritizable.AddRef(go);

        }

        // 不懂，照抄
        public override void DoPostConfigurePreview(BuildingDef def, GameObject go)
        {
            base.DoPostConfigurePreview(def, go);
        }
        // 不懂，照抄
        public override void DoPostConfigureUnderConstruction(GameObject go)
        {
            base.DoPostConfigureUnderConstruction(go);
        }
        // 不懂，照抄
        public override void DoPostConfigureComplete(GameObject go)
        {
            go.AddOrGetDef<PoweredActiveController.Def>().showWorkingStatus = true;
        }

        public const string ID = "ReDesalinator";

        public static readonly Tag TAG1_mod_lu06001 = SimHashes.Salt.CreateTag();
        public static readonly Tag TAG2_mod_lu06001 = SimHashes.Water.CreateTag();
    }
}
