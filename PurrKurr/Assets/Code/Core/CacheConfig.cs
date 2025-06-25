using System.Text;

namespace Code.Core
{
    public static class CacheConfig
    {
        public const string CacheFilePath = "PurrCache.json";
        // Note: This should not be publicly visible
        public static readonly byte[] EncryptionKey = Encoding.UTF8.GetBytes("16ByteSecretKey!");
        public static readonly byte[] IV = Encoding.UTF8.GetBytes("16ByteInitVector");
    }
}
