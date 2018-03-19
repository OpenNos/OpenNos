using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenNos.Core;
using OpenNos.Data;
using OpenNos.Domain;

namespace OpenNos.GameObject.Helpers
{
    public class EquipmentOptionHelper : Singleton<EquipmentOptionHelper>
    {
        #region Cellons

        public List<BCard> CellonToBCards(List<EquipmentOptionDTO> optionList, short itemVnum)
        {
            return optionList.Select(option => CellonToBcard(option, itemVnum)).ToList();
        }

        private BCard CellonToBcard(EquipmentOptionDTO option, short itemVnum)
        {
            BCard bcard = new BCard();
            switch (option.Type)
            {
                case (byte) CellonType.Hp:
                    bcard.Type = (byte) BCardType.CardType.MaxHPMP;
                    bcard.SubType = (byte) AdditionalTypes.MaxHPMP.MaximumHPIncreased;
                    bcard.FirstData = option.Value;
                    break;
                case (byte) CellonType.Mp:
                    bcard.Type = (byte) BCardType.CardType.MaxHPMP;
                    bcard.SubType = (byte) AdditionalTypes.MaxHPMP.MaximumMPIncreased;
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

        #region Shells

        public List<BCard> ShellToBCards(List<EquipmentOptionDTO> optionList, short itemVnum)
        {
            return optionList.Select(option => ShellToBCards(option, itemVnum)).ToList();
        }

        public BCard ShellToBCards(EquipmentOptionDTO option, short itemVNum)
        {
            BCard bCard = new BCard();

            switch ((ShellOptionType) option.Type)
            {
                case ShellOptionType.IncreaseDamage:
                    bCard.Type = (byte)BCardType.CardType.MultAttack;
                    bCard.SubType = (byte)AdditionalTypes.MultAttack.AllAttackIncreased;
                    bCard.FirstData = option.Value;
                    break;
                case ShellOptionType.SDamagePercentage:
                    bCard.Type = (byte)BCardType.CardType.Damage;
                    bCard.SubType = (byte)AdditionalTypes.Damage.DamageIncreased;
                    bCard.FirstData = option.Value;
                    break;
                case ShellOptionType.MinorBleeding:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.Bleeding:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.SeriousBleeding:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.Blackout:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.Frozen:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.DeadlyBlackout:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.IncreaseDamageOnPlants:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.IncreaseDamageOnAnimals:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.IncreaseDamageOnDemons:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.IncreaseDamagesOnZombies:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.IncreaseDamagesOnSmallAnimals:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.SDamagePercentageOnGiantMonsters:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.IncreaseCritChance:
                    bCard.Type = (byte)BCardType.CardType.Critical;
                    bCard.SubType = (byte)AdditionalTypes.Critical.InflictingIncreased;
                    bCard.FirstData = option.Value;
                    break;
                case ShellOptionType.IncreaseCritDamages:
                    bCard.Type = (byte)BCardType.CardType.Critical;
                    bCard.SubType = (byte)AdditionalTypes.Critical.DamageFromCriticalIncreased;
                    bCard.FirstData = option.Value;
                    break;
                case ShellOptionType.ProtectWandSkillInterruption:
                    bCard.Type = (byte)BCardType.CardType.Casting;
                    bCard.SubType = (byte)AdditionalTypes.Casting.InterruptCastingNegated;
                    bCard.FirstData = option.Value;
                    break;
                case ShellOptionType.IncreaseFireElement:
                    bCard.Type = (byte)BCardType.CardType.Element;
                    bCard.SubType = (byte)AdditionalTypes.Element.FireIncreased;
                    bCard.FirstData = option.Value;
                    break;
                case ShellOptionType.IncreaseWaterElement:
                    bCard.Type = (byte)BCardType.CardType.Element;
                    bCard.SubType = (byte)AdditionalTypes.Element.WaterIncreased;
                    bCard.FirstData = option.Value;
                    break;
                case ShellOptionType.IncreaseLightElement:
                    bCard.Type = (byte)BCardType.CardType.Element;
                    bCard.SubType = (byte)AdditionalTypes.Element.LightIncreased;
                    bCard.FirstData = option.Value;
                    break;
                case ShellOptionType.IncreaseDarknessElement:
                    bCard.Type = (byte)BCardType.CardType.Element;
                    bCard.SubType = (byte)AdditionalTypes.Element.DarkIncreased;
                    bCard.FirstData = option.Value;
                    break;
                case ShellOptionType.SIncreaseAllElements:
                    bCard.Type = (byte)BCardType.CardType.Element;
                    bCard.SubType = (byte)AdditionalTypes.Element.AllIncreased;
                    bCard.FirstData = option.Value;
                    break;
                case ShellOptionType.ReduceMpConsumption:
                    /* 
                     * TO BE DONE 
                     * CAN'T FIND CORRESPONDING BCARD
                     */
                    break;
                case ShellOptionType.HpRegenerationOnKill:
                    bCard.Type = (byte)BCardType.CardType.RecoveryAndDamagePercent;
                    bCard.SubType = (byte)AdditionalTypes.RecoveryAndDamagePercent.HPRecovered;
                    bCard.FirstData = option.Value;
                    break;
                case ShellOptionType.MpRegenerationOnKill:
                    bCard.Type = (byte)BCardType.CardType.RecoveryAndDamagePercent;
                    bCard.SubType = (byte)AdditionalTypes.RecoveryAndDamagePercent.MPRecovered;
                    bCard.FirstData = option.Value;
                    break;
                case ShellOptionType.AttackSl:
                    bCard.Type = (byte)BCardType.CardType.SPSL;
                    bCard.SubType = (byte)AdditionalTypes.SPSL.Attack;
                    bCard.FirstData = option.Value;
                    break;
                case ShellOptionType.DefenseSl:
                    bCard.Type = (byte)BCardType.CardType.SPSL;
                    bCard.SubType = (byte)AdditionalTypes.SPSL.Defense;
                    bCard.FirstData = option.Value;
                    break;
                case ShellOptionType.ElementSl:
                    bCard.Type = (byte)BCardType.CardType.SPSL;
                    bCard.SubType = (byte)AdditionalTypes.SPSL.Element;
                    bCard.FirstData = option.Value;
                    break;
                case ShellOptionType.HpMpSl:
                    bCard.Type = (byte)BCardType.CardType.SPSL;
                    bCard.SubType = (byte)AdditionalTypes.SPSL.HPMP;
                    bCard.FirstData = option.Value;
                    break;
                case ShellOptionType.SGlobalSl:
                    bCard.Type = (byte)BCardType.CardType.SPSL;
                    bCard.SubType = (byte)AdditionalTypes.SPSL.All;
                    bCard.FirstData = option.Value;
                    break;
                case ShellOptionType.GoldPercentage:
                    bCard.Type = (byte)BCardType.CardType.Item;
                    bCard.SubType = (byte)AdditionalTypes.Item.IncreaseEarnedGold;
                    bCard.FirstData = option.Value;
                    break;
                case ShellOptionType.XpPercentage:
                    bCard.Type = (byte)BCardType.CardType.Item;
                    bCard.SubType = (byte)AdditionalTypes.Item.EXPIncreased;
                    bCard.FirstData = option.Value;
                    break;
                case ShellOptionType.JobXpPercentage:
                    /* 
                     * TO BE DONE 
                     * CAN'T FIND BCARD
                     */
                    break;
                case ShellOptionType.PvpDamagePercentage:
                    bCard.Type = (byte)BCardType.CardType.SpecialisationBuffResistance;
                    bCard.SubType = (byte)AdditionalTypes.SpecialisationBuffResistance.IncreaseDamageInPVP;
                    bCard.FirstData = option.Value;
                    break;
                case ShellOptionType.PvpEnemyDefenseDecreased:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.PvpResistanceDecreasedFire:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.PvpResistanceDecreasedWater:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.PvpResistanceDecreasedLight:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.PvpResistanceDecreasedDark:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.PvpResistanceDecreasedAll:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.PvpAlwaysHit:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.PvpDamageProbabilityPercentage:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.PvpWithdrawMp:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.PvpIgnoreResistanceFire:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.PvpIgnoreResistanceWater:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.PvpIgnoreResistanceLight:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.PvpIgnoreResistanceDark:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.RegenSpecialistPointPerKill:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.IncreasePrecision:
                    bCard.Type = (byte)BCardType.CardType.Target;
                    bCard.SubType = (byte)AdditionalTypes.Target.AllHitRateIncreased;
                    bCard.FirstData = option.Value;
                    break;
                case ShellOptionType.IncreaseConcentration:
                    bCard.Type = (byte)BCardType.CardType.Target;
                    bCard.SubType = (byte)AdditionalTypes.Target.MagicalConcentrationIncreased;
                    bCard.FirstData = option.Value;
                    break;
                case ShellOptionType.CloseCombatDefense:
                    bCard.Type = (byte)BCardType.CardType.MultDefence;
                    bCard.SubType = (byte)AdditionalTypes.MultDefence.MeleeDefenceIncreased;
                    bCard.FirstData = option.Value;
                    break;
                case ShellOptionType.LongRangeDefense:
                    bCard.Type = (byte)BCardType.CardType.MultDefence;
                    bCard.SubType = (byte)AdditionalTypes.MultDefence.RangedDefenceIncreased;
                    bCard.FirstData = option.Value;
                    break;
                case ShellOptionType.MagicalDefense:
                    bCard.Type = (byte)BCardType.CardType.MultDefence;
                    bCard.SubType = (byte)AdditionalTypes.MultDefence.MagicalDefenceIncreased;
                    bCard.FirstData = option.Value;
                    break;
                case ShellOptionType.SDefenseAllPercentage:
                    bCard.Type = (byte)BCardType.CardType.Defence;
                    bCard.SubType = (byte)AdditionalTypes.MultDefence.AllDefenceIncreased;
                    bCard.FirstData = option.Value;
                    break;
                case ShellOptionType.ReducedMinorBleeding:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.ReducedSeriousBleeding:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.ReducedAllBleeding:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.ReducedSmallBlackout:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.ReducedAllBlackout:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.ReducedHandOfDeath:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.ReducedFrozenChance:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.ReducedBlindChance:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.ReducedArrestationChance:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.ReducedDefenseReduction:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.ReducedShockChance:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.ReducedRigidityChance:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.SReducedAllNegative:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.OnRestHpRecoveryPercentage:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.NaturalHpRecoveryPercentage:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.OnRestMpRecoveryPercentage:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.NaturalMpRecoveryPercentage:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.SOnAttackRecoveryPercentage:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.ReduceCriticalChance:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.FireResistanceIncrease:
                    bCard.Type = (byte)BCardType.CardType.ElementResistance;
                    bCard.SubType = (byte)AdditionalTypes.ElementResistance.FireIncreased;
                    bCard.FirstData = option.Value;
                    break;
                case ShellOptionType.WaterResistanceIncrease:
                    bCard.Type = (byte)BCardType.CardType.ElementResistance;
                    bCard.SubType = (byte)AdditionalTypes.ElementResistance.WaterIncreased;
                    bCard.FirstData = option.Value;
                    break;
                case ShellOptionType.LightResistanceIncrease:
                    bCard.Type = (byte)BCardType.CardType.ElementResistance;
                    bCard.SubType = (byte)AdditionalTypes.ElementResistance.LightIncreased;
                    bCard.FirstData = option.Value;
                    break;
                case ShellOptionType.DarkResistanceIncrease:
                    bCard.Type = (byte)BCardType.CardType.ElementResistance;
                    bCard.SubType = (byte)AdditionalTypes.ElementResistance.DarkIncreased;
                    bCard.FirstData = option.Value;
                    break;
                case ShellOptionType.SIncreaseAllResistance:
                    bCard.Type = (byte)BCardType.CardType.ElementResistance;
                    bCard.SubType = (byte)AdditionalTypes.ElementResistance.AllIncreased;
                    bCard.FirstData = option.Value;
                    break;
                case ShellOptionType.DignityLossReduced:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.PointConsumptionReduced:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.MiniGameProductionIncreased:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.FoodHealing:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.PvpDefensePercentage:
                    bCard.Type = (byte)BCardType.CardType.SpecialisationBuffResistance;
                    bCard.SubType = (byte)AdditionalTypes.SpecialisationBuffResistance.DecreaseDamageInPVP;
                    bCard.FirstData = option.Value;
                    break;
                case ShellOptionType.PvpDodgeClose:
                    bCard.Type = (byte)BCardType.CardType.DodgeAndDefencePercent;
                    bCard.SubType = (byte)AdditionalTypes.DodgeAndDefencePercent.DodgingMeleeIncreased;
                    bCard.FirstData = option.Value;
                    break;
                case ShellOptionType.PvpDodgeRanged:
                    bCard.Type = (byte)BCardType.CardType.DodgeAndDefencePercent;
                    bCard.SubType = (byte)AdditionalTypes.DodgeAndDefencePercent.DodgingRangedIncreased;
                    bCard.FirstData = option.Value;
                    break;
                case ShellOptionType.PvpDodgeMagic:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.SPvpDodgeAll:
                    bCard.Type = (byte)BCardType.CardType.DodgeAndDefencePercent;
                    bCard.SubType = (byte)AdditionalTypes.DodgeAndDefencePercent.DodgeIncreased;
                    bCard.FirstData = option.Value;
                    break;
                case ShellOptionType.PvpMpProtect:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.ChampionPvpIgnoreAttackFire:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.ChampionPvpIgnoreAttackWater:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.ChampionPvpIgnoreAttackLight:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.ChampionPvpIgnoreAttackDark:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.AbsorbDamagePercentageA:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.AbsorbDamagePercentageB:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.AbsorbDamagePercentageC:
                    /* TO BE DONE */
                    break;
                case ShellOptionType.IncreaseEvasiveness:
                    /* TO BE DONE */
                    break;
            }
            return bCard;
        }
        #endregion
    }
}