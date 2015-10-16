using System;
using System.Collections;
using System.Text;
using System.Security.Cryptography;
using System.Linq;

namespace OpenNos.Core
{
    public class LoginEncryption : EncryptionBase
    {
        public static string sha256(string inputString)
        {
            using (SHA256 hash = SHA256Managed.Create())
            {
                return String.Join("", hash
                  .ComputeHash(Encoding.UTF8.GetBytes(inputString))
                  .Select(item => item.ToString("x2")));
            }
        }
        public override string Decrypt(byte[] data, int size)
        {
            string result;
            try
            {
                for (int i = 0; i < size; i++)
                {
                    data[i] = (byte)(data[i] - 15 ^ 195);
                }
                result = Encoding.ASCII.GetString(data).Substring(0, size);
            }
            catch
            {
                result = String.Empty;
            }
            return result;
        }

        public override byte[] Encrypt(string data)
        {
            byte[] result;
            try
            {
                byte[] array = new byte[data.Length + 1];
                array = Encoding.ASCII.GetBytes(data);
                for (int i = 0; i < data.Length; i++)
                {
                    array[i] = Convert.ToByte((int)(data[i] + '\u000f'));
                }
                array[array.Length - 1] = 25;
                result = array;
            }
            catch
            {
                result = new byte[0];
            }
            return result;
        }

        public static string GetPassword(string passcrypt)
        {
            bool equal = passcrypt.Length % 2 == 0 ? true : false;
            string str = equal == true ? passcrypt.Remove(0, 3) : passcrypt.Remove(0, 4);
           string decpass = string.Empty;
            for (int i = 0; i<str.Length; i += 2) decpass += str[i];

            if (decpass.Length % 2 != 0)
            {
                str = decpass = string.Empty;
                str = passcrypt.Remove(0, 2);
                for (int i = 0; i<str.Length; i += 2) decpass += str[i];
            }

            StringBuilder temp = new StringBuilder();
            for (int i = 0; i<decpass.Length; i += 2) temp.Append(Convert.ToChar(Convert.ToUInt32(decpass.Substring(i, 2), 16)));
            decpass = temp.ToString();

            return decpass;
        }
    }
}
