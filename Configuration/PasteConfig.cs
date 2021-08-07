using System.IO;

namespace Teknik.Configuration
{
    public class PasteConfig
    {
        public bool Enabled { get; set; }
        public int UrlLength { get; set; }
        public int DeleteKeyLength { get; set; }
        public string SyntaxVisualStyle { get; set; }
        public int KeySize { get; set; }
        public int BlockSize { get; set; }
        // The size of the chunk that the file will be encrypted/decrypted in (bytes)
        public int ChunkSize { get; set; }
        // Storage settings
        public StorageConfig StorageConfig { get; set; }

        public PasteConfig()
        {
            Enabled = true;
            UrlLength = 5;
            DeleteKeyLength = 24;
            KeySize = 256;
            BlockSize = 128;
            ChunkSize = 1040;
            SyntaxVisualStyle = "vs";
            StorageConfig = new StorageConfig("pastes");
        }
    }
}
