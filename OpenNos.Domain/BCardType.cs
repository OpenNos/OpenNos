/*
 * This file is part of the OpenNos Emulator Project. See AUTHORS file for Copyright information
 *
 * This program is free software; you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation; either version 2 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 */

namespace OpenNos.Domain
{
    public class BCardType
    {
        #region Enums

        public enum CardType : byte
        {
            //1-20

            SpecialAttack = 1,
            SpecialDefence = 2,
            AttackPowerChange = 3,
            ChangeTarget = 4,
            ChangeCritical = 5,
            SpecialCritical = 6,
            ChangeElement = 7,
            IncreaseDamage = 8,
            ChangeDefence = 9,
            ChangeDodgeAndDefencePercent = 10,
            ChangeBlock = 11,
            ChangeAbsorption = 12,
            ChangeElementResistance = 13,
            EnemyElementResistance = 14,
            ChangeDamage = 15,
            GuarantedDodgeRangedAttack = 16,
            ChangeMorale = 17,
            Casting = 18,
            Move = 19,
            Reflection = 20,

            //21-40

            DrainAndSteal = 21,
            HealingBurningAndCasting = 22,
            ChangeHPMP = 23,
            SpecialisationBuffResistance = 24,
            Buff = 25,
            Summons = 26,
            SpecialEffects = 27,
            Capture = 28,
            SpecialDamageAndExplosions = 29,
            SpecialEffects2 = 30,
            CalculatingLevel = 31,
            Recovery = 32,
            MaxHPMP = 33,
            MultAttack = 34,
            MultDefence = 35,
            TimeCircleSkills = 36,
            RecoveryAndDamagePercent = 37,
            Count = 38,
            NoDefeatAndNoDamage = 39,
            SpecialActions = 40,

            //41-60

            ChangeMode = 41,
            NoCharacteristicValue = 42,
            LightAndShadow = 43,
            Item = 44,
            DebuffResistance = 45,
            SpecialBehaviour = 46,
            Quest = 47,
            SecondSPCard = 48,
            SPCardUpgrade = 49,
            HugeSnowman = 50,
            Drain = 51,
            BossMonstersSkill = 52,
            LordHatus = 53,
            LordCalvinas = 54,
            SESpecialist = 55,
            FourthGlacernonFamilyRaid = 56,
            SummonedMonsterAttack = 57,
            BearSpirit = 58,
            SummonSkill = 59,
            InflictSkill = 60,

            //61-80

            Missingno = 61,
            HideBarrelSkill = 62,
            FocusEnemyOnMeSkill = 63,
            TauntSkill = 64,
            FireCannoneerRangeBuff = 65,
            VulcanoElementBuff = 66,
            DamageConvertingSkill = 67,
            MeditationSkill = 68,
            FalconSkill = 69,
            AbsorptionAndPowerSkill = 70,
            LeonaPassiveSkill = 71,
            FearSkill = 72,
            SniperAttack = 73,
            FrozenDebuff = 74,
            JumpBackPush = 75,
            FairyXPIncrease = 76,
            SummonAndRecoverHP = 77,
            TeamArenaBuff = 78,
            ArenaCamera = 79,
            DarkCloneSummon = 80,

            //81-??

            AbsorbedSpirit = 81,
            AngerSkill = 82,
            MeteoriteTeleport = 83,
            StealBuff = 84,
        }
    }

    //update to meet BCardType
    // /*
    public class AdditionalTypes : BCardType
    {
        //1-20
        private enum SpecialAttack : byte
        {
            AllIcreased = 11,
            AllDecreased = 12,
            MeleeIncreased = 21,
            MeleeDecreased = 22,
            RangedIncreased = 31,
            RangedDecreased = 32,
            MagicalIncreased = 41,
            MagicalDecreased = 42,
        }

        private enum SpecialDefence : byte
        {
        }

        private enum AttackPowerChange : byte
        {
        }

        private enum ChangeTarget : byte
        {
        }

        private enum ChangeCritical : byte
        {
        }

        private enum SpecialCritical : byte
        {
        }

        private enum ChangeElement : byte
        {
        }

        private enum IncreaseDamage : byte
        {
        }

        private enum ChangeDefence : byte
        {
        }

