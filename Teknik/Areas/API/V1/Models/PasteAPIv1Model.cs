using Teknik.Utilities;

namespace Teknik.Areas.API.V1.Models
{
    public class PasteAPIv1Model : BaseAPIv1Model
    {
        public string code { get; set; }

        public string title { get; set; }

        public string syntax { get; set; }

        public ExpirationUnit expireUnit { get; set; }

        public int expireLength { get; set; }

        public string password { get; set; }

        public PasteAPIv1Model()
        {
            code = null;
            title = string.Empty;
            syntax = "text";
            expireUnit = ExpirationUnit.Never;
            expireLength = 1;
            password = string.Empty;
        }
    }
}