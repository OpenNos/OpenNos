using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNos.Core
{
    public abstract class EncryptionBase
    {
        #region Methods

        public abstract string Decrypt(byte[] data, int size);

        public abstract byte[] Encrypt(string data);

        #endregion
    }
}