        private enum ChangeDodgeAndDefencePercent : byte
        {
        }

        private enum ChangeBlock : byte
        {
        }

        private enum ChangeAbsorption : byte
        {
        }

        private enum ChangeElementResistance : byte
        {
        }

        private enum EnemyElementResistance : byte
        {
        }

        private enum ChangeDamage : byte
        {
        }

        private enum GuarantedDodgeRangedAttack : byte
        {
        }

        private enum ChangeMorale : byte
        {
        }

        private enum Casting : byte
        {
        }

        private enum Move : byte
        {
        }

        private enum Reflection : byte
        {
        }

        //21-40
        private enum DrainAndSteal : byte
        {
        }

        private enum HealingBurningAndCasting : byte
        {
        }

        private enum ChangeHPMP : byte
        {
        }

        private enum SpecialisationBuffResistance : byte
        {
        }

        private enum Buff : byte
        {
        }

        private enum Summons : byte
        {
        }

        private enum SpecialEffects : byte
        {
        }

        private enum Capture : byte
        {
        }

        private enum SpecialDamageAndExplosions : byte
        {
        }

        private enum SpecialEffects2 : byte
        {
        }

        private enum CalculatingLevel : byte
        {
        }

        private enum Recovery : byte
        {
        }

        private enum MaxHPMP : byte
        {
        }

        private enum MultAttack : byte
        {
        }

        private enum MultDefence : byte
        {
        }

        private enum TimeCircleSkills : byte
        {
        }

        private enum RecoveryAndDamagePercent : byte
        {
        }

        private enum Count : byte
        {
        }

        private enum NoDefeatAndNoDamage : byte
        {
        }

        private enum SpecialActions : byte
        {
        }

        //41-60
        private enum ChangeMode : byte
        {
        }

        private enum NoCharacteristicValue : byte
        {
        }

        private enum LightAndShadow : byte
        {
        }

        private enum Item : byte
        {
        }

        private enum DebuffResistance : byte
        {
        }

        private enum SpecialBehaviour : byte
        {
        }

        private enum Quest : byte
        {
        }

        private enum SecondSPCard : byte
        {
        }

        private enum SPCardUpgrade : byte
        {
        }

        private enum HugeSnowman : byte
        {
        }

        private enum Drain : byte
        {
        }

        private enum BossMonstersSkill : byte
        {
        }

        private enum LordHatus : byte
        {
        }

        private enum LordCalvinas : byte
        {
        }

        private enum SESpecialist : byte
        {
        }

        private enum FourthGlacernonFamilyRaid : byte
        {
        }

        private enum SummonedMonsterAttack : byte
        {
        }

        private enum BearSpirit : byte
        {
        }

        private enum SummonSkill : byte
        {
        }

        private enum InflictSkill : byte
        {
        }

        //61-80
        private enum Missingno : byte
        {
        }

        private enum HideBarrelSkill : byte
        {
        }

        private enum FocusEnemyOnMeSkill : byte
        {
        }

        private enum TauntSkill : byte
        {
        }

        private enum FireCannoneerRangeBuff : byte
        {
        }

        private enum VulcanoElementBuff : byte
        {
        }

        private enum DamageConvertingSkill : byte
        {
        }

        private enum MeditationSkill : byte
        {
        }

        private enum FalconSkill : byte
        {
        }

        private enum AbsorptionAndPowerSkill : byte
        {
        }

        private enum LeonaPassiveSkill : byte
        {
        }

        private enum FearSkill : byte
        {
        }

        private enum SniperAttack : byte
        {
        }

        private enum FrozenDebuff : byte
        {
        }

        private enum JumpBackPush : byte
        {
        }

        private enum FairyXPIncrease : byte
        {
        }

        private enum SummonAndRecoverHP : byte
        {
        }

        private enum TeamArenaBuff : byte
        {
        }

        private enum ArenaCamera : byte
        {
        }

        private enum DarkCloneSummon : byte
        {
        }

        //81-??
        private enum AbsorbedSpirit : byte
        {
        }

        private enum AngerSkill : byte
        {
        }

        private enum MeteoriteTeleport : byte
        {
        }

        private enum StealBuff : byte
        {
        }

        //end!
    }

    // */

    #endregion
}