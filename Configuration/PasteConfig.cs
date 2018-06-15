namespace Teknik.Configuration
{
    public class PasteConfig
    {
        public bool Enabled { get; set; }
        public int UrlLength { get; set; }
        public int KeySize { get; set; }
        public int BlockSize { get; set; }
        public string SyntaxVisualStyle { get; set; }

        public PasteConfig()
        {
            Enabled = true;
            UrlLength = 5;
            KeySize = 256;
            BlockSize = 128;
            SyntaxVisualStyle = "vs";
        }
    }
}
