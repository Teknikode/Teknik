using System;
using System.Text.Json.Serialization;
using Teknik.Utilities.Attributes;

namespace Teknik.IdentityServer.Models
{
    public class AuthToken
    {
        public Guid AuthTokenId { get; set; }
        
        public string Name { get; set; }

        public DateTime? LastUsed { get; set; }

        [JsonIgnore]
        public string ApplicationUserId { get; set; }


        [JsonIgnore]
        public virtual ApplicationUser ApplicationUser { get; set; }

        [JsonIgnore]
        [CaseSensitive]
        public string Token { get; set; }
    }
}
