using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using Teknik.Areas.Users.Models;
using Teknik.Areas.Users.Utility;
using Teknik.Data;
using Teknik.Models;
using Teknik.Utilities;

namespace Teknik.Security
{
    public class TeknikPrincipal : ITeknikPrincipal
    {
        private IIdentity _Identity;
        public IIdentity Identity
        {
            get
            {
                return this._Identity;
            }
        }

        private User m_Info;
        public User Info
        {
            get
            {
                if (m_Info == null && Identity != null && Identity.IsAuthenticated)
                {
                    //TeknikEntities db = new TeknikEntities();
                    //m_Info = UserHelper.GetUser(db, Identity.Name);
                }
                return m_Info;
            }
        }

        public TeknikPrincipal(string username)
        {
            this._Identity = new GenericIdentity(username, "Forms");
        }

        public bool IsInRole(string role)
        {
            return UserHelper.UserHasRoles(Info, role);
        }
    }
}
