using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Data;
using OpenNos.Domain;

namespace OpenNos.GameObject.Helpers
{
    public static class EquipmentOptionHelper
    {

        #region Cellons

        public static List<BCard> CellonToBCards(List<EquipmentOptionDTO> optionList, short itemVnum)
        {
            return optionList.Select(option => CellonToBcard(option, itemVnum)).ToList();
        }

        private static BCard CellonToBcard(EquipmentOptionDTO option, short itemVnum)
        {
            BCard bcard = new BCard();
            switch (option.Type)
            {
                case (byte) CellonType.Hp:
                    bcard.Type = (byte) BCardType.CardType.MaxHPMP;
                    bcard.SubType = (byte) AdditionalTypes.MaxHPMP.IncreasesMaximumHP;
                    bcard.FirstData = option.Value;
                    break;
                case (byte) CellonType.Mp:
                    bcard.Type = (byte) BCardType.CardType.MaxHPMP;
                    bcard.SubType = (byte) AdditionalTypes.MaxHPMP.IncreasesMaximumMP;
                    bcard.FirstData = option.Value;
                    break;
                case (byte) CellonType.HpRecovery:
                    bcard.Type = (byte) BCardType.CardType.Recovery;
                    bcard.SubType = (byte) AdditionalTypes.Recovery.HPRecoveryIncreased;
                    bcard.FirstData = option.Value;
                    break;
                case (byte) CellonType.MpRecovery:
                    bcard.Type = (byte) BCardType.CardType.Recovery;
                    bcard.SubType = (byte) AdditionalTypes.Recovery.MPRecoveryIncreased;
                    bcard.FirstData = option.Value;
                    break;
                case (byte) CellonType.MpConsumption:
                    //TODO FIND Correct Bcard or settle in the code.
                    break;
                case (byte) CellonType.CriticalDamageDecrease:
                    bcard.Type = (byte) BCardType.CardType.Critical;
                    bcard.SubType = (byte) AdditionalTypes.Critical.DamageFromCriticalDecreased;
                    bcard.FirstData = option.Value;
                    break;
            }
            bcard.ItemVNum = itemVnum;
            return bcard;
        }

        #endregion
        
        public static List<BCard> ShellToBCards(List<EquipmentOptionDTO> optionList, short itemVnum)
        {
            return optionList.Select(option => ShellToBCards(option, itemVnum)).ToList();
        }

