using System;
using System.Collections.Generic;
using System.Linq;
using OpenNos.Data;
using OpenNos.Domain;

namespace OpenNos.GameObject.Helpers
{
    public class CellonGeneratorHelper
    {
        private readonly Dictionary<int, Dictionary<CellonType, CellonGenerator>> _generatorDictionary =
            new Dictionary<int, Dictionary<CellonType, CellonGenerator>>
            {
                {
                    1,
                    new Dictionary<CellonType, CellonGenerator>
                    {
                        {CellonType.Hp, new CellonGenerator {Min = 30, Max = 100}},
                        {CellonType.Mp, new CellonGenerator {Min = 50, Max = 120}},
                        {CellonType.HpRecovery, new CellonGenerator {Min = 5, Max = 10}},
                        {CellonType.MpRecovery, new CellonGenerator {Min = 8, Max = 15}}
                    }
                },
                {
                    2, new Dictionary<CellonType, CellonGenerator>
                    {
                        {CellonType.Hp, new CellonGenerator {Min = 120, Max = 200}},
                        {CellonType.Mp, new CellonGenerator {Min = 150, Max = 250}},
                        {CellonType.HpRecovery, new CellonGenerator {Min = 14, Max = 20}},
                        {CellonType.MpRecovery, new CellonGenerator {Min = 16, Max = 25}}
                    }
                },
                {
                    3, new Dictionary<CellonType, CellonGenerator>
                    {
                        {CellonType.Hp, new CellonGenerator {Min = 220, Max = 330}},
                        {CellonType.Mp, new CellonGenerator {Min = 280, Max = 330}},
                        {CellonType.HpRecovery, new CellonGenerator {Min = 22, Max = 28}},
                        {CellonType.MpRecovery, new CellonGenerator {Min = 16, Max = 25}}
                    }
                },
                {
                    4, new Dictionary<CellonType, CellonGenerator>
                    {
                        {CellonType.Hp, new CellonGenerator {Min = 330, Max = 400}},
                        {CellonType.Mp, new CellonGenerator {Min = 350, Max = 420}},
                        {CellonType.HpRecovery, new CellonGenerator {Min = 30, Max = 38}},
                        {CellonType.MpRecovery, new CellonGenerator {Min = 36, Max = 45}}
                    }
                },
                {
                    5, new Dictionary<CellonType, CellonGenerator>
                    {
                        {CellonType.Hp, new CellonGenerator {Min = 430, Max = 550}},
                        {CellonType.Mp, new CellonGenerator {Min = 550, Max = 550}},
                        {CellonType.HpRecovery, new CellonGenerator {Min = 40, Max = 50}},
                        {CellonType.MpRecovery, new CellonGenerator {Min = 50, Max = 60}}
                    }
                },
                {
                    6, new Dictionary<CellonType, CellonGenerator>
                    {
                        {CellonType.Hp, new CellonGenerator {Min = 600, Max = 750}},
                        {CellonType.Mp, new CellonGenerator {Min = 600, Max = 750}},
                        {CellonType.HpRecovery, new CellonGenerator {Min = 55, Max = 70}},
                        {CellonType.MpRecovery, new CellonGenerator {Min = 65, Max = 80}},
                        {CellonType.CriticalDamageDecrease, new CellonGenerator {Min = 21, Max = 35}}
                    }
                },
                {
                    7, new Dictionary<CellonType, CellonGenerator>
                    {
                        {CellonType.Hp, new CellonGenerator {Min = 800, Max = 1000}},
                        {CellonType.Mp, new CellonGenerator {Min = 800, Max = 1000}},
                        {CellonType.HpRecovery, new CellonGenerator {Min = 75, Max = 90}},
                        {CellonType.MpRecovery, new CellonGenerator {Min = 75, Max = 90}},
                        {CellonType.CriticalDamageDecrease, new CellonGenerator {Min = 11, Max = 20}}
                    }
                },
                {
                    8, new Dictionary<CellonType, CellonGenerator>
                    {
                        {CellonType.Hp, new CellonGenerator {Min = 1000, Max = 1300}},
                        {CellonType.Mp, new CellonGenerator {Min = 1000, Max = 1300}},
                        {CellonType.HpRecovery, new CellonGenerator {Min = 100, Max = 120}},
                        {CellonType.MpRecovery, new CellonGenerator {Min = 100, Max = 120}},
                        {CellonType.MpConsumption, new CellonGenerator {Min = 13, Max = 17}},
                        {CellonType.CriticalDamageDecrease, new CellonGenerator {Min = 21, Max = 35}}
                    }
                },
                {
                    9, new Dictionary<CellonType, CellonGenerator>
                    {
                        {CellonType.Hp, new CellonGenerator {Min = 1100, Max = 1500}},
                        {CellonType.Mp, new CellonGenerator {Min = 1100, Max = 1500}},
                        {CellonType.HpRecovery, new CellonGenerator {Min = 110, Max = 135}},
                        {CellonType.MpRecovery, new CellonGenerator {Min = 110, Max = 135}},
                        {CellonType.MpConsumption, new CellonGenerator {Min = 14, Max = 21}},
                        {CellonType.CriticalDamageDecrease, new CellonGenerator {Min = 22, Max = 45}}
                    }
                }
            };

        public EquipmentOptionDTO GenerateOption(int itemEffectValue)
        {
            if (new Random().Next(100) > 50)
            {
                return null;
            }
            Dictionary<CellonType, CellonGenerator> dictionary = _generatorDictionary[itemEffectValue];
            Dictionary<CellonType, CellonGenerator>.ValueCollection list = dictionary.Values;
            EquipmentOptionDTO result = new EquipmentOptionDTO();
            int rand = new Random().Next(list.Count * 2);
            for (int i = 0; i < list.Count; i++)
            {
                if (i != rand)
                {
                    continue;
                }
                result.Value = new Random().Next(list.ElementAt(i).Min, list.ElementAt(i).Max);
                result.Level = (byte) itemEffectValue;
                result.Type = (byte) i;
                return result;
            }
            return null;
        }

        private class CellonGenerator
        {
            public int Min { get; set; }
            public int Max { get; set; }
        }

        #region Singleton

        private static CellonGeneratorHelper _instance;

        public static CellonGeneratorHelper Instance
        {
            get { return _instance ?? (_instance = new CellonGeneratorHelper()); }
        }

        #endregion
    }
}