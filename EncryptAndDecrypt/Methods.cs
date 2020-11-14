using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Private_Note.EncryptAndDecrypt
{
    public class Methods
    {
        public static string Key = "abc@@defg";

        public static string Encrypt(String password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return "";
            }
            password += Key;
            var passwordInBytes = Encoding.UTF8.GetBytes(password);
            return Convert.ToBase64String(passwordInBytes);
        }

        public static string Decrypt(String encryptedPassword)
        {
            if (string.IsNullOrEmpty(encryptedPassword))
            {
                return "";
            }
            var base64EncodeBytes = Convert.FromBase64String(encryptedPassword);
            var result = Encoding.UTF8.GetString(base64EncodeBytes);
            result = result.Substring(0, result.Length - Key.Length);
            return result;
        }
    }
}
