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

namespace OpenNos.DAL.EF.MySQL.DB
{
    using MySql.Data.Entity;
    using System.Data.Entity;
    using System.Data.Entity.ModelConfiguration.Conventions;

    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public partial class OpenNosContext : DbContext
    {
        #region Instantiation

        public OpenNosContext() : base("name=OpenNosContext")
        {
            this.Configuration.LazyLoadingEnabled = true;

            // --DO NOT DISABLE, otherwise the mapping will fail
            // only one time access to database so no proxy generation needed, its just slowing down in our case
            this.Configuration.ProxyCreationEnabled = false;
        }

        #endregion

        #region Properties

        public virtual DbSet<Account> Account { get; set; }

        public virtual DbSet<CellonOption> CellonOption { get; set; }

        public virtual DbSet<Character> Character { get; set; }

        public virtual DbSet<CharacterSkill> CharacterSkill { get; set; }

        public virtual DbSet<Combo> Combo { get; set; }

        public virtual DbSet<Drop> Drop { get; set; }

        public virtual DbSet<GeneralLog> GeneralLog { get; set; }

        public virtual DbSet<Inventory> Inventory { get; set; }

        public virtual DbSet<Item> Item { get; set; }

        public virtual DbSet<ItemInstance> ItemInstance { get; set; }

        public virtual DbSet<Mail> Mail { get; set; }

        public virtual DbSet<Map> Map { get; set; }

        public virtual DbSet<MapMonster> MapMonster { get; set; }

        public virtual DbSet<MapNpc> MapNpc { get; set; }

        public virtual DbSet<MapType> MapType { get; set; }

        public virtual DbSet<MapTypeMap> MapTypeMap { get; set; }

        public virtual DbSet<NpcMonster> NpcMonster { get; set; }

        public virtual DbSet<NpcMonsterSkill> NpcMonsterSkill { get; set; }

        public virtual DbSet<PenaltyLog> PenaltyLog { get; set; }

        public virtual DbSet<Portal> Portal { get; set; }

        public virtual DbSet<QuicklistEntry> QuicklistEntry { get; set; }

        public virtual DbSet<Recipe> Recipe { get; set; }

        public virtual DbSet<RecipeItem> RecipeItem { get; set; }

        public virtual DbSet<Respawn> Respawn { get; set; }

        public virtual DbSet<Shop> Shop { get; set; }

        public virtual DbSet<ShopItem> ShopItem { get; set; }

        public virtual DbSet<ShopSkill> ShopSkill { get; set; }

        public virtual DbSet<Skill> Skill { get; set; }

        public virtual DbSet<Teleporter> Teleporter { get; set; }

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

            modelBuilder.Entity<ItemInstance>()
               .HasOptional(ii => ii.Inventory)
               .WithRequired(inv => inv.ItemInstance);

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

            modelBuilder.Entity<Account>()
                .HasMany(e => e.GeneralLog)
                .WithRequired(e => e.Account)
                .HasForeignKey(e => e.AccountId)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Character>()
                .Property(e => e.Name)
                .IsUnicode(false);

            modelBuilder.Entity<Character>()
                .HasMany(e => e.CharacterSkill)
                .WithRequired(e => e.Character)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Character>()
                .HasMany(e => e.Inventory)
                .WithRequired(e => e.Character)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Character>()
                .HasMany(e => e.QuicklistEntry)
                .WithRequired(e => e.Character)
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
                .HasMany(e => e.Mail1)
                .WithRequired(e => e.Receiver)
                .HasForeignKey(e => e.ReceiverId)
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
                .HasMany(e => e.ShopItem)
                .WithRequired(e => e.Item)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Mail>()
                 .HasOptional(e => e.Item)
                 .WithMany(e => e.Mail)
                 .HasForeignKey(e => e.ItemVNum)
                 .WillCascadeOnDelete(false);

            modelBuilder.Entity<Map>()
                .HasMany(e => e.Character)
                .WithRequired(e => e.Map)
                .WillCascadeOnDelete(false);

            modelBuilder.Entity<Map>()
                .HasMany(e => e.MapMonster)
                .WithRequired(e => e.Map)
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
                .HasMany(e => e.Teleporter)
                .WithRequired(e => e.Map)
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