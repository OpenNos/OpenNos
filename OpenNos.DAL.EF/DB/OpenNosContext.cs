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

using OpenNos.DAL.EF.Entities;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;

namespace OpenNos.DAL.EF.DB
{
    public class OpenNosContext : DbContext
    {
        #region Instantiation

        public OpenNosContext() : base("name=OpenNosContext")
        {
            Configuration.LazyLoadingEnabled = true;

            // --DO NOT DISABLE, otherwise the mapping will fail only one time access to database so
            // no proxy generation needed, its just slowing down in our case
            Configuration.ProxyCreationEnabled = false;
        }

        #endregion

        #region Properties

        public virtual DbSet<Account> Account { get; set; }

        public virtual DbSet<BazaarItem> BazaarItem { get; set; }

        public virtual DbSet<Card> Card { get; set; }

        public virtual DbSet<BCard> BCard { get; set; }

        public virtual DbSet<CellonOption> CellonOption { get; set; }

        public virtual DbSet<Character> Character { get; set; }

        public virtual DbSet<CharacterRelation> CharacterRelation { get; set; }

        public virtual DbSet<CharacterSkill> CharacterSkill { get; set; }

        public virtual DbSet<RollGeneratedItem> RollGeneratedItem { get; set; }

        public virtual DbSet<Combo> Combo { get; set; }

        public virtual DbSet<Drop> Drop { get; set; }

        public virtual DbSet<Family> Family { get; set; }

        public virtual DbSet<FamilyCharacter> FamilyCharacter { get; set; }

        public virtual DbSet<FamilyLog> FamilyLog { get; set; }

        public virtual DbSet<GeneralLog> GeneralLog { get; set; }

        public virtual DbSet<Item> Item { get; set; }

        public virtual DbSet<ItemInstance> ItemInstance { get; set; }

        public virtual DbSet<Mail> Mail { get; set; }

        public virtual DbSet<Map> Map { get; set; }

        public virtual DbSet<MapMonster> MapMonster { get; set; }

        public virtual DbSet<MapNpc> MapNpc { get; set; }

        public virtual DbSet<MapType> MapType { get; set; }

        public virtual DbSet<MapTypeMap> MapTypeMap { get; set; }

        public virtual DbSet<Mate> Mate { get; set; }

        public virtual DbSet<MinilandObject> MinilandObject { get; set; }

        public virtual DbSet<NpcMonster> NpcMonster { get; set; }

        public virtual DbSet<NpcMonsterSkill> NpcMonsterSkill { get; set; }

        public virtual DbSet<PenaltyLog> PenaltyLog { get; set; }

        public virtual DbSet<Portal> Portal { get; set; }

        public virtual DbSet<QuicklistEntry> QuicklistEntry { get; set; }

        public virtual DbSet<Recipe> Recipe { get; set; }

        public virtual DbSet<RecipeItem> RecipeItem { get; set; }

        public virtual DbSet<Respawn> Respawn { get; set; }

        public virtual DbSet<RespawnMapType> RespawnMapType { get; set; }

        public virtual DbSet<ScriptedInstance> ScriptedInstance { get; set; }

        public virtual DbSet<Shop> Shop { get; set; }

        public virtual DbSet<ShopItem> ShopItem { get; set; }

        public virtual DbSet<ShopSkill> ShopSkill { get; set; }

        public virtual DbSet<Skill> Skill { get; set; }

        public virtual DbSet<StaticBonus> StaticBonus { get; set; }

        public virtual DbSet<Teleporter> Teleporter { get; set; }

        public virtual DbSet<StaticBuff> StaticBuff { get; set; }

        #endregion

        #region Methods

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            // remove automatic pluralization
            modelBuilder.Conventions.Remove<PluralizingTableNameConvention>();

            // build TPH tables for inheritance
            modelBuilder.Entity<ItemInstance>()
                 .Map<WearableInstance>(m => m.Requires("WearableInstance"))
                 .Map<SpecialistInstance>(m => m.Requires("SpecialistInstance"))
                 .Map<UsableInstance>(m => m.Requires("UsableInstance"));

            modelBuilder.Entity<Account>()
                .Property(e => e.Password)
                .IsUnicode(false);

