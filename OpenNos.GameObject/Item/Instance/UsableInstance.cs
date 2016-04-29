using OpenNos.Data;
using System;

namespace OpenNos.GameObject
{
    public class UsableInstance : UsableInstanceDTO, IGameObject
    {
        #region Instantiation

        public UsableInstance(UsableInstanceDTO usableInstance)
        {
            HP = usableInstance.HP;
            MP = usableInstance.MP;
        }

        #endregion

        #region Methods

        public void Save()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}