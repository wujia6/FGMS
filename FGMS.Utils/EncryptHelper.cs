using System.Security.Cryptography;
using System.Text;

namespace FGMS.Utils
{
    public class EncryptHelper
    {
        /// <summary>
        /// 加密
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static string DesEncrypt(string inputString)
        {
            return DesEncrypt(inputString, Key);
        }

        /// <summary>
        /// 解密
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        public static string DesDecrypt(string inputString)
        {
            return DesDecrypt(inputString, Key);
        }

        /// <summary>
        /// 密匙
        /// </summary>
        private static string Key
        {
            get
            {
                return "adminpwd";
            }
        }

        /// <summary>
        /// 加密字符串
        /// 注意:密钥必须为８位
        /// </summary>
        /// <param name="strText">字符串</param>
        /// <param name="encryptKey">密钥</param>
        /// <return>返回加密后的字符串</return>
        private static string DesEncrypt(string inputString, string encryptKey)
        {
            byte[] byKey;
            byte[] IV = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
            try
            {
                byKey = Encoding.UTF8.GetBytes(encryptKey.Substring(0, 8));
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                byte[] inputByteArray = Encoding.UTF8.GetBytes(inputString);
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateEncryptor(byKey, IV), CryptoStreamMode.Write);
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();
                return Convert.ToBase64String(ms.ToArray());
            }
            catch (Exception error)
            {
                //return error.Message;
                return null;
            }
        }

        /// <summary>
        /// 解密字符串
        /// </summary>
        /// <param name="inputString">加密的字符串</param>
        /// <param name="decryptKey">密钥</param>
        /// <return>返回解密后的字符串</return>
        private static string DesDecrypt(string inputString, string decryptKey)
        {
            byte[] byKey;
            byte[] IV = { 0x12, 0x34, 0x56, 0x78, 0x90, 0xAB, 0xCD, 0xEF };
            byte[] inputByteArray = new byte[inputString.Length];
            try
            {
                byKey = Encoding.UTF8.GetBytes(decryptKey.Substring(0, 8));
                DESCryptoServiceProvider des = new DESCryptoServiceProvider();
                inputByteArray = Convert.FromBase64String(inputString);
                MemoryStream ms = new MemoryStream();
                CryptoStream cs = new CryptoStream(ms, des.CreateDecryptor(byKey, IV), CryptoStreamMode.Write);
                cs.Write(inputByteArray, 0, inputByteArray.Length);
                cs.FlushFinalBlock();
                Encoding encoding = new UTF8Encoding();
                return encoding.GetString(ms.ToArray());
            }
            catch (Exception error)
            {
                //return error.Message;
                return null;
            }
        }
    }
}
