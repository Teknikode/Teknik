using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;
using Teknik.Attributes;

namespace Teknik.Areas.Users.Models
{
    public class AuthToken
    {
        public int AuthTokenId { get; set; }

        public int UserId { get; set; }

        public virtual User User { get; set; }

        public string Name { get; set; }

        [CaseSensitive]
        public string HashedToken { get; set; }

        public DateTime? LastDateUsed { get; set; }
    }
}