            modelBuilder.Entity<Account>()
                .HasMany(e => e.Character)
                .WithRequired(e => e.Account)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Account>()
                .HasMany(e => e.PenaltyLog)
                .WithRequired(e => e.Account)
                .HasForeignKey(e => e.AccountId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Character>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<Character>()
                .HasMany(e => e.Inventory)
                .WithRequired(e => e.Character)
                .HasForeignKey(e => e.CharacterId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Character>()
                .HasMany(e => e.Mate)
                .WithRequired(e => e.Character)
                .HasForeignKey(e => e.CharacterId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Character>()
                .HasMany(e => e.CharacterSkill)
                .WithRequired(e => e.Character)
                .HasForeignKey(e => e.CharacterId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Character>()
                .HasMany(e => e.StaticBonus)
                .WithRequired(e => e.Character)
                .HasForeignKey(e => e.CharacterId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Character>()
                .HasMany(e => e.CharacterRelation1)
                .WithRequired(e => e.Character1)
                .HasForeignKey(e => e.CharacterId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Character>()
                .HasMany(e => e.CharacterRelation2)
                .WithRequired(e => e.Character2)
                .HasForeignKey(e => e.RelatedCharacterId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Character>()
                .HasMany(e => e.StaticBuff)
                .WithRequired(e => e.Character)
                .HasForeignKey(e => e.CharacterId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Card>()
                .HasMany(e => e.StaticBuff)
                .WithRequired(e => e.Card)
                .HasForeignKey(e => e.CardId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Character>()
                .HasMany(e => e.QuicklistEntry)
                .WithRequired(e => e.Character)
                .HasForeignKey(e => e.CharacterId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Character>()
                .HasMany(e => e.Respawn)
                .WithRequired(e => e.Character)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Character>()
                .HasMany(e => e.Mail)
                .WithRequired(e => e.Sender)
                .HasForeignKey(e => e.SenderId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Character>()
                .HasMany(e => e.MinilandObject)
                .WithRequired(e => e.Character)
                .HasForeignKey(e => e.CharacterId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Character>()
                .HasMany(e => e.Mail1)
                .WithRequired(e => e.Receiver)
                .HasForeignKey(e => e.ReceiverId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Family>()
                .HasMany(e => e.FamilyLogs)
                .WithRequired(e => e.Family)
                .HasForeignKey(e => e.FamilyId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<FamilyCharacter>()
                .HasRequired(e => e.Character)
                .WithMany(e => e.FamilyCharacter)
                .HasForeignKey(e => e.CharacterId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<BazaarItem>()
                .HasRequired(e => e.Character)
                .WithMany(e => e.BazaarItem)
                .HasForeignKey(e => e.SellerId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<BazaarItem>()
                .HasRequired(e => e.ItemInstance)
                .WithMany(e => e.BazaarItem)
                .HasForeignKey(e => e.ItemInstanceId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<MinilandObject>()
                .HasOptional(e => e.ItemInstance)
                .WithMany(e => e.MinilandObject)
                .HasForeignKey(e => e.ItemInstanceId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<FamilyCharacter>()
                .HasRequired(e => e.Family)
                .WithMany(e => e.FamilyCharacters)
                .HasForeignKey(e => e.FamilyId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Item>()
                .HasMany(e => e.Drop)
                .WithRequired(e => e.Item)
                .HasForeignKey(e => e.ItemVNum)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Item>()
                .HasMany(e => e.Recipe)
                .WithRequired(e => e.Item)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Item>()
                .HasMany(e => e.RecipeItem)
                .WithRequired(e => e.Item)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Item>()
                .HasMany(e => e.ItemInstances)
                .WithRequired(e => e.Item)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Item>()
                .HasMany(e => e.ShopItem)
                .WithRequired(e => e.Item)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Mail>()
                 .HasOptional(e => e.Item)
                 .WithMany(e => e.Mail)
                 .HasForeignKey(e => e.AttachmentVNum)
                 .WillCascadeOnDelete(false);

            modelBuilder.Entity<RollGeneratedItem>()
               .HasRequired(e => e.OriginalItem)
               .WithMany(e => e.RollGeneratedItem)
               .HasForeignKey(e => e.OriginalItemVNum)
               .WillCascadeOnDelete(false);

            modelBuilder.Entity<RollGeneratedItem>()
               .HasRequired(e => e.ItemGenerated)
               .WithMany(e => e.RollGeneratedItem2)
               .HasForeignKey(e => e.ItemGeneratedVNum)
               .WillCascadeOnDelete(false);

            modelBuilder.Entity<Map>()
                .HasMany(e => e.Character)
                .WithRequired(e => e.Map)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Map>()
                .HasMany(e => e.MapMonster)
                .WithRequired(e => e.Map)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Respawn>()
                .HasRequired(e => e.Map)
                .WithMany(e => e.Respawn)
                .HasForeignKey(e => e.MapId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Respawn>()
                .HasRequired(e => e.RespawnMapType)
                .WithMany(e => e.Respawn)
                .HasForeignKey(e => e.RespawnMapTypeId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<RespawnMapType>()
                .HasRequired(e => e.Map)
                .WithMany(e => e.RespawnMapType)
                .HasForeignKey(e => e.DefaultMapId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<MapType>()
                .HasOptional(e => e.RespawnMapType)
                .WithMany(e => e.MapTypes)
                .HasForeignKey(e => e.RespawnMapTypeId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<MapType>()
                .HasOptional(e => e.ReturnMapType)
                .WithMany(e => e.MapTypes1)
                .HasForeignKey(e => e.ReturnMapTypeId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Map>()
                .HasMany(e => e.MapNpc)
                .WithRequired(e => e.Map)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Map>()
                .HasMany(e => e.Portal)
                .WithRequired(e => e.Map)
                .HasForeignKey(e => e.DestinationMapId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Map>()
                .HasMany(e => e.Portal1)
                .WithRequired(e => e.Map1)
                .HasForeignKey(e => e.SourceMapId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Map>()
               .HasMany(e => e.ScriptedInstance)
               .WithRequired(e => e.Map)
               .HasForeignKey(e => e.MapId)
               .WillCascadeOnDelete(false);

            modelBuilder.Entity<Map>()
                .HasMany(e => e.Teleporter)
                .WithRequired(e => e.Map)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<BCard>()
             .HasOptional(e => e.Skill)
              .WithMany(e => e.BCards)
              .HasForeignKey(e => e.SkillVNum)
              .WillCascadeOnDelete(false);

            modelBuilder.Entity<BCard>()
            .HasOptional(e => e.NpcMonster)
             .WithMany(e => e.BCards)
             .HasForeignKey(e => e.NpcMonsterVNum)
             .WillCascadeOnDelete(false);

            modelBuilder.Entity<BCard>()
                .HasOptional(e => e.Card)
                .WithMany(e => e.BCards)
                .HasForeignKey(e => e.CardId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<BCard>()
                .HasOptional(e => e.Item)
                 .WithMany(e => e.BCards)
                 .HasForeignKey(e => e.ItemVNum)
                 .WillCascadeOnDelete(false);

            modelBuilder.Entity<MapTypeMap>()
                .HasRequired(e => e.Map)
                .WithMany(e => e.MapTypeMap)
                .HasForeignKey(e => e.MapId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<MapTypeMap>()
                .HasRequired(e => e.MapType)
                .WithMany(e => e.MapTypeMap)
                .HasForeignKey(e => e.MapTypeId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<MapType>()
                .HasMany(e => e.Drops)
                .WithOptional(e => e.MapType)
                .HasForeignKey(e => e.MapTypeId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<MapNpc>()
                .HasMany(e => e.Recipe)
                .WithRequired(e => e.MapNpc)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<MapNpc>()
                .HasMany(e => e.Shop)
                .WithRequired(e => e.MapNpc)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<MapNpc>()
                .HasMany(e => e.Teleporter)
                .WithRequired(e => e.MapNpc)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<NpcMonster>()
                .HasMany(e => e.Drop)
                .WithOptional(e => e.NpcMonster)
                .HasForeignKey(e => e.MonsterVNum)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<NpcMonster>()
                .HasMany(e => e.Mate)
                .WithRequired(e => e.NpcMonster)
                .HasForeignKey(e => e.NpcMonsterVNum)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<NpcMonster>()
                .HasMany(e => e.MapMonster)
                .WithRequired(e => e.NpcMonster)
                .HasForeignKey(e => e.MonsterVNum)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<NpcMonster>()
                .HasMany(e => e.MapNpc)
                .WithRequired(e => e.NpcMonster)
                .HasForeignKey(e => e.NpcVNum)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<NpcMonster>()
                .HasMany(e => e.NpcMonsterSkill)
                .WithRequired(e => e.NpcMonster)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Recipe>()
                .HasMany(e => e.RecipeItem)
                .WithRequired(e => e.Recipe)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Shop>()
                .HasMany(e => e.ShopItem)
                .WithRequired(e => e.Shop)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Shop>()
                .HasMany(e => e.ShopSkill)
                .WithRequired(e => e.Shop)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Skill>()
                .HasMany(e => e.CharacterSkill)
                .WithRequired(e => e.Skill)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Skill>()
                .HasMany(e => e.Combo)
                .WithRequired(e => e.Skill)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Skill>()
                .HasMany(e => e.NpcMonsterSkill)
                .WithRequired(e => e.Skill)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Skill>()
                .HasMany(e => e.ShopSkill)
                .WithRequired(e => e.Skill)
                .WillCascadeOnDelete(false);
        }

        #endregion
    }
}