        public static BCard ShellToBCards(EquipmentOptionDTO option, short itemVNum)
        {
            BCard bCard = new BCard();

            switch ((ShellOptionType) option.Type)
            {
                case ShellOptionType.IncreaseDamage:
                    break;
                case ShellOptionType.SDamagePercentage:
                    break;
                case ShellOptionType.MinorBleeding:
                    break;
                case ShellOptionType.Bleeding:
                    break;
                case ShellOptionType.SeriousBleeding:
                    break;
                case ShellOptionType.Blackout:
                    break;
                case ShellOptionType.Frozen:
                    break;
                case ShellOptionType.DeadlyBlackout:
                    break;
                case ShellOptionType.IncreaseDamageOnPlants:
                    break;
                case ShellOptionType.IncreaseDamageOnAnimals:
                    break;
                case ShellOptionType.IncreaseDamageOnDemons:
                    break;
                case ShellOptionType.IncreaseDamagesOnZombies:
                    break;
                case ShellOptionType.IncreaseDamagesOnSmallAnimals:
                    break;
                case ShellOptionType.SDamagePercentageOnGiantMonsters:
                    break;
                case ShellOptionType.IncreaseCritChance:
                    break;
                case ShellOptionType.IncreaseCritDamages:
                    break;
                case ShellOptionType.ProtectWandSkillInterruption:
                    break;
                case ShellOptionType.IncreaseFireElement:
                    break;
                case ShellOptionType.IncreaseWaterElement:
                    break;
                case ShellOptionType.IncreaseLightElement:
                    break;
                case ShellOptionType.IncreaseDarknessElement:
                    break;
                case ShellOptionType.SIncreaseAllElements:
                    break;
                case ShellOptionType.ReduceMpConsumption:
                    break;
                case ShellOptionType.HpRegenerationOnKill:
                    break;
                case ShellOptionType.MpRegenerationOnKill:
                    break;
                case ShellOptionType.AttackSl:
                    break;
                case ShellOptionType.DefenseSl:
                    break;
                case ShellOptionType.ElementSl:
                    break;
                case ShellOptionType.HpMpSl:
                    break;
                case ShellOptionType.SGlobalSl:
                    break;
                case ShellOptionType.GoldPercentage:
                    break;
                case ShellOptionType.XpPercentage:
                    break;
                case ShellOptionType.JobXpPercentage:
                    break;
                case ShellOptionType.PvpDamagePercentage:
                    break;
                case ShellOptionType.PvpEnemyDefenseDecreased:
                    break;
                case ShellOptionType.PvpResistanceDecreasedFire:
                    break;
                case ShellOptionType.PvpResistanceDecreasedWater:
                    break;
                case ShellOptionType.PvpResistanceDecreasedLight:
                    break;
                case ShellOptionType.PvpResistanceDecreasedDark:
                    break;
                case ShellOptionType.PvpResistanceDecreasedAll:
                    break;
                case ShellOptionType.PvpAlwaysHit:
                    break;
                case ShellOptionType.PvpDamageProbabilityPercentage:
                    break;
                case ShellOptionType.PvpWithdrawMp:
                    break;
                case ShellOptionType.PvpIgnoreResistanceFire:
                    break;
                case ShellOptionType.PvpIgnoreResistanceWater:
                    break;
                case ShellOptionType.PvpIgnoreResistanceLight:
                    break;
                case ShellOptionType.PvpIgnoreResistanceDark:
                    break;
                case ShellOptionType.RegenSpecialistPointPerKill:
                    break;
                case ShellOptionType.IncreasePrecision:
                    break;
                case ShellOptionType.IncreaseConcentration:
                    break;
                case ShellOptionType.CloseCombatDefense:
                    break;
                case ShellOptionType.LongRangeDefense:
                    break;
                case ShellOptionType.MagicalDefense:
                    break;
                case ShellOptionType.SDefenseAllPercentage:
                    break;
                case ShellOptionType.ReducedMinorBleeding:
                    break;
                case ShellOptionType.ReducedSeriousBleeding:
                    break;
                case ShellOptionType.ReducedAllBleeding:
                    break;
                case ShellOptionType.ReducedSmallBlackout:
                    break;
                case ShellOptionType.ReducedAllBlackout:
                    break;
                case ShellOptionType.ReducedHandOfDeath:
                    break;
                case ShellOptionType.ReducedFrozenChance:
                    break;
                case ShellOptionType.ReducedBlindChance:
                    break;
                case ShellOptionType.ReducedArrestationChance:
                    break;
                case ShellOptionType.ReducedDefenseReduction:
                    break;
                case ShellOptionType.ReducedShockChance:
                    break;
                case ShellOptionType.ReducedRigidityChance:
                    break;
                case ShellOptionType.SReducedAllNegative:
                    break;
                case ShellOptionType.OnRestHpRecoveryPercentage:
                    break;
                case ShellOptionType.NaturalHpRecoveryPercentage:
                    break;
                case ShellOptionType.OnRestMpRecoveryPercentage:
                    break;
                case ShellOptionType.NaturalMpRecoveryPercentage:
                    break;
                case ShellOptionType.SOnAttackRecoveryPercentage:
                    break;
                case ShellOptionType.ReduceCriticalChance:
                    break;
                case ShellOptionType.FireResistanceIncrease:
                    break;
                case ShellOptionType.WaterResistanceIncrease:
                    break;
                case ShellOptionType.LightResistanceIncrease:
                    break;
                case ShellOptionType.DarkResistanceIncrease:
                    break;
                case ShellOptionType.SIncreaseAllResistance:
                    break;
                case ShellOptionType.DignityLossReduced:
                    break;
                case ShellOptionType.PointConsumptionReduced:
                    break;
                case ShellOptionType.MiniGameProductionIncreased:
                    break;
                case ShellOptionType.FoodHealing:
                    break;
                case ShellOptionType.PvpDefensePercentage:
                    break;
                case ShellOptionType.PvpDodgeClose:
                    break;
                case ShellOptionType.PvpDodgeRanged:
                    break;
                case ShellOptionType.PvpDodgeMagic:
                    break;
                case ShellOptionType.SPvpDodgeAll:
                    break;
                case ShellOptionType.PvpMpProtect:
                    break;
                case ShellOptionType.ChampionPvpIgnoreAttackFire:
                    break;
                case ShellOptionType.ChampionPvpIgnoreAttackWater:
                    break;
                case ShellOptionType.ChampionPvpIgnoreAttackLight:
                    break;
                case ShellOptionType.ChampionPvpIgnoreAttackDark:
                    break;
                case ShellOptionType.AbsorbDamagePercentageA:
                    break;
                case ShellOptionType.AbsorbDamagePercentageB:
                    break;
                case ShellOptionType.AbsormDamagePercentageC:
                    break;
                case ShellOptionType.IncreaseEvasiveness:
                    break;
            }
            return bCard;
        }
    }
}