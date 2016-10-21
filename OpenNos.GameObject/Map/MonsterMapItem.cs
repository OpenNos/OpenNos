using OpenNos.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.GameObject
{
    public class MonsterMapItem : MapItem
    {

        public MonsterMapItem(short x, short y, short itemVNum, int amount = 1, long owner = -1) : base(x, y)
        {
            ItemVNum = itemVNum;
            if(amount < 100)
            {
                Amount = (byte)amount;
            }
            GoldAmount = amount;
            Owner = owner;
        }

        public override short ItemVNum { get; set; } 

        public override byte Amount { get; set; }

        public int GoldAmount { get; set; }

        public long? Owner { get; set; }

        public override ItemInstance GetItemInstance()
        {
            if(_itemInstance == null)
            {
                _itemInstance = Inventory.InstantiateItemInstance(ItemVNum, Owner.Value, Amount);
            }

            return _itemInstance;
        }

        public void Rarify(ClientSession session)
        {
            ItemInstance instance = GetItemInstance();
            if(instance.Item.EquipmentSlot == EquipmentType.Armor || instance.Item.EquipmentSlot == EquipmentType.MainWeapon 
                || instance.Item.EquipmentSlot == EquipmentType.SecondaryWeapon)
            {
                if (instance is WearableInstance)
                {
                    ((WearableInstance)instance).RarifyItem(session, RarifyMode.Drop, RarifyProtection.None);
                }
            }
        }
    }
}
