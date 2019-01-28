using System.IO;

namespace Teknik.Configuration
{
    public class PasteConfig
    {
        public bool Enabled { get; set; }
        public int UrlLength { get; set; }
        public int DeleteKeyLength { get; set; }
        public string SyntaxVisualStyle { get; set; }
        // Location of the upload directory
        public string PasteDirectory { get; set; }
        // File Extension for saved files
        public string FileExtension { get; set; }
        public int KeySize { get; set; }
        public int BlockSize { get; set; }
        // The size of the chunk that the file will be encrypted/decrypted in (bytes)
        public int ChunkSize { get; set; }

        public PasteConfig()
        {
            Enabled = true;
            UrlLength = 5;
            DeleteKeyLength = 24;
            KeySize = 256;
            BlockSize = 128;
            ChunkSize = 1040;
            PasteDirectory = Directory.GetCurrentDirectory();
            FileExtension = "enc";
            SyntaxVisualStyle = "vs";
        }
    }
}
