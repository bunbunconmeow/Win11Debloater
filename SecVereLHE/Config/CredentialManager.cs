using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace SecVerseLHE.Config
{
    public class UserCredentials
    {
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
    }

    internal class CredentialManager
    {
        private static readonly string ConfigPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "login.dat");
        private static readonly byte[] Entropy = { 9, 1, 1, 4, 2, 0 };

        public static void SaveCredentials(string user, string pass)
        {
            var data = new UserCredentials { Username = user ?? "", Password = pass ?? "" };
            string json = JsonConvert.SerializeObject(data);
            byte[] plainBytes = Encoding.UTF8.GetBytes(json);


            byte[] encryptedBytes = DpapiNative.Protect(plainBytes, Entropy);

            File.WriteAllBytes(ConfigPath, encryptedBytes);
        }

        public static UserCredentials LoadCredentials()
        {
            if (!File.Exists(ConfigPath)) return null;

            try
            {
                byte[] encryptedBytes = File.ReadAllBytes(ConfigPath);
                byte[] plainBytes = DpapiNative.Unprotect(encryptedBytes, Entropy);
                string json = Encoding.UTF8.GetString(plainBytes);
                return JsonConvert.DeserializeObject<UserCredentials>(json);
            }
            catch
            {
                return null; 
            }
        }
    }
}
