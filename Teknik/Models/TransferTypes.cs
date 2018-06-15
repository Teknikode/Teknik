using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using Teknik.Areas.Users.Models;
using Teknik.Areas.Paste.Models;

namespace Teknik.Models
{
    public enum TransferTypes
    {
        Sha256Password = 0,
        CaseSensitivePassword = 1,
        ASCIIPassword = 2
    }

    public class TransferType
    {
        public int TransferTypeId { get; set; }

        public TransferTypes Type { get; set; }

        public virtual int? UserId { get; set; }

        public virtual User User { get; set; }

        public virtual int? PasteId { get; set; }

        public virtual Paste Paste { get; set; }
    }
}