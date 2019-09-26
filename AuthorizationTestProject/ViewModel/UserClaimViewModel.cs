using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AuthorizationTestProject.ViewModel
{
    public class UserClaimViewModel
    {
        public UserClaimViewModel()
        {
            Cliams = new List<UserCliam>();
        }

        public string UserId { get; set; }
        public List<UserCliam> Cliams { get; set; }
    }
}
