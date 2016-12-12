using OpenNos.Core;
using System.Text;

namespace OpenNos.Test
{
    public class TestEncryption : EncryptionBase
    {
        #region Instantiation

        public TestEncryption() : base(true)
        {
        }

        public TestEncryption(bool hasCustomParameter) : base(hasCustomParameter)
        {
        }

        #endregion

        #region Methods

        public override string Decrypt(byte[] data, int sessionId = 0)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            return encoding.GetString(data);
        }

        public override string DecryptCustomParameter(byte[] data)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            return encoding.GetString(data);
        }

        public override byte[] Encrypt(string data)
        {
            UTF8Encoding encoding = new UTF8Encoding();
            return encoding.GetBytes(data);
        }

        #endregion
    }
}