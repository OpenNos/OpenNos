using System;
using System.Collections;
using System.Text;
using System.Security.Cryptography;
using System.Linq;

namespace OpenNos.Core
{
    public static class Encryption
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

        public static string LoginDecrypt(byte[] tmp, int size)
        {
            string result;
            try
            {
                for (int i = 0; i < size; i++)
                {
                    tmp[i] = (byte)(tmp[i] - 15 ^ 195);
                }
                result = Encoding.ASCII.GetString(tmp).Substring(0, size);
            }
            catch
            {
                result = "error";
            }
            return result;
        }
        public static byte[] LoginEncrypt(string str)
        {
            byte[] result;
            try
            {
                byte[] array = new byte[str.Length + 1];
                array = Encoding.ASCII.GetBytes(str);
                for (int i = 0; i < str.Length; i++)
                {
                    array[i] = Convert.ToByte((int)(str[i] + '\u000f'));
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
        public static string GetPassword(string pswd)
        {
            int length = pswd.Length;
            bool flag = length % 2 != 0? false:true;
            checked
            {
                string result;
                try
                {
                    bool flag2 = flag;
                    if (flag2)
                    {
                        string text = pswd.Remove(0, 3);
                        ArrayList arrayList = new ArrayList();
                        arrayList.AddRange(text.ToCharArray());
                        string text2 = "";
                        int temp = 0;
                        int num = text.Length - 1;
                        int num2 = temp;
                        while (true)
                        {
                            int temp2 = num2;
                            int num3 = num;
                            if (temp2 > num3)
                            {
                                break;
                            }
                            text2 = String.Format("{0}{1}", text2, arrayList[num2]);
                            num2 += 2;
                        }
                        ArrayList arrayList2 = new ArrayList();
                        arrayList2.AddRange(text2.ToCharArray());
                        string text3 = "";
                        int temp3 = 0;
                        int num4 = text2.Length - 1;
                        int num5 = temp3;
                        while (true)
                        {
                            int temp4 = num5;
                            int num3 = num4;
                            if (temp4 > num3)
                            {
                                break;
                            }
                            text3 += Convert.ToChar(Convert.ToUInt32(String.Format("{0}{1}",arrayList2[num5], arrayList2[num5 + 1]), 16)).ToString();
                            num5 += 2;
                        }
                        result = text3;
                    }
                    else
                    {
                        string text4 = pswd.Remove(0, 4);
                        ArrayList arrayList3 = new ArrayList();
                        arrayList3.AddRange(text4.ToCharArray());
                        string text5 = "";
                        int temp5 = 0;
                        int num6 = text4.Length - 1;
                        int num7 = temp5;
                        while (true)
                        {
                            int temp6 = num7;
                            int num3 = num6;
                            if (temp6 > num3)
                            {
                                break;
                            }
                            text5 = String.Format("{0}{1}", text5, arrayList3[num7]);
                            num7 += 2;
                        }
                        ArrayList arrayList4 = new ArrayList();
                        arrayList4.AddRange(text5.ToCharArray());
                        int temp7 = 0;
                        int num8 = text5.Length - 1;
                        int num9 = temp7;
                        string text6 = "";
                        while (true)
                        {
                            int temp8 = num9;
                            int num3 = num8;
                            if (temp8 > num3)
                            {
                                break;
                            }
                            text6 += Convert.ToChar(Convert.ToUInt32(String.Format("{0}{1}", arrayList4[num9], arrayList4[num9 + 1]), 16).ToString());
                            num9 += 2;
                        }
                        result = text6;
                    }
                }
                catch (Exception ex)
                {
                    result = "Error";
                }
                return result;

            }
        }
    }
}
