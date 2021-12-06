using System;
using Teknik.Utilities.Attributes;

namespace Teknik.Areas.Users.Models
{
    public class AuthToken
    {
        public string AuthTokenId { get; set; }

        public string Name { get; set; }

        public string Token { get; set; }

        public DateTime? LastUsed { get; set; }
    }
}
