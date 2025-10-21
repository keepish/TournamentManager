using System.Security.Cryptography;
using System.Text;

namespace TournamentManager.Core.Services
{
    public class SecureStorage : ISecureStorage
    {
        private readonly byte[] _key;
        private readonly byte[] _iv;

        public SecureStorage()
        {
            using var deriveBytes = new Rfc2898DeriveBytes("4qg380-ehgr0-38gh0sigdh24gh80",
                Encoding.UTF8.GetBytes("alkfj;4dsaehtbivgu25"),
                10012,
                HashAlgorithmName.SHA256);
            _key = deriveBytes.GetBytes(32);
            _iv = deriveBytes.GetBytes(16);
        }

        public bool Contains(string key)
        {
            string filePath = GetFilePath(key);
            return File.Exists(filePath);
        }

        public string Load(string key)
        {
            try
            {
                string filePath = GetFilePath(key);
                if (!File.Exists(filePath))
                    return null;

                string base64Encrypted = File.ReadAllText(filePath);
                byte[] encryptedData = Convert.FromBase64String(base64Encrypted);

                using var aes = Aes.Create();
                aes.Key = _key;
                aes.IV = _iv;

                using var decryptor = aes.CreateDecryptor();
                using var ms = new MemoryStream(encryptedData);
                using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
                using var sr = new StreamReader(cs);

                return sr.ReadToEnd();
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки данных: {ex.Message}");
                return null;
            }
        }

        public void Remove(string key)
        {
            try
            {
                string filePath = GetFilePath(key);
                if (File.Exists(filePath))
                    File.Delete(filePath);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка удаления данных: {ex.Message}");
            }
        }

        public void Save(string key, string value)
        {
            try
            {
                using var aes = Aes.Create();
                aes.Key = _key;
                aes.IV = _iv;

                using var encryptor = aes.CreateEncryptor();
                using var ms = new MemoryStream();
                using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                using (var sw = new StreamWriter(cs))
                {
                    sw.Write(value);
                }

                var encryptedData = ms.ToArray();
                string base64Encrypted = Convert.ToBase64String(encryptedData);

                string filePath = GetFilePath(key);

                File.WriteAllText(filePath, base64Encrypted);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения данных: {ex.Message}");
            }
        }

        private string GetFilePath(string key)
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string folder = Path.Combine(appData, "TournamentManager");
            Directory.CreateDirectory(folder);

            using var sha256 = SHA256.Create();
            byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(key));
            string fileName = Convert.ToBase64String(hash)
                .Replace("/", "_")
                .Replace("+", "-")
                .Substring(0, 16);

            return Path.Combine(folder, $"{fileName}.dat");
        }
    }
}
