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

        public virtual ICollection<User> Users { get; set; }

        public virtual ICollection<Paste> Pastes { get; set; }
    }
}