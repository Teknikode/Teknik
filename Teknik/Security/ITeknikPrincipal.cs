using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using Teknik.Areas.Users.Models;

namespace Teknik.Security
{
    public interface ITeknikPrincipal : IPrincipal
    {
        User Info { get; }
    }
}