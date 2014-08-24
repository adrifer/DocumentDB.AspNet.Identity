using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DocumentDB.AspNet.Identity
{
    class IdentityUser : IUser
    {
        public string Id { get; set; }

        public string UserName { get; set; }

        /// <summary>
        /// Gets the logins.
        /// </summary>
        /// <value>The logins.</value>
        public virtual List<UserLoginInfo> Logins { get; private set; }
    }
}
