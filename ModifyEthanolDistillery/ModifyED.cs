using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;

namespace ModifyEthanolDistillery
{
    public class ModifyED
    {
        [HarmonyPatch(typeof(EthanolDistilleryConfig), "ConfigureBuildingTemplate")]
        public class Patch
        {
            [HarmonyPostfix]
            public static void Postfix(GameObject go, Tag prefab_tag)
            {
                Storage storage = go.AddOrGet<Storage>();
                // 增加乙醇蒸馏器自身存储量（疑似），1000->2000
                storage.capacityKg = 2000f;
                ManualDeliveryKG manualDeliveryKG = go.AddOrGet<ManualDeliveryKG>();
                // 增加乙醇蒸馏器相关存储量，这里应该是复制人给它运送木材的量， 600->1200
                manualDeliveryKG.capacity = 1200f;
                ElementConverter elementConverter = go.AddOrGet<ElementConverter>();
                elementConverter.outputElements = new ElementConverter.OutputElement[]
                {
                    new ElementConverter.OutputElement(0.5f, SimHashes.Ethanol, 
                        346.5f, false, 
                        true, 0f, 0.5f, 
                        1f, byte.MaxValue, 0, true),
                    // 输出污染土 0.33333334f->0.66666667f
                    new ElementConverter.OutputElement(0.66666667f, SimHashes.ToxicSand, 
                        366.5f, false, 
                        true, 0f, 0.5f, 
                        1f, byte.MaxValue, 0, true),
                    // 0.16666667f->0.124f
                    new ElementConverter.OutputElement(0.124f, SimHashes.CarbonDioxide,
                        366.5f, false, 
                        false, 0f, 0.5f, 
                        1f, byte.MaxValue, 0, true)
                };
            }
        }
    }
}
