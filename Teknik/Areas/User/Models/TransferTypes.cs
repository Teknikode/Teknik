using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace Teknik.Areas.Users.Models
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

        public List<User> Users { get; set; }
    }
}