namespace Console.MicroBase
{
    using System.Collections.Generic;
    using System.Runtime.CompilerServices;
    using System.Security.Cryptography;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    public interface IEntity
    {
        int Id { get; set; }
    }

    public class Table<T> where T : IEntity
    {
        private readonly ReaderWriterLockSlim _lock = new();
        public List<T> Rows { get; set; } = new();
        public int NextId { get; set; } = 1;

        public int Count() => Rows.Count;

        public T Insert(T entity)
        {
            _lock.EnterWriteLock();

            try
            {
                entity.Id = NextId++;
                Rows.Add(entity);
                return entity;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool Update(T entity)
        {
            _lock.EnterWriteLock();

            try
            {
                var index = Rows.FindIndex(x => x.Id == entity.Id);
                if (index == -1)
                {
                    return false;
                }

                Rows[index] = entity;
                return true;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool Delete(int id)
        {
            _lock.EnterWriteLock();

            try
            {
                var entity = Rows.FirstOrDefault(x => x.Id == id);
                if (entity == null)
                {
                    return false;
                }

                Rows.Remove(entity);
                return true;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool IsRow(Func<T, bool> predicate)
        {
            return Rows.Any(predicate);
        }


        public T Get(Func<T, bool> predicate)
        {
            _lock.EnterWriteLock();

            try
            {
                return (T)Rows.Where(predicate).FirstOrDefault();
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public IReadOnlyList<T> GetAll() => Rows;

        public T? GetById(int id) => Rows.FirstOrDefault(x => x.Id == id);
    }

    public class TableData
    {
        public string TypeName { get; set; } = "";
        public string JsonData { get; set; } = "";
    }

    public class MicroBase
    {
        private readonly ReaderWriterLockSlim _lock = new();
        public Dictionary<string, TableData> Tables { get; set; } = new();

        public Table<T> CreateTable<T>(string name) where T : IEntity
        {
            this._lock.EnterWriteLock();

            try
            {
                var table = new Table<T>();
                Tables[name] = new TableData
                {
                    TypeName = typeof(T).AssemblyQualifiedName!,
                    JsonData = JsonSerializer.Serialize(table)
                };

                return table;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public bool TableExists(string name)
        {
            return Tables.ContainsKey(name);
        }

        public IReadOnlyCollection<string> GetTableNames()
        {
            _lock.EnterReadLock();
            try
            {
                return Tables.Keys.ToList();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        public Table<T> GetTable<T>(string name) where T : IEntity
        {
            _lock.EnterWriteLock();

            try
            {
                var tableData = Tables[name];
                return JsonSerializer.Deserialize<Table<T>>(tableData.JsonData)!;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void UpdateTable<T>(string name, Table<T> table) where T : IEntity
        {
            _lock.EnterWriteLock();

            try
            {
                Tables[name].JsonData = JsonSerializer.Serialize(table);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }

    public static class DatabaseFile
    {
        private static readonly SemaphoreSlim _fileLock = new(1, 1);

        static DatabaseFile()
        {
            JsonOption = new JsonSerializerOptions
            {
                WriteIndented = true
            };
        }

        private static JsonSerializerOptions JsonOption { get; set; }

        private static string FilePath { get; set; }

        private static bool ExistDatabase()
        {
            return File.Exists(FilePath);
        }

        public static void Save(MicroBase db, string filePath, string password)
        {
            try
            {
                var json = JsonSerializer.Serialize(db, JsonOption);

                var encrypted = CryptoService.Encrypt(json, password);
                File.WriteAllBytes(filePath, encrypted);
            }
            catch (Exception ex)
            {
                string errorText = ex.Message;
                throw;
            }
        }

        public static async Task SaveAsync(MicroBase db, string filePath, string password)
        {
            await _fileLock.WaitAsync();
            try
            {
                var json = JsonSerializer.Serialize(db);
                var encrypted = CryptoService.Encrypt(json, password);
                await File.WriteAllBytesAsync(filePath, encrypted);
            }
            finally
            {
                _fileLock.Release();
            }
        }

        public static MicroBase Load(string filePath, string password)
        {
            try
            {
                var encrypted = File.ReadAllBytes(filePath);
                var json = CryptoService.Decrypt(encrypted, password);

                return JsonSerializer.Deserialize<MicroBase>(json)!;

            }
            catch (Exception ex)
            {
                string errorText = ex.Message;
                throw;
            }
        }

        public static async Task<MicroBase> LoadAsync(string filePath, string password)
        {
            await _fileLock.WaitAsync();
            try
            {
                var encrypted = await File.ReadAllBytesAsync(filePath);
                var json = CryptoService.Decrypt(encrypted, password);
                return JsonSerializer.Deserialize<MicroBase>(json)!;
            }
            finally
            {
                _fileLock.Release();
            }
        }
    }

    public static class CryptoService
    {
        private const int SaltSize = 16;
        private const int NonceSize = 12;
        private const int TagSize = 16;
        private const int KeySize = 32;

        public static byte[] Encrypt(string plainText, string password)
        {
            byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
            byte[] nonce = RandomNumberGenerator.GetBytes(NonceSize);

            byte[] key = DeriveKey(password, salt);

            byte[] plaintextBytes = Encoding.UTF8.GetBytes(plainText);
            byte[] ciphertext = new byte[plaintextBytes.Length];
            byte[] tag = new byte[TagSize];

            using var aes = new AesGcm(key);
            aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);

            var result = new byte[
                salt.Length + nonce.Length + ciphertext.Length + tag.Length];

            int offset = 0;
            Buffer.BlockCopy(salt, 0, result, offset, salt.Length);
            offset += salt.Length;

            Buffer.BlockCopy(nonce, 0, result, offset, nonce.Length);
            offset += nonce.Length;

            Buffer.BlockCopy(ciphertext, 0, result, offset, ciphertext.Length);
            offset += ciphertext.Length;

            Buffer.BlockCopy(tag, 0, result, offset, tag.Length);

            return result;
        }

        public static string Decrypt(byte[] encryptedData, string password)
        {
            byte[] salt = encryptedData[..SaltSize];
            byte[] nonce = encryptedData[SaltSize..(SaltSize + NonceSize)];
            byte[] tag = encryptedData[^TagSize..];
            byte[] ciphertext = encryptedData[
                (SaltSize + NonceSize)..^TagSize];

            byte[] key = DeriveKey(password, salt);
            byte[] plaintext = new byte[ciphertext.Length];

            using var aes = new AesGcm(key);
            aes.Decrypt(nonce, ciphertext, tag, plaintext);

            return Encoding.UTF8.GetString(plaintext);
        }

        // 🔑 Moderne Key-Derivation ohne PBKDF2
        private static byte[] DeriveKey(string password, byte[] salt)
        {
            using var hmac = new HMACSHA256(salt);
            byte[] hash = hmac.ComputeHash(
                Encoding.UTF8.GetBytes(password));

            return hash[..KeySize];
        }
    }
}
